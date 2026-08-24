using Microsoft.AspNetCore.Mvc;
using Rivo.Application.Assistant.Dtos;
using Rivo.Application.Assistant.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

public class AssistantController : ApiControllerBase
{
    private readonly IAssistantService _assistantService;

    public AssistantController(IAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    // Без PermissionAuthorize — AI-помощник доступен любому авторизованному сотруднику, не только
    // ролям с конкретными правами.
    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<AssistantReplyDto>>> Chat(
        [FromBody] AskAssistantRequestDto request,
        CancellationToken cancellationToken)
    {
        var context = new AssistantContext(TenantId, CurrentUserId, CurrentRoleId);
        var result = await _assistantService.AskAsync(request, context, cancellationToken);
        return Ok(ApiResponse<AssistantReplyDto>.Ok(result));
    }
}
