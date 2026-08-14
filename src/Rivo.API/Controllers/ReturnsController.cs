using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Returns.Dtos;
using Rivo.Application.Returns.Interfaces;

namespace Rivo.API.Controllers;

public class ReturnsController : ApiControllerBase
{
    private readonly IReturnsService _returnsService;

    public ReturnsController(IReturnsService returnsService)
    {
        _returnsService = returnsService;
    }

    [PermissionAuthorize("Sales.Read")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ReturnDto>>>> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _returnsService.GetPagedAsync(TenantId, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<ReturnDto>>.Ok(result));
    }

    [PermissionAuthorize("Sales.Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ReturnDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _returnsService.GetByIdAsync(TenantId, id, cancellationToken);
        return Ok(ApiResponse<ReturnDto>.Ok(result));
    }

    [PermissionAuthorize("Sales.Return")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReturnDto>>> Create(CreateReturnRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _returnsService.CreateAsync(TenantId, CurrentUserId, request, cancellationToken);
        return Ok(ApiResponse<ReturnDto>.Ok(result));
    }
}
