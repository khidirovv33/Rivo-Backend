using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.PurchaseOrders.Dtos;
using Rivo.Application.PurchaseOrders.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrdersService _service;

    public PurchaseOrdersController(IPurchaseOrdersService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<PurchaseOrderDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] Guid? supplierId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, supplierId, cancellationToken);
        return Ok(ApiResponse<PaginatedList<PurchaseOrderDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Create(
        [FromBody] CreatePurchaseOrderDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PurchaseOrderDto>.Ok(result));
    }

    [HttpPost("{id:guid}/send")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Send(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.SendAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result));
    }

    [HttpPost("{id:guid}/confirm")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ConfirmAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.CancelAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result));
    }
}
