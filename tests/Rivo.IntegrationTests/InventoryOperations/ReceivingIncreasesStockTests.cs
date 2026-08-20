using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Services;
using Rivo.Application.PurchaseOrders.Dtos;
using Rivo.Application.PurchaseOrders.Services;
using Rivo.Application.Receiving.Dtos;
using Rivo.Application.Receiving.Services;
using Rivo.Application.StockMovements.Services;
using Rivo.Domain.Enums;
using Rivo.IntegrationTests.Common;
using Xunit;
using WarehouseEntity = Rivo.Domain.Entities.Warehouses.Warehouse;

namespace Rivo.IntegrationTests.InventoryOperations;

/// <summary>DoD: "закупка корректно увеличивает остаток".</summary>
public class ReceivingIncreasesStockTests
{
    [Fact]
    public async Task Full_receiving_increases_stock_creates_purchase_and_completes_order()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var purchaseOrders = new PurchaseOrdersService(context, currentUser, audit);
        var receiving = new ReceivingService(context, stockMovements, new FakeNotificationsService(), currentUser, audit);

        var warehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Test Warehouse" };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();
        var warehouseId = warehouse.Id;
        var productId = Guid.NewGuid();

        var order = await purchaseOrders.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = Guid.NewGuid(),
            WarehouseId = warehouseId,
            Items = [new CreatePurchaseOrderItemDto { ProductId = productId, Quantity = 10, UnitCost = 5 }],
        });
        await purchaseOrders.SendAsync(order.Id);
        await purchaseOrders.ConfirmAsync(order.Id);

        var result = await receiving.CreateAsync(new CreateReceivingDto
        {
            PurchaseOrderId = order.Id,
            Items = [new CreateReceivingItemDto { PurchaseOrderItemId = order.Items[0].Id, QuantityReceived = 10 }],
        });

        var stock = await context.Stocks.SingleAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);
        stock.SystemQuantity.Should().Be(10);

        var purchase = await context.Purchases.SingleAsync(x => x.ReceivingId == result.Id);
        purchase.TotalAmount.Should().Be(50);
        purchase.OutstandingAmount.Should().Be(50);

        var updatedOrder = await purchaseOrders.GetByIdAsync(order.Id);
        updatedOrder.Status.Should().Be(PurchaseOrderStatus.Received);
    }

    [Fact]
    public async Task Partial_receiving_leaves_order_partially_received()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var purchaseOrders = new PurchaseOrdersService(context, currentUser, audit);
        var receiving = new ReceivingService(context, stockMovements, new FakeNotificationsService(), currentUser, audit);

        var warehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Test Warehouse" };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        var order = await purchaseOrders.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = Guid.NewGuid(),
            WarehouseId = warehouse.Id,
            Items = [new CreatePurchaseOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 10, UnitCost = 5 }],
        });
        await purchaseOrders.SendAsync(order.Id);
        await purchaseOrders.ConfirmAsync(order.Id);

        await receiving.CreateAsync(new CreateReceivingDto
        {
            PurchaseOrderId = order.Id,
            Items = [new CreateReceivingItemDto { PurchaseOrderItemId = order.Items[0].Id, QuantityReceived = 4 }],
        });

        var updatedOrder = await purchaseOrders.GetByIdAsync(order.Id);
        updatedOrder.Status.Should().Be(PurchaseOrderStatus.PartiallyReceived);
        updatedOrder.Items[0].ReceivedQuantity.Should().Be(4);
        updatedOrder.Items[0].RemainingQuantity.Should().Be(6);
    }
}
