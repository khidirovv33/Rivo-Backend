using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rivo.Application.Auth.Interfaces;
using Rivo.Application.Auth.Services;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Brands.Services;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Categories.Services;
using Rivo.Application.Customers.Interfaces;
using Rivo.Application.Customers.Services;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Loyalty.Services;
using Rivo.Application.Orders.Interfaces;
using Rivo.Application.Orders.Services;
using Rivo.Application.Payments.Interfaces;
using Rivo.Application.Payments.Services;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Application.Permissions.Services;
using Rivo.Application.Pos.Interfaces;
using Rivo.Application.Pos.Services;
using Rivo.Application.Products.Interfaces;
using Rivo.Application.Products.Services;
using Rivo.Application.Returns.Interfaces;
using Rivo.Application.Returns.Services;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Roles.Services;
using Rivo.Application.Stores.Interfaces;
using Rivo.Application.Stores.Services;
using Rivo.Application.Users.Interfaces;
using Rivo.Application.Users.Services;

namespace Rivo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IPermissionsService, PermissionsService>();
        services.AddScoped<IStoresService, StoresService>();
        services.AddScoped<IProductsService, ProductsService>();
        services.AddScoped<ICategoriesService, CategoriesService>();
        services.AddScoped<IBrandsService, BrandsService>();
        services.AddScoped<ICustomersService, CustomersService>();
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddScoped<IPosService, PosService>();
        services.AddScoped<IOrdersService, OrdersService>();
        services.AddScoped<IPaymentsService, PaymentsService>();
        services.AddScoped<IReturnsService, ReturnsService>();

        return services;
    }
}
