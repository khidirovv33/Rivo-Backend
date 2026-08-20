using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Inventories.Dtos;
using Rivo.Application.Inventories.Interfaces;
using Rivo.Application.InventoryItems.Dtos;
using Rivo.Application.Notifications.Interfaces;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Domain.Entities.Inventories;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Inventories.Services;

public class InventoriesService : IInventoriesService
{
    /// <summary>Порог "крупной недостачи" (§16 ТЗ) для триггера LargeShortage-уведомления. Не вынесено
    /// в настройки tenant'а, т.к. Settings ещё не реализован ни одним из разработчиков.</summary>
    private const decimal LargeShortageCostThreshold = 500m;

    private readonly IApplicationDbContext _context;
    private readonly IStockMovementsService _stockMovements;
    private readonly INotificationsService _notifications;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public InventoriesService(
        IApplicationDbContext context,
        IStockMovementsService stockMovements,
        INotificationsService notifications,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _context = context;
        _stockMovements = stockMovements;
        _notifications = notifications;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PaginatedList<InventoryDto>> GetAllAsync(
        PagedRequest request, Guid? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _context.Inventories.AsNoTracking().Include(x => x.Items).AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        query = request.SortDescending ? query.OrderByDescending(x => x.StartedAt) : query.OrderBy(x => x.StartedAt);

        var mapped = query.Select(x => ToDto(x));
        return await PaginatedList<InventoryDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<InventoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await FindAsync(id, cancellationToken);
        return ToDto(inventory);
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto, CancellationToken cancellationToken = default)
    {
        var inventoryNumber = await GenerateInventoryNumberAsync(cancellationToken);
        var userId = _currentUser.UserId ?? throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["ResponsibleUserId"] = ["Не удалось определить текущего пользователя."],
        });

        var inventory = new Inventory
        {
            WarehouseId = dto.WarehouseId,
            InventoryNumber = inventoryNumber,
            Status = InventoryStatus.Draft,
            ResponsibleUserId = userId,
            StartedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            CreatedBy = userId,
        };

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", nameof(Inventory), inventory.Id.ToString(), newValue: inventoryNumber, cancellationToken: cancellationToken);

        return ToDto(inventory);
    }

    public async Task<InventoryDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await FindAsync(id, cancellationToken);
        EnsureStatus(inventory, InventoryStatus.Draft, InventoryStatus.Completed);

        inventory.Status = InventoryStatus.Completed;
        inventory.CompletedAt = DateTime.UtcNow;
        inventory.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Complete", nameof(Inventory), inventory.Id.ToString(), cancellationToken: cancellationToken);

        return ToDto(inventory);
    }

    public async Task<InventoryDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await FindAsync(id, cancellationToken);
        EnsureStatus(inventory, InventoryStatus.Completed, InventoryStatus.Approved);

        foreach (var item in inventory.Items.Where(i => i.Difference != 0))
        {
            await _stockMovements.CreateAsync(new CreateStockMovementDto
            {
                WarehouseId = inventory.WarehouseId,
                ProductId = item.ProductId,
                ProductVariationId = item.ProductVariationId,
                Type = StockMovementType.Adjustment,
                Quantity = item.Difference,
                Reason = $"Inventory {inventory.InventoryNumber}",
                ReferenceType = "Inventory",
                ReferenceId = inventory.Id,
            }, cancellationToken);
        }

        inventory.Status = InventoryStatus.Approved;
        inventory.ApprovedAt = DateTime.UtcNow;
        inventory.ApprovedByUserId = _currentUser.UserId;
        inventory.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        var shortageCost = inventory.Items.Where(i => i.Difference < 0).Sum(i => -i.DifferenceCost);

        await _audit.LogAsync(
            "Approve", nameof(Inventory), inventory.Id.ToString(),
            newValue: $"shortage={inventory.Items.Where(i => i.Difference < 0).Sum(i => -i.Difference)}, surplus={inventory.Items.Where(i => i.Difference > 0).Sum(i => i.Difference)}",
            cancellationToken: cancellationToken);

        if (shortageCost > LargeShortageCostThreshold)
        {
            await _notifications.NotifyAsync(
                NotificationType.LargeShortage,
                "Крупная недостача",
                $"Ревизия {inventory.InventoryNumber}: недостача на сумму {shortageCost:0.00}.",
                referenceType: "Inventory",
                referenceId: inventory.Id,
                cancellationToken: cancellationToken);
        }

        return ToDto(inventory);
    }

    public async Task<InventoryDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await FindAsync(id, cancellationToken);

        if (inventory.Status == InventoryStatus.Approved)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = ["Нельзя отменить уже утверждённую ревизию (остатки скорректированы)."],
            });
        }

        inventory.Status = InventoryStatus.Cancelled;
        inventory.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Cancel", nameof(Inventory), inventory.Id.ToString(), cancellationToken: cancellationToken);

        return ToDto(inventory);
    }

    private static void EnsureStatus(Inventory inventory, InventoryStatus expected, InventoryStatus target)
    {
        if (inventory.Status != expected)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Нельзя перевести ревизию из {inventory.Status} в {target} (ожидался {expected})."],
            });
        }
    }

    private async Task<Inventory> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Inventories.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), id);
    }

    private async Task<string> GenerateInventoryNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var countToday = await _context.Inventories.CountAsync(x => x.StartedAt >= today, cancellationToken);
        return $"INV-{today:yyyyMMdd}-{countToday + 1:D4}";
    }

    private static InventoryDto ToDto(Inventory inventory) => new()
    {
        Id = inventory.Id,
        WarehouseId = inventory.WarehouseId,
        InventoryNumber = inventory.InventoryNumber,
        Status = inventory.Status,
        ResponsibleUserId = inventory.ResponsibleUserId,
        StartedAt = inventory.StartedAt,
        CompletedAt = inventory.CompletedAt,
        ApprovedAt = inventory.ApprovedAt,
        Notes = inventory.Notes,
        Items = inventory.Items.Select(i => new InventoryItemDto
        {
            Id = i.Id,
            InventoryId = i.InventoryId,
            ProductId = i.ProductId,
            ProductVariationId = i.ProductVariationId,
            SystemQuantity = i.SystemQuantity,
            ActualQuantity = i.ActualQuantity,
            Difference = i.Difference,
            UnitCost = i.UnitCost,
            DifferenceCost = i.DifferenceCost,
        }).ToList(),
    };
}
