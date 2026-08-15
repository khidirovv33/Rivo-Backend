using Rivo.Application.Common.Models;
using Rivo.Application.Warehouses.Dtos;

namespace Rivo.Application.Warehouses.Interfaces;

public interface IWarehousesService
{
    Task<PaginatedList<WarehouseDto>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);

    Task<WarehouseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default);

    Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
