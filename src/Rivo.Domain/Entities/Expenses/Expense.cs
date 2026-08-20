using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Expenses;

/// <summary>Расход (раздел 12 ТЗ): аренда, зарплата, транспорт, коммунальные, реклама, прочее.</summary>
public class Expense : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public ExpenseCategory Category { get; set; }

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public string? Description { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }
}
