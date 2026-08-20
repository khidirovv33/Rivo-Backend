using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Finance.Dtos;
using Rivo.Application.Finance.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/finance")]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _service;

    public FinanceController(IFinanceService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<FinanceSummaryDto>>> GetSummary(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(from, to, cancellationToken);
        return Ok(ApiResponse<FinanceSummaryDto>.Ok(result));
    }
}
