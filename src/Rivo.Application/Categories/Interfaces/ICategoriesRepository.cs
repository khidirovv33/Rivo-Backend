using Rivo.Domain.Entities.Categories;

namespace Rivo.Application.Categories.Interfaces;

public interface ICategoriesRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Update(Category category);
    void Remove(Category category);
}
