using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Receiving.Dtos;
using Rivo.Application.Receiving.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/receiving")]
public class ReceivingController : ControllerBase
{
    private readonly IReceivingService _service;

    public ReceivingController(IReceivingService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<ReceivingDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] Guid? purchaseOrderId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, purchaseOrderId, cancellationToken);
        return Ok(ApiResponse<PaginatedList<ReceivingDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<ReceivingDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ReceivingDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<ReceivingDto>>> Create(
        [FromBody] CreateReceivingDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ReceivingDto>.Ok(result));
    }
}
