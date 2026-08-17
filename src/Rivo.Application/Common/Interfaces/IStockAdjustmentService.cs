namespace Rivo.Application.Common.Interfaces;

/// <summary>
/// Contract with Dev 2's Warehouse/Stock module (§8 ТЗ). A sale must decrease stock, a return must increase it.
/// Until Dev 2 ships the real Warehouse implementation, Infrastructure registers a logging no-op so Dev1's
/// Orders/Returns flow is fully functional and only the DI registration needs to change later.
/// </summary>
public interface IStockAdjustmentService
{
    Task DecreaseStockAsync(Guid tenantId, Guid branchId, Guid productId, Guid? productVariationId, int quantity, CancellationToken cancellationToken = default);
    Task IncreaseStockAsync(Guid tenantId, Guid branchId, Guid productId, Guid? productVariationId, int quantity, CancellationToken cancellationToken = default);
}
