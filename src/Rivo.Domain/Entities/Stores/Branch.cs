using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Stores;

/// <summary>A physical point of sale belonging to a Store.</summary>
public class Branch : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public StoreStatus Status { get; set; } = StoreStatus.Active;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
