using Microsoft.Extensions.Logging;
using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Placeholder for Dev 3's Finance module (§12 ТЗ). Same "logging no-op until the real module lands" pattern
/// as StockAdjustmentService — swap the DI registration once Finance.Income/Expenses exist.
/// </summary>
public class FinanceIntegrationService : IFinanceIntegrationService
{
    private readonly ILogger<FinanceIntegrationService> _logger;

    public FinanceIntegrationService(ILogger<FinanceIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task RecordSaleAsync(Guid tenantId, Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Finance:PENDING-DEV3] Sale income Tenant={TenantId} Order={OrderId} Amount={Amount}", tenantId, orderId, amount);
        return Task.CompletedTask;
    }

    public Task RecordRefundAsync(Guid tenantId, Guid returnId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Finance:PENDING-DEV3] Refund Tenant={TenantId} Return={ReturnId} Amount={Amount}", tenantId, returnId, amount);
        return Task.CompletedTask;
    }
}
