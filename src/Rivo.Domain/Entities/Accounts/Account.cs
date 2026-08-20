using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Accounts;

/// <summary>Счёт: Cash, Bank, Card (раздел 12 ТЗ). Balance — денежный остаток, обновляется атомарно вместе с AccountTransaction.</summary>
public class Account : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public AccountType Type { get; set; }

    public decimal Balance { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
}
