using Microsoft.EntityFrameworkCore;
using Rivo.Application.Roles.Interfaces;
using Rivo.Domain.Entities.Roles;

namespace Rivo.Infrastructure.Persistence.Repositories.Roles;

public class RolesRepository : IRolesRepository
{
    private readonly ApplicationDbContext _context;

    public RolesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default) =>
        _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name, cancellationToken);

    public Task<List<Role>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Roles.Where(r => r.TenantId == tenantId).OrderBy(r => r.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default) =>
        await _context.Roles.AddAsync(role, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken = default) =>
        await _context.Roles.AddRangeAsync(roles, cancellationToken);

    public void Update(Role role) => _context.Roles.Update(role);

    public void Remove(Role role) => _context.Roles.Remove(role);
}
