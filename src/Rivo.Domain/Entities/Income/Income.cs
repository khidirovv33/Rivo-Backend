using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Income;

/// <summary>
/// Доход (раздел 12 ТЗ): продажи (через IFinanceIntegrationService.RecordSaleAsync — контракт Dev1)
/// и прочие поступления. Amount знаковый: Sale — положительный, Refund — отрицательный, так что
/// Revenue за период = Sum(Amount) по всем записям без дополнительных условий.
/// </summary>
public class Income : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public IncomeType Type { get; set; }

    public decimal Amount { get; set; }

    public DateTime IncomeDate { get; set; }

    public string? Description { get; set; }

    /// <summary>Например "Order", "Return".</summary>
    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }
}
