using Rivo.Application.Common.Models;
using Rivo.Application.Customers.Dtos;

namespace Rivo.Application.Customers.Interfaces;

public interface ICustomersService
{
    Task<CustomerDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<CustomerDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(Guid tenantId, CreateCustomerRequestDto request, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(Guid tenantId, Guid id, UpdateCustomerRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
}
