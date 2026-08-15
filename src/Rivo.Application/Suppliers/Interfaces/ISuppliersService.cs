using Rivo.Application.Common.Models;
using Rivo.Application.Suppliers.Dtos;

namespace Rivo.Application.Suppliers.Interfaces;

public interface ISuppliersService
{
    Task<PaginatedList<SupplierDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SupplierDto> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default);

    Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
