namespace Rivo.Application.Audit.Interfaces;

/// <summary>Общий контракт записи в Audit Log, используемый сервисами всех модулей.</summary>
public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityName,
        string entityId,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default);
}
