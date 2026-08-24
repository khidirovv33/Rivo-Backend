using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rivo.Application.Accounts.Interfaces;
using Rivo.Application.Accounts.Services;
using Rivo.Application.Analytics.Interfaces;
using Rivo.Application.Analytics.Services;
using Rivo.Application.Assistant.Interfaces;
using Rivo.Application.Assistant.Services;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Audit.Services;
using Rivo.Application.Dashboard.Interfaces;
using Rivo.Application.Dashboard.Services;
using Rivo.Application.Auth.Interfaces;
using Rivo.Application.Auth.Services;
using Rivo.Application.Barcodes.Interfaces;
using Rivo.Application.Barcodes.Services;
using Rivo.Application.Brands.Interfaces;
using Rivo.Application.Brands.Services;
using Rivo.Application.Categories.Interfaces;
using Rivo.Application.Categories.Services;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Customers.Interfaces;
using Rivo.Application.Customers.Services;
using Rivo.Application.Expenses.Interfaces;
using Rivo.Application.Expenses.Services;
using Rivo.Application.Finance.Interfaces;
using Rivo.Application.Finance.Services;
using Rivo.Application.Income.Interfaces;
using Rivo.Application.Income.Services;
using Rivo.Application.Inventories.Interfaces;
using Rivo.Application.Inventories.Services;
using Rivo.Application.InventoryItems.Interfaces;
using Rivo.Application.InventoryItems.Services;
using Rivo.Application.Loyalty.Interfaces;
using Rivo.Application.Loyalty.Services;
using Rivo.Application.Notifications.Interfaces;
using Rivo.Application.Notifications.Services;
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
using Rivo.Application.PurchaseOrders.Interfaces;
using Rivo.Application.PurchaseOrders.Services;
using Rivo.Application.Purchases.Interfaces;
using Rivo.Application.Purchases.Services;
using Rivo.Application.Receiving.Interfaces;
using Rivo.Application.Receiving.Services;
using Rivo.Application.Reports.Interfaces;
using Rivo.Application.Reports.Services;
using Rivo.Application.Returns.Interfaces;
using Rivo.Application.Returns.Services;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Roles.Services;
using Rivo.Application.Stock.Interfaces;
using Rivo.Application.Stock.Services;
using Rivo.Application.StockMovements.Interfaces;
using Rivo.Application.StockMovements.Services;
using Rivo.Application.Stores.Interfaces;
using Rivo.Application.Stores.Services;
using Rivo.Application.Suppliers.Interfaces;
using Rivo.Application.Suppliers.Services;
using Rivo.Application.Transfers.Interfaces;
using Rivo.Application.Transfers.Services;
using Rivo.Application.Users.Interfaces;
using Rivo.Application.Users.Services;
using Rivo.Application.Warehouses.Interfaces;
using Rivo.Application.Warehouses.Services;

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
        services.AddScoped<IAssistantToolsService, AssistantToolsService>();

        // Dev2 — Inventory & Operations
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IWarehousesService, WarehousesService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockMovementsService, StockMovementsService>();
        services.AddScoped<ISuppliersService, SuppliersService>();
        services.AddScoped<IPurchaseOrdersService, PurchaseOrdersService>();
        services.AddScoped<IReceivingService, ReceivingService>();
        services.AddScoped<IPurchasesService, PurchasesService>();
        services.AddScoped<ITransfersService, TransfersService>();
        services.AddScoped<IBarcodesService, BarcodesService>();
        services.AddScoped<IInventoriesService, InventoriesService>();
        services.AddScoped<IInventoryItemsService, InventoryItemsService>();

        // Real implementation of Dev1's contract (§8 ТЗ) — replaces the Infrastructure placeholder.
        services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();

        // Dev3 — Finance & Intelligence
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<IIncomeService, IncomeService>();
        services.AddScoped<IExpensesService, ExpensesService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<INotificationsService, NotificationsService>();

        // Real implementation of Dev1's contract (§12 ТЗ) — replaces the Infrastructure placeholder.
        services.AddScoped<IFinanceIntegrationService, Rivo.Application.Income.Services.FinanceIntegrationService>();

        return services;
    }
}
