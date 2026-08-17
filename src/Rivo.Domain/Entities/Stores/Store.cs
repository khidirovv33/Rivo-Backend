using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Stores;

public class Store : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public StoreStatus Status { get; set; } = StoreStatus.Active;
    public string Currency { get; set; } = "USD";
    public decimal DefaultTaxRate { get; set; }
    public string? OpeningHours { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
