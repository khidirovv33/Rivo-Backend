using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Categories;

public class Category : BaseEntity, ITenantEntity, ISoftDelete
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
