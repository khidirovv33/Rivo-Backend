using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Orders.Dtos;
using Rivo.Application.Pos.Dtos;
using Rivo.Application.Pos.Interfaces;

namespace Rivo.API.Controllers;

public class PosController : ApiControllerBase
{
    private readonly IPosService _posService;

    public PosController(IPosService posService)
    {
        _posService = posService;
    }

    [PermissionAuthorize("Sales.Create")]
    [HttpPost("checkout")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Checkout(CheckoutRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _posService.CheckoutAsync(TenantId, CurrentUserId, request, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [PermissionAuthorize("Sales.Read")]
    [HttpGet("receipts/{orderId:guid}")]
    public async Task<IActionResult> GetReceiptPdf(Guid orderId, CancellationToken cancellationToken)
    {
        var pdfBytes = await _posService.GenerateReceiptPdfAsync(TenantId, orderId, cancellationToken);
        return File(pdfBytes, "application/pdf", $"receipt-{orderId}.pdf");
    }
}
