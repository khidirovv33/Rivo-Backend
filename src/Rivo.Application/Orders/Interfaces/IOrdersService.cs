using Rivo.Application.Common.Models;
using Rivo.Application.Orders.Dtos;

namespace Rivo.Application.Orders.Interfaces;

/// <summary>Read-side of Sales history. Orders are created via IPosService.CheckoutAsync, not here.</summary>
public interface IOrdersService
{
    Task<OrderDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<OrderDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
}
