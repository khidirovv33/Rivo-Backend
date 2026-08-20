using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Dashboard.Dtos;
using Rivo.Application.Dashboard.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("overview")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetOverview(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetOverviewAsync(from, to, cancellationToken);
        return Ok(ApiResponse<DashboardOverviewDto>.Ok(result));
    }
}
