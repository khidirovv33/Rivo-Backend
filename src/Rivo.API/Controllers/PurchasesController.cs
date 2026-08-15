using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Purchases.Dtos;
using Rivo.Application.Purchases.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/purchases")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchasesService _service;

    public PurchasesController(IPurchasesService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<PurchaseDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] Guid? supplierId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, supplierId, cancellationToken);
        return Ok(ApiResponse<PaginatedList<PurchaseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PurchaseDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseDto>.Ok(result));
    }

    [HttpPost("{id:guid}/payments")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<PurchaseDto>>> RecordPayment(
        Guid id, [FromBody] RecordPaymentDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.RecordPaymentAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PurchaseDto>.Ok(result));
    }
}
