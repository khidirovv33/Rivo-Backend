using Rivo.Domain.Common;
using Rivo.Domain.Enums;

namespace Rivo.Domain.Entities.Notifications;

/// <summary>Раздел 16 ТЗ. UserId == null — уведомление для всех пользователей tenant'а.</summary>
public class Notification : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }
}
