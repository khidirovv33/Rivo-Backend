using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Audit.Services;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Purchases.Interfaces;
using Rivo.Application.Purchases.Services;
using Rivo.Application.PurchaseOrders.Interfaces;
using Rivo.Application.PurchaseOrders.Services;
using Rivo.Application.Receiving.Interfaces;
using Rivo.Application.Receiving.Services;
using Rivo.Application.Stock.Interfaces;
using Rivo.Application.Stock.Services;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Application.StockMovements.Services;
using Rivo.Application.Suppliers.Interfaces;
using Rivo.Application.Suppliers.Services;
using Rivo.Application.Warehouses.Interfaces;
using Rivo.Application.Warehouses.Services;
using Rivo.Infrastructure.Common;
using Rivo.Infrastructure.Identity;
using Rivo.Infrastructure.Multitenancy;
using Rivo.Infrastructure.Persistence;
using System.Text;

namespace Rivo.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRivoInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService, TenantService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<IAuditService, AuditService>();

        // Dev2 — Inventory & Operations
        services.AddScoped<IWarehousesService, WarehousesService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockMovementsService, StockMovementsService>();
        services.AddScoped<ISuppliersService, SuppliersService>();
        services.AddScoped<IPurchaseOrdersService, PurchaseOrdersService>();
        services.AddScoped<IReceivingService, ReceivingService>();
        services.AddScoped<IPurchasesService, PurchasesService>();

        var jwtSection = configuration.GetSection("Jwt");
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty)),
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddRivoApplication(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(Rivo.Application.Common.Mappings.MappingProfile).Assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssembly(typeof(Rivo.Application.Common.Mappings.MappingProfile).Assembly);

        return services;
    }
}
