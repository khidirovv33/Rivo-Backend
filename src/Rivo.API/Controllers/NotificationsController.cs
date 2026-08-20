using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.Application.Common.Models;
using Rivo.Application.Notifications.Dtos;
using Rivo.Application.Notifications.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsService _service;

    public NotificationsController(INotificationsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<NotificationDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] bool? unreadOnly, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, unreadOnly, cancellationToken);
        return Ok(ApiResponse<PaginatedList<NotificationDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _service.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _service.MarkAllAsReadAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("low-stock-check")]
    public async Task<ActionResult<ApiResponse<int>>> RunLowStockCheck(CancellationToken cancellationToken)
    {
        var created = await _service.RunLowStockCheckAsync(cancellationToken);
        return Ok(ApiResponse<int>.Ok(created, $"{created} new low-stock notification(s)."));
    }
}
