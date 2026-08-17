namespace Rivo.Domain.Entities.Permissions;

/// <summary>Global permission catalog (not tenant-scoped), e.g. "Products.Create", "Sales.Return". Seeded once at startup.</summary>
public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
}
