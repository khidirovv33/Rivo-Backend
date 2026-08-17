using Microsoft.EntityFrameworkCore;
using Rivo.Application.Stores.Interfaces;
using Rivo.Domain.Entities.Stores;

namespace Rivo.Infrastructure.Persistence.Repositories.Stores;

public class StoresRepository : IStoresRepository
{
    private readonly ApplicationDbContext _context;

    public StoresRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Stores.Include(s => s.Branches).IgnoreQueryFilters()
            .Where(s => !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<List<Store>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Stores.Include(s => s.Branches).Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);

    public async Task AddAsync(Store store, CancellationToken cancellationToken = default) =>
        await _context.Stores.AddAsync(store, cancellationToken);

    public void Update(Store store) => _context.Stores.Update(store);

    public void Remove(Store store) => _context.Stores.Remove(store);

    public Task<Branch?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Branches.IgnoreQueryFilters()
            .Where(b => !b.IsDeleted)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task AddBranchAsync(Branch branch, CancellationToken cancellationToken = default) =>
        await _context.Branches.AddAsync(branch, cancellationToken);

    public void UpdateBranch(Branch branch) => _context.Branches.Update(branch);

    public void RemoveBranch(Branch branch) => _context.Branches.Remove(branch);
}
