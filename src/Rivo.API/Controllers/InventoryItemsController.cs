using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.InventoryItems.Dtos;
using Rivo.Application.InventoryItems.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/inventories/{inventoryId:guid}/items")]
public class InventoryItemsController : ControllerBase
{
    private readonly IInventoryItemsService _service;

    public InventoryItemsController(IInventoryItemsService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<List<InventoryItemDto>>>> GetAll(
        Guid inventoryId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByInventoryAsync(inventoryId, cancellationToken);
        return Ok(ApiResponse<List<InventoryItemDto>>.Ok(result));
    }

    [HttpPost("scan")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<InventoryItemDto>>> Scan(
        Guid inventoryId, [FromBody] ScanInventoryItemDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.ScanAsync(inventoryId, dto, cancellationToken);
        return Ok(ApiResponse<InventoryItemDto>.Ok(result));
    }

    [HttpDelete("{itemId:guid}")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<IActionResult> Remove(Guid inventoryId, Guid itemId, CancellationToken cancellationToken)
    {
        await _service.RemoveAsync(inventoryId, itemId, cancellationToken);
        return NoContent();
    }
}
