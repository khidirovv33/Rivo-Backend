using Rivo.Application.Common.Models;
using Rivo.Application.PurchaseOrders.Dtos;

namespace Rivo.Application.PurchaseOrders.Interfaces;

public interface IPurchaseOrdersService
{
    Task<PaginatedList<PurchaseOrderDto>> GetAllAsync(
        PagedRequest request, Guid? supplierId, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> SendAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
