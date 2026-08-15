using Rivo.Application.Common.Models;
using Rivo.Application.Receiving.Dtos;

namespace Rivo.Application.Receiving.Interfaces;

public interface IReceivingService
{
    Task<PaginatedList<ReceivingDto>> GetAllAsync(PagedRequest request, Guid? purchaseOrderId, CancellationToken cancellationToken = default);

    Task<ReceivingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проводит приём товара (полный или частичный): увеличивает ReceivedQuantity строк заказа,
    /// создаёт StockMovement (Receipt) на каждую позицию, при полном получении заказа переводит
    /// PurchaseOrder.Status = Received (иначе PartiallyReceived), и создаёт финансовую запись Purchase.
    /// </summary>
    Task<ReceivingDto> CreateAsync(CreateReceivingDto dto, CancellationToken cancellationToken = default);
}
