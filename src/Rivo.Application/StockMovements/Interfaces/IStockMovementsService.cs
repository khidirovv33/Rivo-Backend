using Rivo.Application.Common.Models;
using Rivo.Application.StockMovements.Dtos;

namespace Rivo.Application.StockMovements.Interfaces;

public interface IStockMovementsService
{
    Task<PaginatedList<StockMovementDto>> GetAllAsync(
        PagedRequest request, Guid? warehouseId, Guid? productId, CancellationToken cancellationToken = default);

    Task<StockMovementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Единственный путь изменения Stock.SystemQuantity во всей системе. Используется другими
    /// Dev2-модулями напрямую (Receiving, Transfers, Inventory/Revision) и через контракт
    /// Stock Movements — модулями Dev1 (продажа/возврат).
    /// </summary>
    Task<StockMovementDto> CreateAsync(CreateStockMovementDto dto, CancellationToken cancellationToken = default);
}
