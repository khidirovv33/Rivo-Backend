using Rivo.Application.Common.Models;
using Rivo.Application.Inventories.Dtos;

namespace Rivo.Application.Inventories.Interfaces;

public interface IInventoriesService
{
    Task<PaginatedList<InventoryDto>> GetAllAsync(
        PagedRequest request, Guid? warehouseId, CancellationToken cancellationToken = default);

    Task<InventoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<InventoryDto> CreateAsync(CreateInventoryDto dto, CancellationToken cancellationToken = default);

    /// <summary>Завершает подсчёт (сканирование окончено) — позиции больше нельзя добавлять/менять.</summary>
    Task<InventoryDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Подтверждает ревизию: для каждой позиции с разницей создаёт корректирующий StockMovement.</summary>
    Task<InventoryDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<InventoryDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
