using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Services;
using Rivo.Application.Inventories.Dtos;
using Rivo.Application.Inventories.Services;
using Rivo.Application.InventoryItems.Dtos;
using Rivo.Application.InventoryItems.Services;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Services;
using Rivo.Domain.Enums;
using Rivo.IntegrationTests.Common;
using Xunit;
using WarehouseEntity = Rivo.Domain.Entities.Warehouses.Warehouse;

namespace Rivo.IntegrationTests.InventoryOperations;

/// <summary>DoD: "ревизия показывает систему/факт/разницу и после утверждения корректирует склад".</summary>
public class InventoryApprovalCorrectsStockTests
{
    [Fact]
    public async Task Approving_a_shortage_reduces_system_stock_to_the_actual_count()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var inventories = new InventoriesService(context, stockMovements, new FakeNotificationsService(), currentUser, audit);
        var inventoryItems = new InventoryItemsService(context);

        var warehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Test Warehouse" };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();
        var warehouseId = warehouse.Id;
        var productId = Guid.NewGuid();

        // system says 20 in stock
        await stockMovements.CreateAsync(new CreateStockMovementDto
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            Type = StockMovementType.Receipt,
            Quantity = 20,
        });

        var inventory = await inventories.CreateAsync(new CreateInventoryDto { WarehouseId = warehouseId });

        // physical count finds only 15 -- shortage of 5
        var scanned = await inventoryItems.ScanAsync(inventory.Id, new ScanInventoryItemDto
        {
            ProductId = productId,
            ActualQuantity = 15,
            UnitCost = 3,
        });

        scanned.SystemQuantity.Should().Be(20);
        scanned.Difference.Should().Be(-5);
        scanned.DifferenceCost.Should().Be(-15);

        await inventories.CompleteAsync(inventory.Id);
        var approved = await inventories.ApproveAsync(inventory.Id);

        approved.Status.Should().Be(InventoryStatus.Approved);
        approved.ShortageQuantity.Should().Be(5);
        approved.ShortageCost.Should().Be(15);

        var stock = await context.Stocks.SingleAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);
        stock.SystemQuantity.Should().Be(15);

        var movement = await context.StockMovements.SingleAsync(x => x.ReferenceType == "Inventory" && x.ReferenceId == inventory.Id);
        movement.Type.Should().Be(StockMovementType.Adjustment);
        movement.Quantity.Should().Be(-5);
    }

    [Fact]
    public async Task Rescanning_the_same_product_updates_the_line_instead_of_duplicating()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var inventories = new InventoriesService(context, stockMovements, new FakeNotificationsService(), currentUser, audit);
        var inventoryItems = new InventoryItemsService(context);

        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var inventory = await inventories.CreateAsync(new CreateInventoryDto { WarehouseId = warehouseId });

        await inventoryItems.ScanAsync(inventory.Id, new ScanInventoryItemDto { ProductId = productId, ActualQuantity = 3 });
        await inventoryItems.ScanAsync(inventory.Id, new ScanInventoryItemDto { ProductId = productId, ActualQuantity = 7 });

        var items = await inventoryItems.GetByInventoryAsync(inventory.Id);
        items.Should().ContainSingle();
        items[0].ActualQuantity.Should().Be(7);
    }
}
