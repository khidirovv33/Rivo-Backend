using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Dashboard.Dtos;
using Rivo.Application.Dashboard.Interfaces;

namespace Rivo.API.Controllers;

public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetOverview(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetOverviewAsync(from, to, cancellationToken);
        return Ok(ApiResponse<DashboardOverviewDto>.Ok(result));
    }

    // Без PermissionAuthorize — общая сводка на главном экране доступна любой авторизованной роли.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetHomeOverview([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetHomeOverviewAsync(TenantId, branchId, cancellationToken);
        return Ok(ApiResponse<DashboardDto>.Ok(result));
    }
}
