using Microsoft.EntityFrameworkCore;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Common;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Entities.Warehouses;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ITenantEntity.TenantId));
            }
        }
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
