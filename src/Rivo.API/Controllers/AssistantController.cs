using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Rivo.Application.Assistant.Dtos;
using Rivo.Application.Assistant.Interfaces;
using Rivo.Application.Common.Models;

namespace Rivo.API.Controllers;

public class AssistantController : ApiControllerBase
{
    private readonly IAssistantService _assistantService;
    private readonly IVoiceAssistantService _voiceAssistantService;

    public AssistantController(IAssistantService assistantService, IVoiceAssistantService voiceAssistantService)
    {
        _assistantService = assistantService;
        _voiceAssistantService = voiceAssistantService;
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

    /// <summary>
    /// Голосовой вход: аудио-запись вопроса сотрудника (multipart/form-data). Распознаётся в текст,
    /// уходит через тот же пайплайн, что и /chat (включая function calling), и по возможности
    /// озвучивается обратно. historyJson — необязательная JSON-сериализация предыдущих сообщений
    /// диалога (List&lt;ChatMessageDto&gt;) для продолжения разговора несколькими репликами.
    /// </summary>
    [HttpPost("voice")]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<ApiResponse<VoiceAssistantReplyDto>>> Voice(
        IFormFile audio,
        [FromForm] string? historyJson,
        CancellationToken cancellationToken)
    {
        if (audio.Length == 0)
        {
            return BadRequest(ApiResponse<VoiceAssistantReplyDto>.Fail("Audio file is empty."));
        }

        var history = string.IsNullOrWhiteSpace(historyJson)
            ? new List<ChatMessageDto>()
            : JsonSerializer.Deserialize<List<ChatMessageDto>>(historyJson) ?? new List<ChatMessageDto>();

        var context = new AssistantContext(TenantId, CurrentUserId, CurrentRoleId);

        await using var stream = audio.OpenReadStream();
        var result = await _voiceAssistantService.AskAsync(
            stream, audio.FileName, audio.ContentType, history, context, cancellationToken);

        return Ok(ApiResponse<VoiceAssistantReplyDto>.Ok(result));
    }
}
