using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Entities.Audit;

namespace Rivo.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Writes Who/What/When/Entity/EntityId/OldValue/NewValue/IP (§16 ТЗ) for every tracked Add/Modify/Delete,
/// in the same SaveChanges batch as the change itself, so no caller needs to remember to audit anything.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService _currentTenantService;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService, ICurrentTenantService currentTenantService)
    {
        _currentUserService = currentUserService;
        _currentTenantService = currentTenantService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var tenantId = _currentTenantService.TenantId ?? Guid.Empty;

        foreach (var entry in entries)
        {
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            var entityId = idProperty?.CurrentValue?.ToString() ?? string.Empty;

            string? oldValues = null;
            string? newValues = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues = JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;
                case EntityState.Deleted:
                    oldValues = JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    break;
                case EntityState.Modified:
                    var changed = entry.Properties.Where(p => p.IsModified).ToList();
                    oldValues = JsonSerializer.Serialize(changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    newValues = JsonSerializer.Serialize(changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;
            }

            context.Set<AuditLog>().Add(new AuditLog
            {
                TenantId = tenantId,
                UserId = _currentUserService.UserId,
                Action = entry.State.ToString(),
                EntityName = entry.Entity.GetType().Name,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = _currentUserService.IpAddress
            });
        }
    }
}
