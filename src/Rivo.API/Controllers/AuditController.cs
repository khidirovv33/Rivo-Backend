using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Audit.Dtos;
using Rivo.Application.Audit.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _service;

    public AuditController(IAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<PaginatedList<AuditLogDto>>>> GetAll(
        [FromQuery] PagedRequest request, [FromQuery] string? entityName, [FromQuery] Guid? userId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(request, entityName, userId, from, to, cancellationToken);
        return Ok(ApiResponse<PaginatedList<AuditLogDto>>.Ok(result));
    }
}
