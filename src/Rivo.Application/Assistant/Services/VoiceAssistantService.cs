using Microsoft.Extensions.Logging;
using Rivo.Application.Assistant.Dtos;
using Rivo.Application.Assistant.Interfaces;

namespace Rivo.Application.Assistant.Services;

public class VoiceAssistantService : IVoiceAssistantService
{
    private readonly IVoiceTranscriptionService _transcription;
    private readonly IAssistantService _assistant;
    private readonly IVoiceSynthesisService _synthesis;
    private readonly ILogger<VoiceAssistantService> _logger;

    public VoiceAssistantService(
        IVoiceTranscriptionService transcription,
        IAssistantService assistant,
        IVoiceSynthesisService synthesis,
        ILogger<VoiceAssistantService> logger)
    {
        _transcription = transcription;
        _assistant = assistant;
        _synthesis = synthesis;
        _logger = logger;
    }

    public async Task<VoiceAssistantReplyDto> AskAsync(
        Stream audio,
        string fileName,
        string contentType,
        List<ChatMessageDto> history,
        AssistantContext context,
        CancellationToken cancellationToken = default)
    {
        var transcript = await _transcription.TranscribeAsync(audio, fileName, contentType, cancellationToken);

        var messages = new List<ChatMessageDto>(history) { new() { Role = "user", Content = transcript } };
        var reply = await _assistant.AskAsync(new AskAssistantRequestDto { Messages = messages }, context, cancellationToken);

        var result = new VoiceAssistantReplyDto { Transcript = transcript, Reply = reply.Reply };

        try
        {
            var spoken = await _synthesis.SynthesizeAsync(reply.Reply, cancellationToken);
            if (spoken.HasValue)
            {
                result.AudioReplyBase64 = Convert.ToBase64String(spoken.Value.Audio);
                result.AudioReplyContentType = spoken.Value.ContentType;
            }
        }
        catch (Exception ex)
        {
            // Озвучка — бонус поверх текстового ответа; сотрудник всё равно получает Reply как текст.
            _logger.LogWarning(ex, "Voice reply synthesis failed; returning text-only reply.");
        }

        return result;
    }
}
