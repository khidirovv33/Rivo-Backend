using Rivo.Domain.Common;

namespace Rivo.Domain.Entities.Audit;

/// <summary>
/// Owner: Developer 3 (Finance & Intelligence) — этот минимальный write-контракт заведён в Phase A,
/// т.к. ключевые операции Dev2 (списания, корректировки, ревизии) обязаны писать сюда по DoD.
/// Отчёты/фильтрация/UI по Audit Log — зона Dev3.
/// </summary>
public class AuditLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }
}
