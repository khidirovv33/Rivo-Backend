using Rivo.Application.Roles.Dtos;

namespace Rivo.Application.Roles.Interfaces;

public interface IRolesService
{
    Task<RoleDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<List<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(Guid tenantId, CreateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateAsync(Guid tenantId, Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
}
