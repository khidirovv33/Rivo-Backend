using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Barcodes.Dtos;
using Rivo.Application.Barcodes.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/barcodes")]
public class BarcodesController : ControllerBase
{
    private readonly IBarcodesService _service;

    public BarcodesController(IBarcodesService service)
    {
        _service = service;
    }

    [HttpGet("product/{productId:guid}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<BarcodeDto>>>> GetByProduct(
        Guid productId, [FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetByProductAsync(productId, request, cancellationToken);
        return Ok(ApiResponse<PaginatedList<BarcodeDto>>.Ok(result));
    }

    [HttpGet("scan/{code}")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<ActionResult<ApiResponse<BarcodeDto>>> Scan(string code, CancellationToken cancellationToken)
    {
        var result = await _service.ScanAsync(code, cancellationToken);
        return Ok(ApiResponse<BarcodeDto>.Ok(result));
    }

    [HttpPost("generate")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<BarcodeDto>>> Generate(
        [FromBody] GenerateBarcodeDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.GenerateAsync(dto, cancellationToken);
        return Ok(ApiResponse<BarcodeDto>.Ok(result));
    }

    [HttpPost("register")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<ActionResult<ApiResponse<BarcodeDto>>> Register(
        [FromBody] RegisterBarcodeDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.RegisterAsync(dto, cancellationToken);
        return Ok(ApiResponse<BarcodeDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize("Inventory.Create")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/label")]
    [PermissionAuthorize("Inventory.Read")]
    public async Task<IActionResult> GetLabel(Guid id, CancellationToken cancellationToken)
    {
        var png = await _service.GetLabelImageAsync(id, cancellationToken);
        return File(png, "image/png");
    }
}
