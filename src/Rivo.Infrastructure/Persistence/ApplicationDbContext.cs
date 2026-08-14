using Microsoft.EntityFrameworkCore;
using Rivo.Application.Common.Interfaces;
using Rivo.Domain.Entities.Audit;
using Rivo.Domain.Entities.Auth;
using Rivo.Domain.Entities.Brands;
using Rivo.Domain.Entities.Categories;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Entities.Loyalty;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Payments;
using Rivo.Domain.Entities.Permissions;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Entities.Returns;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Entities.Tenancy;
using Rivo.Domain.Entities.Users;

namespace Rivo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Tenant isolation: every tenant-scoped read is filtered to the caller's tenant by default.
        // Auth flows that must look a user up before a tenant is known (login, registration email check)
        // explicitly call .IgnoreQueryFilters() in the repository.
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
    }
}
