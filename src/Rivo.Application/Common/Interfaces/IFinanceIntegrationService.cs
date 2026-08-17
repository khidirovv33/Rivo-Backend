namespace Rivo.Application.Common.Interfaces;

/// <summary>
/// Contract with Dev 3's Finance module (§12 ТЗ): a sale is income, a return reduces it. Same "logging no-op
/// until the real module lands" pattern as IStockAdjustmentService.
/// </summary>
public interface IFinanceIntegrationService
{
    Task RecordSaleAsync(Guid tenantId, Guid orderId, decimal amount, CancellationToken cancellationToken = default);
    Task RecordRefundAsync(Guid tenantId, Guid returnId, decimal amount, CancellationToken cancellationToken = default);
}
