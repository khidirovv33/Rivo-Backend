using Rivo.Application.Common.Interfaces;
using Rivo.Application.Notifications.Interfaces;
using Rivo.Domain.Enums;

namespace Rivo.IntegrationTests.Common;

public class FakeCurrentTenantService : ICurrentTenantService
{
    public Guid? TenantId { get; set; } = Guid.NewGuid();
}

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();

    public string? Email => "tester@rivo.local";

    public string? RoleName => "Owner";

    public string? IpAddress => "127.0.0.1";

    public bool IsAuthenticated => true;
}

public class FakeDateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>No-op — these integration tests assert on stock/finance side effects, not on notifications.</summary>
public class FakeNotificationsService : INotificationsService
{
    public Task<Rivo.Application.Common.Models.PaginatedList<Rivo.Application.Notifications.Dtos.NotificationDto>> GetAllAsync(
        Rivo.Application.Common.Models.PagedRequest request, bool? unreadOnly, CancellationToken cancellationToken = default) =>
        Task.FromResult(new Rivo.Application.Common.Models.PaginatedList<Rivo.Application.Notifications.Dtos.NotificationDto>([], 0, request.PageNumber, request.PageSize));

    public Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task MarkAllAsReadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyAsync(
        NotificationType type, string title, string message, Guid? userId = null,
        string? referenceType = null, Guid? referenceId = null, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<int> RunLowStockCheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}
