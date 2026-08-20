namespace Rivo.Application.Audit.Dtos;

/// <summary>Who, What, When, Entity, EntityId, OldValue, NewValue, IP — раздел 16 ТЗ.</summary>
public class AuditLogDto
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }
}
