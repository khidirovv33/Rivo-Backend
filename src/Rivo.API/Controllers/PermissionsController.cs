using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Permissions.Dtos;
using Rivo.Application.Permissions.Interfaces;

namespace Rivo.API.Controllers;

public class PermissionsController : ApiControllerBase
{
    private readonly IPermissionsService _permissionsService;

    public PermissionsController(IPermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    // Permissions are only ever viewed alongside role assignment, so this rides on Roles.Read
    // rather than a dedicated Permissions.* entry in the catalog.
    [PermissionAuthorize("Roles.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _permissionsService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<PermissionDto>>.Ok(result));
    }
}
