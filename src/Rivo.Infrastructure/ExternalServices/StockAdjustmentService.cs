using Microsoft.Extensions.Logging;
using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Placeholder for Dev 2's Warehouse/Stock module (§8 ТЗ). Logs the adjustment instead of touching real stock
/// so Orders/Returns run end-to-end today. Once Dev 2 ships the Warehouse module, register that implementation
/// instead of this one in DependencyInjection.cs — no other code needs to change.
/// </summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly ILogger<StockAdjustmentService> _logger;

    public StockAdjustmentService(ILogger<StockAdjustmentService> logger)
    {
        _logger = logger;
    }

    public Task DecreaseStockAsync(Guid tenantId, Guid branchId, Guid productId, Guid? productVariationId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Stock:PENDING-DEV2] Decrease Tenant={TenantId} Branch={BranchId} Product={ProductId} Variation={VariationId} Qty={Quantity}",
            tenantId, branchId, productId, productVariationId, quantity);
        return Task.CompletedTask;
    }

    public Task IncreaseStockAsync(Guid tenantId, Guid branchId, Guid productId, Guid? productVariationId, int quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Stock:PENDING-DEV2] Increase Tenant={TenantId} Branch={BranchId} Product={ProductId} Variation={VariationId} Qty={Quantity}",
            tenantId, branchId, productId, productVariationId, quantity);
        return Task.CompletedTask;
    }
}
