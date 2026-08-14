using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Customers;

public class Customer : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }

    public decimal TotalPurchasesAmount { get; set; }
    public int TotalOrdersCount { get; set; }
    public int LoyaltyPoints { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
