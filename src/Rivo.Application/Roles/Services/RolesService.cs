using Rivo.Application.Common.Interfaces;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Application.Roles.Dtos;
using Rivo.Application.Roles.Interfaces;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Roles.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _rolesRepository;
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IApplicationDbContext _dbContext;

    public RolesService(IRolesRepository rolesRepository, IPermissionsRepository permissionsRepository, IApplicationDbContext dbContext)
    {
        _rolesRepository = rolesRepository;
        _permissionsRepository = permissionsRepository;
        _dbContext = dbContext;
    }

    public async Task<RoleDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var role = await GetTenantRoleOrThrowAsync(tenantId, id, cancellationToken);
        return await ToDtoAsync(role, cancellationToken);
    }

    public async Task<List<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var roles = await _rolesRepository.GetByTenantAsync(tenantId, cancellationToken);
        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            result.Add(await ToDtoAsync(role, cancellationToken));
        }
        return result;
    }

    public async Task<RoleDto> CreateAsync(Guid tenantId, CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _rolesRepository.GetByNameAsync(tenantId, request.Name, cancellationToken) is not null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = new[] { "A role with this name already exists." }
            });
        }

        var role = new Role
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false
        };

        await _rolesRepository.AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _permissionsRepository.ReplaceRolePermissionsAsync(role.Id, request.PermissionIds, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(role, cancellationToken);
    }

    public async Task<RoleDto> UpdateAsync(Guid tenantId, Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var role = await GetTenantRoleOrThrowAsync(tenantId, id, cancellationToken);
        if (role.IsSystemRole)
        {
            throw new ForbiddenAccessException("System roles cannot be modified.");
        }

        role.Name = request.Name;
        role.Description = request.Description;
        _rolesRepository.Update(role);

        await _permissionsRepository.ReplaceRolePermissionsAsync(role.Id, request.PermissionIds, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(role, cancellationToken);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var role = await GetTenantRoleOrThrowAsync(tenantId, id, cancellationToken);
        if (role.IsSystemRole)
        {
            throw new ForbiddenAccessException("System roles cannot be deleted.");
        }

        _rolesRepository.Remove(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Role> GetTenantRoleOrThrowAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var role = await _rolesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), id);

        if (role.TenantId != tenantId)
        {
            throw new TenantMismatchException();
        }

        return role;
    }

    private async Task<RoleDto> ToDtoAsync(Role role, CancellationToken cancellationToken)
    {
        var permissions = await _permissionsRepository.GetByRoleIdAsync(role.Id, cancellationToken);
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            Permissions = permissions.Select(p => p.Name).ToList()
        };
    }
}
