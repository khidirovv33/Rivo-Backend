using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Warehouses.Dtos;
using Rivo.Application.Warehouses.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehousesService _service;

    public WarehousesController(IWarehousesService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<WarehouseDto>>>> GetAll(
        [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<WarehouseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<WarehouseDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Create(
        [FromBody] CreateWarehouseDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WarehouseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Update(
        Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<WarehouseDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
