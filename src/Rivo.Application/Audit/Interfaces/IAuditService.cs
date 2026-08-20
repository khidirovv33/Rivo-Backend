using Rivo.Application.Audit.Dtos;
using Rivo.Application.Common.Models;

namespace Rivo.Application.Audit.Interfaces;

/// <summary>Общий контракт записи в Audit Log, используемый сервисами всех модулей. Чтение/фильтрация — зона Dev3.</summary>
public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityName,
        string entityId,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default);

    Task<PaginatedList<AuditLogDto>> GetAllAsync(
        PagedRequest request, string? entityName, Guid? userId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default);
}
