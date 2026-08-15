using Rivo.Application.Common.Models;
using Rivo.Application.Purchases.Dtos;

namespace Rivo.Application.Purchases.Interfaces;

public interface IPurchasesService
{
    Task<PaginatedList<PurchaseDto>> GetAllAsync(PagedRequest request, Guid? supplierId, CancellationToken cancellationToken = default);

    Task<PurchaseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Регистрирует оплату по закупке, уменьшая задолженность перед поставщиком.</summary>
    Task<PurchaseDto> RecordPaymentAsync(Guid id, RecordPaymentDto dto, CancellationToken cancellationToken = default);
}
