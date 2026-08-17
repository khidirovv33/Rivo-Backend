using Microsoft.EntityFrameworkCore;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.InventoryItems.Dtos;
using Rivo.Application.InventoryItems.Interfaces;
using Rivo.Domain.Entities.Inventories;
using Rivo.Domain.Entities.InventoryItems;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.InventoryItems.Services;

public class InventoryItemsService : IInventoryItemsService
{
    private readonly IApplicationDbContext _context;

    public InventoryItemsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryItemDto>> GetByInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems.AsNoTracking()
            .Where(x => x.InventoryId == inventoryId)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItemDto> ScanAsync(Guid inventoryId, ScanInventoryItemDto dto, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), inventoryId);

        if (inventory.Status != InventoryStatus.Draft)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Сканирование недоступно для ревизии в статусе {inventory.Status}."],
            });
        }

        var item = await _context.InventoryItems.FirstOrDefaultAsync(
            x => x.InventoryId == inventoryId && x.ProductId == dto.ProductId && x.ProductVariationId == dto.ProductVariationId,
            cancellationToken);

        if (item is not null)
        {
            item.ActualQuantity = dto.ActualQuantity;
            if (dto.UnitCost.HasValue)
            {
                item.UnitCost = dto.UnitCost.Value;
            }
        }
        else
        {
            var stock = await _context.Stocks.AsNoTracking().FirstOrDefaultAsync(
                x => x.WarehouseId == inventory.WarehouseId && x.ProductId == dto.ProductId && x.ProductVariationId == dto.ProductVariationId,
                cancellationToken);

            item = new InventoryItem
            {
                InventoryId = inventoryId,
                ProductId = dto.ProductId,
                ProductVariationId = dto.ProductVariationId,
                SystemQuantity = stock?.SystemQuantity ?? 0,
                ActualQuantity = dto.ActualQuantity,
                UnitCost = dto.UnitCost ?? 0,
            };
            _context.InventoryItems.Add(item);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(item);
    }

    public async Task RemoveAsync(Guid inventoryId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Inventory), inventoryId);

        if (inventory.Status != InventoryStatus.Draft)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["Status"] = [$"Изменение позиций недоступно для ревизии в статусе {inventory.Status}."],
            });
        }

        var item = await _context.InventoryItems.FirstOrDefaultAsync(x => x.Id == itemId && x.InventoryId == inventoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(InventoryItem), itemId);

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static InventoryItemDto ToDto(InventoryItem item) => new()
    {
        Id = item.Id,
        InventoryId = item.InventoryId,
        ProductId = item.ProductId,
        ProductVariationId = item.ProductVariationId,
        SystemQuantity = item.SystemQuantity,
        ActualQuantity = item.ActualQuantity,
        Difference = item.Difference,
        UnitCost = item.UnitCost,
        DifferenceCost = item.DifferenceCost,
    };
}
