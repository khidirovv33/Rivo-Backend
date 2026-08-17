using Microsoft.EntityFrameworkCore;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;
using WarehouseEntity = Rivo.Domain.Entities.Warehouses.Warehouse;

namespace Rivo.Application.StockMovements.Services;

/// <summary>
/// Real implementation of Dev1's IStockAdjustmentService contract (§8 ТЗ: "Продажа уменьшает остаток,
/// возврат возвращает"), replacing the logging no-op placeholder. Routes every call through
/// IStockMovementsService so a sale/return gets the same negative-stock guard and audit trail as any
/// other Dev2 stock movement.
/// </summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IStockMovementsService _stockMovements;

    public StockAdjustmentService(IApplicationDbContext context, IStockMovementsService stockMovements)
    {
        _context = context;
        _stockMovements = stockMovements;
    }

    public Task DecreaseStockAsync(
        Guid tenantId, Guid branchId, Guid productId, Guid? productVariationId, int quantity, CancellationToken cancellationToken = default) =>
        AdjustAsync(branchId, productId, productVariationId, -quantity, StockMovementType.Sale, cancellationToken);

    public Task IncreaseStockAsync(
        Guid tenantId, Guid branchId, Guid productId, Guid? productVariationId, int quantity, CancellationToken cancellationToken = default) =>
        AdjustAsync(branchId, productId, productVariationId, quantity, StockMovementType.Return, cancellationToken);

    private async Task AdjustAsync(
        Guid branchId, Guid productId, Guid? productVariationId, decimal signedQuantity, StockMovementType type, CancellationToken cancellationToken)
    {
        var warehouseId = await GetOrCreateBranchWarehouseAsync(branchId, cancellationToken);

        await _stockMovements.CreateAsync(new CreateStockMovementDto
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            ProductVariationId = productVariationId,
            Type = type,
            Quantity = signedQuantity,
            ReferenceType = "Order",
        }, cancellationToken);
    }

    /// <summary>
    /// One warehouse per branch, provisioned on first sale/return so Dev1's POS flow never has to know
    /// Warehouse setup is a prerequisite -- matches the zero-friction behavior of the placeholder it replaces.
    /// </summary>
    private async Task<Guid> GetOrCreateBranchWarehouseAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var existing = await _context.Warehouses.FirstOrDefaultAsync(w => w.BranchId == branchId, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken)
            ?? throw new NotFoundException("Branch", branchId);

        var warehouse = new WarehouseEntity
        {
            StoreId = branch.StoreId,
            BranchId = branchId,
            Name = $"{branch.Name} — основной склад",
            IsActive = true,
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }
}
