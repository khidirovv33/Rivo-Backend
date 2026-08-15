using Rivo.Application.Common.Models;
using Rivo.Application.Transfers.Dtos;

namespace Rivo.Application.Transfers.Interfaces;

public interface ITransfersService
{
    Task<PaginatedList<TransferDto>> GetAllAsync(
        PagedRequest request, Guid? warehouseId, CancellationToken cancellationToken = default);

    Task<TransferDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransferDto> CreateAsync(CreateTransferDto dto, CancellationToken cancellationToken = default);

    Task<TransferDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransferDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Списывает товар со склада-источника (StockMovement TransferOut) и переводит в Shipped.</summary>
    Task<TransferDto> ShipAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Зачисляет товар на склад-получатель (StockMovement TransferIn) и переводит в Received.</summary>
    Task<TransferDto> ReceiveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TransferDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
