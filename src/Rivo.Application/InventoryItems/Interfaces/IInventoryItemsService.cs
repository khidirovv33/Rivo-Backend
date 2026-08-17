using Rivo.Application.InventoryItems.Dtos;

namespace Rivo.Application.InventoryItems.Interfaces;

public interface IInventoryItemsService
{
    Task<List<InventoryItemDto>> GetByInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default);

    /// <summary>Добавляет позицию или обновляет ActualQuantity, если товар уже сканировался в этой ревизии.</summary>
    Task<InventoryItemDto> ScanAsync(Guid inventoryId, ScanInventoryItemDto dto, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid inventoryId, Guid itemId, CancellationToken cancellationToken = default);
}
