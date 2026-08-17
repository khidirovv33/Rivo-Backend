using Rivo.Application.Brands.Dtos;

namespace Rivo.Application.Brands.Interfaces;

public interface IBrandsService
{
    Task<BrandDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<List<BrandDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<BrandDto> CreateAsync(Guid tenantId, CreateBrandRequestDto request, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateAsync(Guid tenantId, Guid id, UpdateBrandRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
}
