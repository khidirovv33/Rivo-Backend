namespace Rivo.Domain.Entities.Audit;

/// <summary>Minimal audit trail written by the SaveChanges interceptor for every tracked entity change. Full reporting/query UX belongs to Dev 3 (Audit module); this is the shared write-path Dev1 depends on for its Definition of Done.</summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
