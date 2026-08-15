using Microsoft.EntityFrameworkCore;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Entities.Warehouses;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Application.Common.Interfaces;

/// <summary>
/// Общий контракт EF Core DbContext. Каждый разработчик добавляет сюда DbSet своих сущностей
/// по мере реализации модулей (см. Dev2-модули ниже; Dev1/Dev3 дополняют своими).
/// </summary>
public interface IApplicationDbContext
{
    DbSet<AuditLog> AuditLogs { get; }

    // Dev2 — Inventory & Operations
    DbSet<Warehouse> Warehouses { get; }

    DbSet<StockEntity> Stocks { get; }

    DbSet<StockMovement> StockMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
