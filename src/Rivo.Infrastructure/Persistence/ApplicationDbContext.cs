using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Common;
using Rivo.Domain.Entities.Accounts;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.Auth;
using Rivo.Domain.Entities.Brands;
using Rivo.Domain.Entities.Categories;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Entities.Expenses;
using Rivo.Domain.Entities.Loyalty;
using Rivo.Domain.Entities.Notifications;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Payments;
using Rivo.Domain.Entities.Permissions;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Entities.PurchaseOrders;
using Rivo.Domain.Entities.Purchases;
using Rivo.Domain.Entities.Returns;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Entities.StockMovements;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Entities.Suppliers;
using Rivo.Domain.Entities.Tenancy;
using Rivo.Domain.Entities.Transfers;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Entities.Warehouses;
using BarcodeEntity = Rivo.Domain.Entities.Barcodes.Barcode;
using IncomeEntity = Rivo.Domain.Entities.Income.Income;
using InventoryEntity = Rivo.Domain.Entities.Inventories.Inventory;
using InventoryItemEntity = Rivo.Domain.Entities.InventoryItems.InventoryItem;
using ReceivingEntity = Rivo.Domain.Entities.Receiving.Receiving;
using ReceivingItemEntity = Rivo.Domain.Entities.Receiving.ReceivingItem;
using StockEntity = Rivo.Domain.Entities.Stock.Stock;

namespace Rivo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IDateTimeService _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService currentTenantService,
        IDateTimeService dateTime)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _dateTime = dateTime;
    }

    // Dev1 — Core & Commerce
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariation> ProductVariations => Set<ProductVariation>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoyaltyLevel> LoyaltyLevels => Set<LoyaltyLevel>();
    public DbSet<LoyaltyCard> LoyaltyCards => Set<LoyaltyCard>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Dev2 — Inventory & Operations
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

    public DbSet<BarcodeEntity> Barcodes => Set<BarcodeEntity>();

    public DbSet<InventoryEntity> Inventories => Set<InventoryEntity>();

    public DbSet<InventoryItemEntity> InventoryItems => Set<InventoryItemEntity>();

    // Dev3 — Finance & Intelligence
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();

    public DbSet<IncomeEntity> Incomes => Set<IncomeEntity>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>Читается свежо при каждой компиляции запроса — DbContext per-request, значение не устаревает.</summary>
    private Guid CurrentTenantId => _currentTenantService.TenantId ?? Guid.Empty;

    /// <summary>Dev2/Dev3-сущности, для которых генерируется общий именованный tenant-фильтр (см. ApplyTenantQueryFilter).</summary>
    private static readonly Type[] ReflectionTenantEntityTypes =
    [
        typeof(Warehouse), typeof(StockEntity), typeof(StockMovement), typeof(Supplier),
        typeof(PurchaseOrder), typeof(PurchaseOrderItem), typeof(ReceivingEntity), typeof(ReceivingItemEntity),
        typeof(Purchase), typeof(Transfer), typeof(TransferItem), typeof(BarcodeEntity),
        typeof(InventoryEntity), typeof(InventoryItemEntity),
        typeof(Account), typeof(AccountTransaction), typeof(IncomeEntity), typeof(Expense), typeof(Notification),
    ];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Tenant isolation (Dev1 entities): every tenant-scoped read is filtered to the caller's tenant by
        // default. Auth flows that must look a user up before a tenant is known (login, registration email
        // check) explicitly call .IgnoreQueryFilters() in the repository.
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
        modelBuilder.Entity<Store>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<Branch>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<Brand>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId && !e.IsDeleted);
        modelBuilder.Entity<LoyaltyLevel>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
        modelBuilder.Entity<LoyaltyCard>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
        modelBuilder.Entity<Return>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);

        // Tenant isolation (Dev2 entities): named EF Core 10 query filter, combined via AND with any other
        // named filter already set on the same entity by its IEntityTypeConfiguration (e.g. "SoftDelete" on
        // Warehouse/Supplier, "ParentSoftDelete" on Stock/StockMovement) instead of overwriting it.
        foreach (var clrType in ReflectionTenantEntityTypes)
        {
            var entityType = modelBuilder.Model.FindEntityType(clrType)!;
            modelBuilder.Entity(clrType).HasIndex(nameof(ITenantEntity.TenantId));
            ApplyTenantQueryFilter(modelBuilder, entityType);
        }
    }

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
                    if (entry.Entity is ITenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && _currentTenantService.TenantId.HasValue)
                    {
                        tenantEntity.TenantId = _currentTenantService.TenantId.Value;
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
