using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Loyalty;

/// <summary>A tier such as Bronze/Silver/VIP that a customer's LoyaltyCard is assigned to based on spend.</summary>
public class LoyaltyLevel : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal MinimumSpend { get; set; }
    public decimal DiscountPercentage { get; set; }
}
