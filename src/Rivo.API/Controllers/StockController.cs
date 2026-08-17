using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Stock.Dtos;
using Rivo.Application.Stock.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _service;

    public StockController(IStockService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<StockDto>>>> GetAll(
        [FromQuery] PagedRequest request,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, warehouseId, productId, cancellationToken);
        return Ok(ApiResponse<PaginatedList<StockDto>>.Ok(result));
    }

    [HttpGet("{warehouseId:guid}/{productId:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<StockDto>>> Get(
        Guid warehouseId, Guid productId, [FromQuery] Guid? productVariationId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(warehouseId, productId, productVariationId, cancellationToken);
        return Ok(ApiResponse<StockDto>.Ok(result));
    }

    [HttpPost("reserve")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<StockDto>>> Reserve(
        [FromBody] ReserveStockDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.ReserveAsync(dto, cancellationToken);
        return Ok(ApiResponse<StockDto>.Ok(result));
    }

    [HttpPost("release-reservation")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<StockDto>>> ReleaseReservation(
        [FromBody] ReserveStockDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.ReleaseReservationAsync(dto, cancellationToken);
        return Ok(ApiResponse<StockDto>.Ok(result));
    }
}
