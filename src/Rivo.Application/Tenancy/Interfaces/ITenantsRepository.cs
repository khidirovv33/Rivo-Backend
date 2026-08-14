using Rivo.Domain.Entities.Tenancy;

namespace Rivo.Application.Tenancy.Interfaces;

/// <summary>Minimal seam Auth.RegisterAsync needs to create the company account. Full subscription/plan management is Phase 9 (SaaS) — not implemented here.</summary>
public interface ITenantsRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
