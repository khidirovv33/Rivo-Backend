using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Common;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.Barcodes;
using Rivo.Domain.Entities.Inventories;
using Rivo.Domain.Entities.InventoryItems;
using Rivo.Domain.Entities.PurchaseOrders;
using Rivo.Domain.Entities.Purchases;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Entities.Suppliers;
using Rivo.Domain.Entities.Transfers;
using Rivo.Domain.Entities.Warehouses;
using ReceivingEntity = Rivo.Domain.Entities.Receiving.Receiving;
using ReceivingItemEntity = Rivo.Domain.Entities.Receiving.ReceivingItem;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenant;
    private readonly IDateTimeService _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenant,
        IDateTimeService dateTime)
        : base(options)
    {
        _currentTenant = currentTenant;
        _dateTime = dateTime;
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<StockEntity> Stocks => Set<StockEntity>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    public DbSet<ReceivingEntity> Receivings => Set<ReceivingEntity>();

    public DbSet<ReceivingItemEntity> ReceivingItems => Set<ReceivingItemEntity>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<TransferItem> TransferItems => Set<TransferItem>();

    public DbSet<Barcode> Barcodes => Set<Barcode>();

    public DbSet<Inventory> Inventories => Set<Inventory>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    /// <summary>Читается свежо при каждой компиляции запроса — DbContext per-request, значение не устаревает.</summary>
    private Guid CurrentTenantId => _currentTenant.TenantId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ITenantEntity.TenantId));
                ApplyTenantQueryFilter(modelBuilder, entityType);
            }
        }
    }

    /// <summary>
    /// Изоляция данных между tenant'ами (раздел 27 ТЗ). Именованный фильтр (EF Core 10) —
    /// комбинируется через AND с любым другим именованным фильтром на этой же entity
    /// (например "SoftDelete" в WarehouseConfiguration), не перезаписывая его.
    /// </summary>
    private void ApplyTenantQueryFilter(ModelBuilder modelBuilder, IMutableEntityType entityType)
    {
        var clrType = entityType.ClrType;
        var parameter = Expression.Parameter(clrType, "e");

        var tenantCheck = Expression.Equal(
            Expression.Property(parameter, nameof(ITenantEntity.TenantId)),
            Expression.Property(Expression.Constant(this), nameof(CurrentTenantId)));

        modelBuilder.Entity(clrType).HasQueryFilter("TenantIsolation", Expression.Lambda(tenantCheck, parameter));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    if (entry.Entity is ITenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && _currentTenant.TenantId.HasValue)
                    {
                        tenantEntity.TenantId = _currentTenant.TenantId.Value;
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
