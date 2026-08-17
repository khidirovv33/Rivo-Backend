using Microsoft.EntityFrameworkCore;
using Rivo.Application.Brands.Interfaces;
using Rivo.Domain.Entities.Brands;

namespace Rivo.Infrastructure.Persistence.Repositories.Brands;

public class BrandsRepository : IBrandsRepository
{
    private readonly ApplicationDbContext _context;

    public BrandsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Brands.IgnoreQueryFilters().Where(b => !b.IsDeleted).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<List<Brand>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Brands.Where(b => b.TenantId == tenantId).OrderBy(b => b.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Brand brand, CancellationToken cancellationToken = default) =>
        await _context.Brands.AddAsync(brand, cancellationToken);

    public void Update(Brand brand) => _context.Brands.Update(brand);

    public void Remove(Brand brand) => _context.Brands.Remove(brand);
}
