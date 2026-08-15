using Microsoft.EntityFrameworkCore;
using Rivo.Domain.Entities.Audit;

namespace Rivo.Application.Common.Interfaces;

/// <summary>
/// Общий контракт EF Core DbContext. Каждый разработчик добавляет сюда DbSet своих сущностей
/// по мере реализации модулей (см. Dev2-модули ниже; Dev1/Dev3 дополняют своими).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
