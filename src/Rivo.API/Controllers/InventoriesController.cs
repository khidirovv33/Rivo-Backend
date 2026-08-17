using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Inventories.Dtos;
using Rivo.Application.Inventories.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/inventories")]
public class InventoriesController : ControllerBase
{
    private readonly IInventoriesService _service;

    public InventoriesController(IInventoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<InventoryDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, warehouseId, cancellationToken);
        return Ok(ApiResponse<PaginatedList<InventoryDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Create(
        [FromBody] CreateInventoryDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<InventoryDto>.Ok(result));
    }

    [HttpPost("{id:guid}/complete")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.CompleteAsync(id, cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result));
    }

    [HttpPost("{id:guid}/approve")]
    [PermissionAuthorize("Inventory.Approve")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ApproveAsync(id, cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.CancelAsync(id, cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result));
    }
}
