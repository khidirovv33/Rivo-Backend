using Rivo.Application.Common.Models;
using Rivo.Application.Notifications.Dtos;
using Rivo.Domain.Enums;

namespace Rivo.Application.Notifications.Interfaces;

public interface INotificationsService
{
    /// <summary>Уведомления текущего пользователя + broadcast (UserId == null) для его tenant'а.</summary>
    Task<PaginatedList<NotificationDto>> GetAllAsync(PagedRequest request, bool? unreadOnly, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);

    /// <summary>Точка входа для остальных модулей — Dev2/Dev3-сервисы вызывают это напрямую (общий проект).</summary>
    Task NotifyAsync(
        NotificationType type, string title, string message, Guid? userId = null,
        string? referenceType = null, Guid? referenceId = null, CancellationToken cancellationToken = default);

    /// <summary>Сверяет текущие остатки с Product.MinimumStock и создаёт LowStock-уведомления (по требованию/по расписанию).</summary>
    Task<int> RunLowStockCheckAsync(CancellationToken cancellationToken = default);
}
