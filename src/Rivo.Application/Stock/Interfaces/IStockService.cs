using Rivo.Application.Common.Models;
using Rivo.Application.Stock.Dtos;

namespace Rivo.Application.Stock.Interfaces;

public interface IStockService
{
    Task<PaginatedList<StockDto>> GetAllAsync(PagedRequest request, Guid? warehouseId, Guid? productId, CancellationToken cancellationToken = default);

    Task<StockDto> GetAsync(Guid warehouseId, Guid productId, Guid? productVariationId, CancellationToken cancellationToken = default);

    /// <summary>Резервирует товар (например, под открытый заказ POS) — увеличивает ReservedQuantity.</summary>
    Task<StockDto> ReserveAsync(ReserveStockDto dto, CancellationToken cancellationToken = default);

    /// <summary>Снимает резерв (отмена заказа, оформление продажи) — уменьшает ReservedQuantity.</summary>
    Task<StockDto> ReleaseReservationAsync(ReserveStockDto dto, CancellationToken cancellationToken = default);
}
