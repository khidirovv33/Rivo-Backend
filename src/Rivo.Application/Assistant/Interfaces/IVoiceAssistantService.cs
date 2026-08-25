using Rivo.Application.Assistant.Dtos;

namespace Rivo.Application.Assistant.Interfaces;

/// <summary>
/// Оркестратор голосового помощника: STT -> тот же function-calling пайплайн, что у текстового чата
/// (IAssistantService) -> опциональный TTS. Голос не дублирует логику ассистента, а оборачивает её.
/// </summary>
public interface IVoiceAssistantService
{
    Task<VoiceAssistantReplyDto> AskAsync(
        Stream audio,
        string fileName,
        string contentType,
        List<ChatMessageDto> history,
        AssistantContext context,
        CancellationToken cancellationToken = default);
}
