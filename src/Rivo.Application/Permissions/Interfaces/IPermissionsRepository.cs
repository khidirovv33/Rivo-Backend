using Rivo.Domain.Entities.Permissions;

namespace Rivo.Application.Permissions.Interfaces;

public interface IPermissionsRepository
{
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Permission>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task AssignToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
    Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
}
