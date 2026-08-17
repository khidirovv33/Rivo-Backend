using Rivo.Application.Categories.Dtos;

namespace Rivo.Application.Categories.Interfaces;

public interface ICategoriesService
{
    Task<CategoryDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<List<CategoryDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(Guid tenantId, CreateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(Guid tenantId, Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
}
