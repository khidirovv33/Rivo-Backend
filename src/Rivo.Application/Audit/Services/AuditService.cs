using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Dtos;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Common.Models;
using Rivo.Domain.Entities.Audit;

namespace Rivo.Application.Audit.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;
    private readonly IDateTimeService _dateTime;

    public AuditService(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant,
        IDateTimeService dateTime)
    {
        _context = context;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _dateTime = dateTime;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string entityId,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditLog
        {
            TenantId = _currentTenant.TenantId ?? Guid.Empty,
            UserId = _currentUser.UserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValue,
            NewValues = newValue,
            IpAddress = _currentUser.IpAddress,
            CreatedAt = _dateTime.UtcNow,
        };

        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedList<AuditLogDto>> GetAllAsync(
        PagedRequest request, string? entityName, Guid? userId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(x => x.EntityName == entityName);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }

        var mapped = query.OrderByDescending(x => x.CreatedAt).Select(x => new AuditLogDto
        {
            Id = x.Id,
            UserId = x.UserId,
            Action = x.Action,
            EntityName = x.EntityName,
            EntityId = x.EntityId,
            OldValues = x.OldValues,
            NewValues = x.NewValues,
            IpAddress = x.IpAddress,
            CreatedAt = x.CreatedAt,
        });

        return await PaginatedList<AuditLogDto>.CreateAsync(mapped, request.PageNumber, request.PageSize, cancellationToken);
    }
}
