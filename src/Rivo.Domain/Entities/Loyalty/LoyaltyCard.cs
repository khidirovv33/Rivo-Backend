using Rivo.Domain.Common;
using Rivo.Domain.Entities.Customers;

namespace Rivo.Domain.Entities.Loyalty;

public class LoyaltyCard : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string CardNumber { get; set; } = string.Empty;

    public Guid? LoyaltyLevelId { get; set; }
    public LoyaltyLevel? LoyaltyLevel { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
