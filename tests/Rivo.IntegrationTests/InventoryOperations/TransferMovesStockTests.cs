using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Rivo.Application.Audit.Services;
using Rivo.Application.StockMovements.Dtos;
using Rivo.Application.StockMovements.Services;
using Rivo.Application.Transfers.Dtos;
using Rivo.Application.Transfers.Services;
using Rivo.Domain.Enums;
using Rivo.IntegrationTests.Common;
using Xunit;
using WarehouseEntity = Rivo.Domain.Entities.Warehouses.Warehouse;

namespace Rivo.IntegrationTests.InventoryOperations;

/// <summary>DoD: перемещение (Transfer) корректно списывает со склада-источника и зачисляет на склад-получатель.</summary>
public class TransferMovesStockTests
{
    [Fact]
    public async Task Shipping_and_receiving_a_transfer_moves_stock_between_warehouses()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var transfers = new TransfersService(context, stockMovements, currentUser, audit);

        var sourceWarehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Source" };
        var destinationWarehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Destination" };
        context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);
        await context.SaveChangesAsync();
        var sourceWarehouseId = sourceWarehouse.Id;
        var destinationWarehouseId = destinationWarehouse.Id;
        var productId = Guid.NewGuid();

        // seed 20 units on the source warehouse
        await stockMovements.CreateAsync(new CreateStockMovementDto
        {
            WarehouseId = sourceWarehouseId,
            ProductId = productId,
            Type = StockMovementType.Receipt,
            Quantity = 20,
        });

        var transfer = await transfers.CreateAsync(new CreateTransferDto
        {
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Items = [new CreateTransferItemDto { ProductId = productId, Quantity = 8 }],
        });

        await transfers.SubmitAsync(transfer.Id);
        await transfers.ApproveAsync(transfer.Id);
        await transfers.ShipAsync(transfer.Id);

        var sourceStock = await context.Stocks.SingleAsync(x => x.WarehouseId == sourceWarehouseId && x.ProductId == productId);
        sourceStock.SystemQuantity.Should().Be(12);

        await transfers.ReceiveAsync(transfer.Id);

        var destinationStock = await context.Stocks.SingleAsync(x => x.WarehouseId == destinationWarehouseId && x.ProductId == productId);
        destinationStock.SystemQuantity.Should().Be(8);

        var updated = await transfers.GetByIdAsync(transfer.Id);
        updated.Status.Should().Be(TransferStatus.Received);
    }

    [Fact]
    public async Task Shipping_more_than_available_is_rejected()
    {
        await using var context = TestDbContextFactory.Create();
        var currentUser = new FakeCurrentUserService();
        var audit = new AuditService(context, currentUser, new FakeCurrentTenantService(), new FakeDateTimeService());
        var stockMovements = new StockMovementsService(context, currentUser, audit);
        var transfers = new TransfersService(context, stockMovements, currentUser, audit);

        var sourceWarehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Source" };
        var destinationWarehouse = new WarehouseEntity { StoreId = Guid.NewGuid(), Name = "Destination" };
        context.Warehouses.AddRange(sourceWarehouse, destinationWarehouse);
        await context.SaveChangesAsync();
        var productId = Guid.NewGuid();

        var transfer = await transfers.CreateAsync(new CreateTransferDto
        {
            SourceWarehouseId = sourceWarehouse.Id,
            DestinationWarehouseId = destinationWarehouse.Id,
            Items = [new CreateTransferItemDto { ProductId = productId, Quantity = 5 }],
        });

        await transfers.SubmitAsync(transfer.Id);
        await transfers.ApproveAsync(transfer.Id);

        var act = async () => await transfers.ShipAsync(transfer.Id);

        await act.Should().ThrowAsync<Rivo.Domain.Exceptions.ValidationAppException>();
    }
}
