using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Brands;

public class Brand : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
