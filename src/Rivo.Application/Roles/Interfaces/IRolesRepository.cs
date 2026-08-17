using Rivo.Domain.Entities.Roles;

namespace Rivo.Application.Roles.Interfaces;

public interface IRolesRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);
    Task<List<Role>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Role> roles, CancellationToken cancellationToken = default);
    void Update(Role role);
    void Remove(Role role);
}
