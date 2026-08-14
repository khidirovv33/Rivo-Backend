using Microsoft.EntityFrameworkCore;
using Rivo.Application.Tenancy.Interfaces;
using Rivo.Domain.Entities.Tenancy;

namespace Rivo.Infrastructure.Persistence.Repositories.Tenancy;

public class TenantsRepository : ITenantsRepository
{
    private readonly ApplicationDbContext _context;

    public TenantsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default) =>
        await _context.Tenants.AddAsync(tenant, cancellationToken);
}
