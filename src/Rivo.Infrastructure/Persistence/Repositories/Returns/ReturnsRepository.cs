using Microsoft.EntityFrameworkCore;
using Rivo.Application.Returns.Interfaces;
using Rivo.Domain.Entities.Returns;

namespace Rivo.Infrastructure.Persistence.Repositories.Returns;

public class ReturnsRepository : IReturnsRepository
{
    private readonly ApplicationDbContext _context;

    public ReturnsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Return?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Returns.Include(r => r.Items).IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<Return> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Returns.Include(r => r.Items).Where(r => r.TenantId == tenantId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<int> GetReturnedQuantityForOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken = default) =>
        _context.ReturnItems.Where(ri => ri.OrderItemId == orderItemId).SumAsync(ri => ri.Quantity, cancellationToken);

    public async Task AddAsync(Return returnEntity, CancellationToken cancellationToken = default) =>
        await _context.Returns.AddAsync(returnEntity, cancellationToken);
}
