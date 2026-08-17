using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rivo.Application.Auth.Interfaces;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Customers.Interfaces;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Payments.Interfaces;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Application.Products.Interfaces;
using Rivo.Application.Returns.Interfaces;
using Rivo.Application.Barcodes.Interfaces;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Stores.Interfaces;
using Rivo.Application.Tenancy.Interfaces;
using Rivo.Application.Users.Interfaces;
using Rivo.Infrastructure.Common;
using Rivo.Infrastructure.ExternalServices;
using Rivo.Infrastructure.Identity;
using Rivo.Infrastructure.Multitenancy;
using Rivo.Infrastructure.Persistence;
using Rivo.Infrastructure.Persistence.Interceptors;
using Rivo.Infrastructure.Persistence.Repositories.Auth;
using Rivo.Infrastructure.Persistence.Repositories.Brands;
using Rivo.Infrastructure.Persistence.Repositories.Categories;
using Rivo.Infrastructure.Persistence.Repositories.Customers;
using Rivo.Infrastructure.Persistence.Repositories.Loyalty;
using Rivo.Infrastructure.Persistence.Repositories.Orders;
using Rivo.Infrastructure.Persistence.Repositories.Payments;
using Rivo.Infrastructure.Persistence.Repositories.Permissions;
using Rivo.Infrastructure.Persistence.Repositories.Products;
using Rivo.Infrastructure.Persistence.Repositories.Returns;
using Rivo.Infrastructure.Persistence.Repositories.Roles;
using Rivo.Infrastructure.Persistence.Repositories.Stores;
using Rivo.Infrastructure.Persistence.Repositories.Tenancy;
using Rivo.Infrastructure.Persistence.Repositories.Users;

namespace Rivo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService, TenantService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfExportService, PdfExportService>();

        // Dev2's real IStockAdjustmentService (routes through IStockMovementsService) is registered in
        // Rivo.Application.DependencyInjection — it lives in the Application layer, not here.
        // Dev3 contract placeholder — see FinanceIntegrationService for the swap point.
        services.AddScoped<IFinanceIntegrationService, FinanceIntegrationService>();

        // Dev2 — Inventory & Operations (Infrastructure-side implementations)
        services.AddSingleton<IBarcodeValueGenerator, BarcodeGeneratorService>();
        services.AddSingleton<IBarcodeImageRenderer, BarcodeGeneratorService>();

        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITenantsRepository, TenantsRepository>();
        services.AddScoped<IStoresRepository, StoresRepository>();
        services.AddScoped<ICategoriesRepository, CategoriesRepository>();
        services.AddScoped<IBrandsRepository, BrandsRepository>();
        services.AddScoped<IProductsRepository, ProductsRepository>();
        services.AddScoped<ICustomersRepository, CustomersRepository>();
        services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<IPaymentsRepository, PaymentsRepository>();
        services.AddScoped<IReturnsRepository, ReturnsRepository>();

        return services;
    }
}
