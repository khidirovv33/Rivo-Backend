using Rivo.Application.Common.Interfaces;
using Rivo.Application.Income.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.Application.Income.Services;

/// <summary>
/// Real implementation of Dev1's IFinanceIntegrationService contract (§12 ТЗ: продажа — доход,
/// возврат уменьшает его), replacing the logging no-op placeholder. Routes through IIncomeService so
/// a sale/refund gets the same account-ledger/audit trail as any other Finance movement.
/// tenantId/orderId/returnId are accepted per the contract but tenant is actually ambient (current
/// DbContext) and the ids are carried as ReferenceId for traceability, not re-validated here.
/// </summary>
public class FinanceIntegrationService : IFinanceIntegrationService
{
    private readonly IIncomeService _income;

    public FinanceIntegrationService(IIncomeService income)
    {
        _income = income;
    }

    public async Task RecordSaleAsync(Guid tenantId, Guid orderId, decimal amount, CancellationToken cancellationToken = default)
    {
        await _income.RecordAsync(IncomeType.Sale, amount, $"Sale {orderId}", "Order", orderId, cancellationToken);
    }

    public async Task RecordRefundAsync(Guid tenantId, Guid returnId, decimal amount, CancellationToken cancellationToken = default)
    {
        await _income.RecordAsync(IncomeType.Refund, amount, $"Refund {returnId}", "Return", returnId, cancellationToken);
    }
}
