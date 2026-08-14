using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _permissionsService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<PermissionDto>>.Ok(result));
    }
}
