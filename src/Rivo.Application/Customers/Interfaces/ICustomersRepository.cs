using Rivo.Domain.Entities.Customers;

namespace Rivo.Application.Customers.Interfaces;

public interface ICustomersRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    void Remove(Customer customer);
}
