namespace Rivo.Domain.Entities.Tenancy;

/// <summary>The company account. One Tenant can own several Stores. Subscription/plan fields are added in Phase 9 (SaaS) — out of Dev1 scope.</summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
