using Rivo.Domain.Common;
using Rivo.Domain.Entities.Permissions;

namespace Rivo.Domain.Entities.Roles;

/// <summary>Roles are per-tenant. Default set (Owner, Admin, Manager, Cashier, Warehouse Worker, Accountant, Auditor) is seeded on tenant registration; tenants may add custom roles later.</summary>
public class Role : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
