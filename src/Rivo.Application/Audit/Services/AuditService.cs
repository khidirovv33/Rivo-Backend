using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Interfaces;
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
}
