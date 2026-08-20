using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Accounts;

/// <summary>Движение денежных средств (раздел 12 ТЗ) — история для конкретного счёта.</summary>
public class AccountTransaction : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public AccountTransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Description { get; set; }

    /// <summary>Например "Income", "Expense" — что породило движение.</summary>
    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }
}
