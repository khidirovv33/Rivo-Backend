using Microsoft.EntityFrameworkCore;
using Rivo.Application.Categories.Interfaces;
using Rivo.Domain.Entities.Categories;

namespace Rivo.Infrastructure.Persistence.Repositories.Categories;

public class CategoriesRepository : ICategoriesRepository
{
    private readonly ApplicationDbContext _context;

    public CategoriesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Categories.IgnoreQueryFilters().Where(c => !c.IsDeleted).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Category>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Categories.Where(c => c.TenantId == tenantId).OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
        await _context.Categories.AddAsync(category, cancellationToken);

    public void Update(Category category) => _context.Categories.Update(category);

    public void Remove(Category category) => _context.Categories.Remove(category);
}
