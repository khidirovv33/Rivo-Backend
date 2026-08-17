using Rivo.Application.Permissions.Dtos;

namespace Rivo.Application.Permissions.Interfaces;

/// <summary>Read-only global permission catalog. Role-permission assignment lives in RolesService.</summary>
public interface IPermissionsService
{
    Task<List<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
