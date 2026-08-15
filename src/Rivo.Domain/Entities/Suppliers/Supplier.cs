using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Suppliers;

public class Supplier : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
