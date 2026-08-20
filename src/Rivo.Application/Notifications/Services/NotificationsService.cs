using Microsoft.EntityFrameworkCore;
using Rivo.Application.Analytics.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Application.Notifications.Dtos;
using Rivo.Application.Notifications.Interfaces;
using Rivo.Domain.Entities.Notifications;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Notifications.Services;

public class NotificationsService : INotificationsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAnalyticsService _analytics;

    public NotificationsService(IApplicationDbContext context, ICurrentUserService currentUser, IAnalyticsService analytics)
    {
        _context = context;
        _currentUser = currentUser;
        _analytics = analytics;
    }

    public async Task<PaginatedList<NotificationDto>> GetAllAsync(
        PagedRequest request, bool? unreadOnly, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        var query = _context.Notifications.AsNoTracking()
            .Where(x => x.UserId == null || x.UserId == userId);

        if (unreadOnly == true)
        {
            query = query.Where(x => !x.IsRead);
        }

        var mapped = query.OrderByDescending(x => x.CreatedAt).Select(x => ToDto(x));
        return await PaginatedList<NotificationDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), id);

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        var unread = await _context.Notifications
            .Where(x => (x.UserId == null || x.UserId == userId) && !x.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyAsync(
        NotificationType type, string title, string message, Guid? userId = null,
        string? referenceType = null, Guid? referenceId = null, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RunLowStockCheckAsync(CancellationToken cancellationToken = default)
    {
        var lowStockItems = await _analytics.GetLowStockAsync(cancellationToken);
        var created = 0;

        foreach (var item in lowStockItems)
        {
            var alreadyNotified = await _context.Notifications.AnyAsync(
                x => x.Type == NotificationType.LowStock && x.ReferenceType == "Product" && x.ReferenceId == item.ProductId && !x.IsRead,
                cancellationToken);

            if (alreadyNotified)
            {
                continue;
            }

            await NotifyAsync(
                NotificationType.LowStock,
                "Низкий остаток",
                $"{item.ProductName}: остаток {item.CurrentStock} ниже минимума {item.MinimumStock}.",
                referenceType: "Product",
                referenceId: item.ProductId,
                cancellationToken: cancellationToken);

            created++;
        }

        return created;
    }

    private static NotificationDto ToDto(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        Title = notification.Title,
        Message = notification.Message,
        IsRead = notification.IsRead,
        ReferenceType = notification.ReferenceType,
        ReferenceId = notification.ReferenceId,
        CreatedAt = notification.CreatedAt,
    };
}
