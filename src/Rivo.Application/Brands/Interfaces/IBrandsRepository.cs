using Rivo.Domain.Entities.Brands;

namespace Rivo.Application.Brands.Interfaces;

public interface IBrandsRepository
{
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Brand>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Brand brand, CancellationToken cancellationToken = default);
    void Update(Brand brand);
    void Remove(Brand brand);
}
