using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Transfers.Dtos;
using Rivo.Application.Transfers.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transfers")]
public class TransfersController : ControllerBase
{
    private readonly ITransfersService _service;

    public TransfersController(ITransfersService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<TransferDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, warehouseId, cancellationToken);
        return Ok(ApiResponse<PaginatedList<TransferDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TransferDto>.Ok(result));
    }

    [HttpPost]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> Create(
        [FromBody] CreateTransferDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<TransferDto>.Ok(result));
    }

    [HttpPost("{id:guid}/submit")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.SubmitAsync(id, cancellationToken);
        return Ok(ApiResponse<TransferDto>.Ok(result));
    }

    [HttpPost("{id:guid}/approve")]
    [PermissionAuthorize("Inventory.Approve")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ApproveAsync(id, cancellationToken);
        return Ok(ApiResponse<TransferDto>.Ok(result));
    }

    [HttpPost("{id:guid}/ship")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> Ship(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ShipAsync(id, cancellationToken);
        return Ok(ApiResponse<TransferDto>.Ok(result));
    }

    [HttpPost("{id:guid}/receive")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> Receive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ReceiveAsync(id, cancellationToken);
        return Ok(ApiResponse<TransferDto>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<TransferDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.CancelAsync(id, cancellationToken);
        return Ok(ApiResponse<TransferDto>.Ok(result));
    }
}
