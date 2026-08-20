using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Income.Dtos;
using Rivo.Application.Income.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/income")]
public class IncomeController : ControllerBase
{
    private readonly IIncomeService _service;

    public IncomeController(IIncomeService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<IncomeDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, from, to, cancellationToken);
        return Ok(ApiResponse<PaginatedList<IncomeDto>>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Finance.Create")]
    public async Task<ActionResult<ApiResponse<IncomeDto>>> Create(
        [FromBody] CreateIncomeDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return Ok(ApiResponse<IncomeDto>.Ok(result));
    }
}
