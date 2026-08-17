using Microsoft.EntityFrameworkCore;
using Rivo.Application.Orders.Interfaces;
using Rivo.Domain.Entities.Orders;

namespace Rivo.Infrastructure.Persistence.Repositories.Orders;

public class OrdersRepository : IOrdersRepository
{
    private readonly ApplicationDbContext _context;

    public OrdersRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<Order> OrdersWithIncludes() =>
        _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariation)
            .Include(o => o.Payments)
            .Include(o => o.Customer);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        OrdersWithIncludes().IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<OrderItem?> GetOrderItemByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.OrderItems.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<(List<Order> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = OrdersWithIncludes().Where(o => o.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o => o.OrderNumber.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await _context.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order) => _context.Orders.Update(order);
}
