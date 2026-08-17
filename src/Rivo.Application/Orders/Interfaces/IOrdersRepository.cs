using Rivo.Domain.Entities.Orders;

namespace Rivo.Application.Orders.Interfaces;

public interface IOrdersRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderItem?> GetOrderItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Order> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
}
