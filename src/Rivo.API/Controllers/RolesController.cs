using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Roles.Dtos;
using Rivo.Application.Roles.Interfaces;

namespace Rivo.API.Controllers;

public class RolesController : ApiControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [PermissionAuthorize("Roles.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _rolesService.GetAllAsync(TenantId, cancellationToken);
        return Ok(ApiResponse<List<RoleDto>>.Ok(result));
    }

    [PermissionAuthorize("Roles.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _rolesService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<RoleDto>.Ok(result));
    }

    [PermissionAuthorize("Roles.Create")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(CreateRoleRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _rolesService.CreateAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<RoleDto>.Ok(result));
    }

    [PermissionAuthorize("Roles.Update")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _rolesService.UpdateAsync(TenantId, id, request, cancellationToken);
        return Ok(ApiResponse<RoleDto>.Ok(result));
    }

    [PermissionAuthorize("Roles.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _rolesService.DeleteAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Role deleted."));
    }
}
