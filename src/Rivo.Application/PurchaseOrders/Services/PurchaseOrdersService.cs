using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.PurchaseOrders.Dtos;
using Rivo.Application.PurchaseOrders.Interfaces;
using Rivo.Domain.Entities.PurchaseOrders;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.PurchaseOrders.Services;

public class PurchaseOrdersService : IPurchaseOrdersService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public PurchaseOrdersService(IApplicationDbContext context, ICurrentUserService currentUser, IAuditService audit)
    {
        _context = context;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PaginatedList<PurchaseOrderDto>> GetAllAsync(
        PagedRequest request, Guid? supplierId, CancellationToken cancellationToken = default)
    {
        var query = _context.PurchaseOrders.AsNoTracking().Include(x => x.Items).AsQueryable();

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        query = request.SortDescending ? query.OrderByDescending(x => x.OrderDate) : query.OrderBy(x => x.OrderDate);

        var mapped = query.Select(x => ToDto(x));
        return await PaginatedList<PurchaseOrderDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task<PurchaseOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await FindAsync(id, cancellationToken);
        return ToDto(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        var order = new PurchaseOrder
        {
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            OrderNumber = orderNumber,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = dto.ExpectedDate,
            Notes = dto.Notes,
            CreatedBy = _currentUser.UserId,
            Items = dto.Items.Select(i => new PurchaseOrderItem
            {
                ProductId = i.ProductId,
                ProductVariationId = i.ProductVariationId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
            }).ToList(),
        };

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("Create", nameof(PurchaseOrder), order.Id.ToString(), newValue: orderNumber, cancellationToken: cancellationToken);

        return ToDto(order);
    }

    public async Task<PurchaseOrderDto> SendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await FindAsync(id, cancellationToken);
        EnsureTransition(order, PurchaseOrderStatus.Draft, PurchaseOrderStatus.Sent);
        return await ChangeStatusAsync(order, PurchaseOrderStatus.Sent, cancellationToken);
    }

    public async Task<PurchaseOrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await FindAsync(id, cancellationToken);
        EnsureTransition(order, PurchaseOrderStatus.Sent, PurchaseOrderStatus.Confirmed);
        return await ChangeStatusAsync(order, PurchaseOrderStatus.Confirmed, cancellationToken);
    }

    public async Task<PurchaseOrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await FindAsync(id, cancellationToken);

        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Нельзя отменить заказ в статусе {order.Status}."],
            });
        }

        return await ChangeStatusAsync(order, PurchaseOrderStatus.Cancelled, cancellationToken);
    }

    private async Task<PurchaseOrderDto> ChangeStatusAsync(PurchaseOrder order, PurchaseOrderStatus newStatus, CancellationToken cancellationToken)
    {
        var oldStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "StatusChange", nameof(PurchaseOrder), order.Id.ToString(), oldValue: oldStatus.ToString(), newValue: newStatus.ToString(), cancellationToken: cancellationToken);

        return ToDto(order);
    }

    private static void EnsureTransition(PurchaseOrder order, PurchaseOrderStatus expected, PurchaseOrderStatus target)
    {
        if (order.Status != expected)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Нельзя перевести заказ из {order.Status} в {target} (ожидался {expected})."],
            });
        }
    }

    private async Task<PurchaseOrder> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.PurchaseOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), id);
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var countToday = await _context.PurchaseOrders.CountAsync(x => x.OrderDate >= today, cancellationToken);
        return $"PO-{today:yyyyMMdd}-{countToday + 1:D4}";
    }

    private static PurchaseOrderDto ToDto(PurchaseOrder order) => new()
    {
        Id = order.Id,
        SupplierId = order.SupplierId,
        WarehouseId = order.WarehouseId,
        OrderNumber = order.OrderNumber,
        Status = order.Status,
        OrderDate = order.OrderDate,
        ExpectedDate = order.ExpectedDate,
        Notes = order.Notes,
        TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitCost),
        Items = order.Items.Select(i => new PurchaseOrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductVariationId = i.ProductVariationId,
            Quantity = i.Quantity,
            UnitCost = i.UnitCost,
            ReceivedQuantity = i.ReceivedQuantity,
        }).ToList(),
    };
}
