using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Payments.Dtos;
using Rivo.Application.Payments.Interfaces;

namespace Rivo.API.Controllers;

public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [PermissionAuthorize("Payments.Read")]
    [HttpGet("by-order/{orderId:guid}")]
    public async Task<ActionResult<Application.Common.Models.ApiResponse<List<PaymentDto>>>> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _paymentsService.GetByOrderIdAsync(TenantId, orderId, cancellationToken);
        return Ok(Application.Common.Models.ApiResponse<List<PaymentDto>>.Ok(result));
    }
}
