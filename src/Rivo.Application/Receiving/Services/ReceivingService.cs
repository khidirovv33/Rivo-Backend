using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Receiving.Dtos;
using Rivo.Application.Receiving.Interfaces;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Domain.Entities.Purchases;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;
using ReceivingEntity = Rivo.Domain.Entities.Receiving.Receiving;
using ReceivingItemEntity = Rivo.Domain.Entities.Receiving.ReceivingItem;

namespace Rivo.Application.Receiving.Services;

public class ReceivingService : IReceivingService
{
    private readonly IApplicationDbContext _context;
    private readonly IStockMovementsService _stockMovements;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public ReceivingService(
        IApplicationDbContext context,
        IStockMovementsService stockMovements,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _context = context;
        _stockMovements = stockMovements;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PaginatedList<ReceivingDto>> GetAllAsync(
        PagedRequest request, Guid? purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var query = _context.Receivings.AsNoTracking().Include(x => x.Items).AsQueryable();

        if (purchaseOrderId.HasValue)
        {
            query = query.Where(x => x.PurchaseOrderId == purchaseOrderId.Value);
        }

        query = request.SortDescending ? query.OrderByDescending(x => x.ReceivingDate) : query.OrderBy(x => x.ReceivingDate);

        var mapped = query.Select(x => ToDto(x));
        return await PaginatedList<ReceivingDto>.CreateAsync(mapped, request.Page, request.PageSize, cancellationToken);
    }

    public async Task<ReceivingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var receiving = await _context.Receivings.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ReceivingEntity), id);
        return ToDto(receiving);
    }

    public async Task<ReceivingDto> CreateAsync(CreateReceivingDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _context.PurchaseOrders.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == dto.PurchaseOrderId, cancellationToken)
            ?? throw new NotFoundException("PurchaseOrder", dto.PurchaseOrderId);

        if (order.Status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Received)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["PurchaseOrderId"] = [$"Заказ в статусе {order.Status} не может принимать поставку."],
            });
        }

        var receiving = new ReceivingEntity
        {
            PurchaseOrderId = order.Id,
            WarehouseId = order.WarehouseId,
            ReceivingDate = DateTime.UtcNow,
            Status = ReceivingStatus.Draft,
            Notes = dto.Notes,
            CreatedByUserId = _currentUser.UserId,
        };

        decimal totalCost = 0;

        foreach (var line in dto.Items)
        {
            var orderItem = order.Items.FirstOrDefault(x => x.Id == line.PurchaseOrderItemId)
                ?? throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [nameof(line.PurchaseOrderItemId)] = [$"Строка {line.PurchaseOrderItemId} не относится к заказу {order.OrderNumber}."],
                });

            var remaining = orderItem.Quantity - orderItem.ReceivedQuantity;
            if (line.QuantityReceived > remaining)
            {
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [nameof(line.QuantityReceived)] = [$"Получено больше, чем заказано: остаток {remaining}, запрошено {line.QuantityReceived}."],
                });
            }

            var unitCost = line.UnitCost ?? orderItem.UnitCost;

            receiving.Items.Add(new ReceivingItemEntity
            {
                PurchaseOrderItemId = orderItem.Id,
                ProductId = orderItem.ProductId,
                ProductVariationId = orderItem.ProductVariationId,
                QuantityReceived = line.QuantityReceived,
                UnitCost = unitCost,
            });

            orderItem.ReceivedQuantity += line.QuantityReceived;
            totalCost += line.QuantityReceived * unitCost;
        }

        receiving.Status = ReceivingStatus.Completed;
        _context.Receivings.Add(receiving);

        order.Status = order.Items.All(x => x.ReceivedQuantity >= x.Quantity)
            ? PurchaseOrderStatus.Received
            : PurchaseOrderStatus.PartiallyReceived;

        var purchase = new Purchase
        {
            SupplierId = order.SupplierId,
            PurchaseOrderId = order.Id,
            ReceivingId = receiving.Id,
            PurchaseDate = receiving.ReceivingDate,
            TotalAmount = totalCost,
            PaidAmount = 0,
        };
        _context.Purchases.Add(purchase);

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var item in receiving.Items)
        {
            await _stockMovements.CreateAsync(new CreateStockMovementDto
            {
                WarehouseId = receiving.WarehouseId,
                ProductId = item.ProductId,
                ProductVariationId = item.ProductVariationId,
                Type = StockMovementType.Receipt,
                Quantity = item.QuantityReceived,
                Reason = $"Receiving {receiving.Id} / PO {order.OrderNumber}",
                ReferenceType = "Receiving",
                ReferenceId = receiving.Id,
            }, cancellationToken);
        }

        await _audit.LogAsync(
            "Create", nameof(ReceivingEntity), receiving.Id.ToString(), newValue: totalCost.ToString(), cancellationToken: cancellationToken);

        return ToDto(receiving);
    }

    private static ReceivingDto ToDto(ReceivingEntity receiving) => new()
    {
        Id = receiving.Id,
        PurchaseOrderId = receiving.PurchaseOrderId,
        WarehouseId = receiving.WarehouseId,
        ReceivingDate = receiving.ReceivingDate,
        Status = receiving.Status,
        Notes = receiving.Notes,
        Items = receiving.Items.Select(i => new ReceivingItemDto
        {
            Id = i.Id,
            PurchaseOrderItemId = i.PurchaseOrderItemId,
            ProductId = i.ProductId,
            ProductVariationId = i.ProductVariationId,
            QuantityReceived = i.QuantityReceived,
            UnitCost = i.UnitCost,
        }).ToList(),
    };
}
