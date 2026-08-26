namespace Rivo.Application.Assistant.Interfaces;

/// <summary>Speech-to-text: превращает аудио запроса сотрудника в текст для того же чат-пайплайна, что и текстовый ввод.</summary>
public interface IVoiceTranscriptionService
{
    Task<string> TranscribeAsync(Stream audio, string fileName, string contentType, CancellationToken cancellationToken = default);
}
