using Microsoft.EntityFrameworkCore;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.PurchaseOrders;
using Rivo.Domain.Entities.Purchases;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Entities.Suppliers;
using Rivo.Domain.Entities.Warehouses;
using ReceivingEntity = Rivo.Domain.Entities.Receiving.Receiving;
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

    DbSet<Supplier> Suppliers { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }

    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

    DbSet<ReceivingEntity> Receivings { get; }

    DbSet<Domain.Entities.Receiving.ReceivingItem> ReceivingItems { get; }

    DbSet<Purchase> Purchases { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
