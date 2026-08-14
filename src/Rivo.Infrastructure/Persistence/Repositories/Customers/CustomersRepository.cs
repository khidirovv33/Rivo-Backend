using Microsoft.EntityFrameworkCore;
using Rivo.Application.Customers.Interfaces;
using Rivo.Domain.Entities.Customers;

namespace Rivo.Infrastructure.Persistence.Repositories.Customers;

public class CustomersRepository : ICustomersRepository
{
    private readonly ApplicationDbContext _context;

    public CustomersRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Customers.IgnoreQueryFilters().Where(c => !c.IsDeleted).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.FullName.Contains(searchTerm)
                || (c.Phone != null && c.Phone.Contains(searchTerm))
                || (c.Email != null && c.Email.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await _context.Customers.AddAsync(customer, cancellationToken);

    public void Update(Customer customer) => _context.Customers.Update(customer);

    public void Remove(Customer customer) => _context.Customers.Remove(customer);
}
