using Microsoft.EntityFrameworkCore;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Domain.Entities.Permissions;

namespace Rivo.Infrastructure.Persistence.Repositories.Permissions;

public class PermissionsRepository : IPermissionsRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Permissions.OrderBy(p => p.Name).ToListAsync(cancellationToken);

    public Task<List<Permission>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default) =>
        _context.Permissions.Where(p => names.Contains(p.Name)).ToListAsync(cancellationToken);

    public Task<List<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);

    public async Task AssignToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        foreach (var permissionId in permissionIds)
        {
            await _context.RolePermissions.AddAsync(new RolePermission { RoleId = roleId, PermissionId = permissionId }, cancellationToken);
        }
    }

    public async Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        _context.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in permissionIds)
        {
            await _context.RolePermissions.AddAsync(new RolePermission { RoleId = roleId, PermissionId = permissionId }, cancellationToken);
        }
    }
}
