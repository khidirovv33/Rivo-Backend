using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Suppliers.Dtos;
using Rivo.Application.Suppliers.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISuppliersService _service;

    public SuppliersController(ISuppliersService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<SupplierDto>>>> GetAll(
        [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<SupplierDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SupplierDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Create(
        [FromBody] CreateSupplierDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SupplierDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(
        Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SupplierDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
