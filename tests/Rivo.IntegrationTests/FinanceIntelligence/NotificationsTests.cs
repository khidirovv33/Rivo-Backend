using AwesomeAssertions;
using Rivo.Application.Accounts.Dtos;
using Rivo.Application.Accounts.Services;
using Rivo.Application.Analytics.Services;
using Rivo.Application.Audit.Services;
using Rivo.Application.Expenses.Dtos;
using Rivo.Application.Expenses.Services;
using Rivo.Application.Notifications.Services;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Services;
using Rivo.Domain.Enums;
using Rivo.IntegrationTests.Common;
using Xunit;
using WarehouseEntity = Rivo.Domain.Entities.Warehouses.Warehouse;

namespace Rivo.IntegrationTests.FinanceIntelligence;

/// <summary>DoD: "уведомления... финансовые события" / "низкий остаток" (§16 ТЗ).</summary>
public class NotificationsTests
{
    [Fact]
    public async Task Creating_an_expense_above_the_threshold_raises_a_finance_event_notification()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var accounts = new AccountsService(context);
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var analytics = new AnalyticsService(context);
        var notifications = new NotificationsService(context, currentUser, analytics);
        var expenses = new ExpensesService(context, accounts, notifications, currentUser, audit);

        var account = await accounts.CreateAsync(new CreateAccountDto { Name = "Касса", Type = AccountType.Cash });

        // below threshold -- no notification
        await expenses.CreateAsync(new CreateExpenseDto { AccountId = account.Id, Category = ExpenseCategory.Other, Amount = 100m });
        context.Notifications.Should().BeEmpty();

        // above threshold -- notification raised
        await expenses.CreateAsync(new CreateExpenseDto { AccountId = account.Id, Category = ExpenseCategory.Rent, Amount = 5000m, Description = "Аренда офиса" });

        var notification = context.Notifications.Single();
        notification.Type.Should().Be(NotificationType.FinanceEvent);
        notification.ReferenceType.Should().Be("Expense");
    }

    [Fact]
    public async Task Low_stock_check_notifies_once_per_product_until_marked_read()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var analytics = new AnalyticsService(context);
        var notifications = new NotificationsService(context, currentUser, analytics);

        var warehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Test Warehouse" };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        var product = new Rivo.Domain.Entities.Products.Product
        {
            Name = "Молоко",
            Sku = "MLK-1",
            PurchasePrice = 5,
            SellingPrice = 8,
            MinimumStock = 10,
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // system stock = 3, below MinimumStock = 10
        await stockMovements.CreateAsync(new CreateStockMovementDto
        {
            WarehouseId = warehouse.Id,
            ProductId = product.Id,
            Type = StockMovementType.Receipt,
            Quantity = 3,
        });

        var firstRun = await notifications.RunLowStockCheckAsync();
        firstRun.Should().Be(1);

        // running again before anyone reads it must not duplicate the notification
        var secondRun = await notifications.RunLowStockCheckAsync();
        secondRun.Should().Be(0);
        context.Notifications.Should().ContainSingle(n => n.Type == NotificationType.LowStock);
    }
}
