namespace Rivo.Application.Assistant.Interfaces;

/// <summary>Text-to-speech: озвучивает финальный текстовый ответ помощника. Необязательная часть — сбой здесь не должен ронять весь ответ.</summary>
public interface IVoiceSynthesisService
{
    /// <returns>Аудио-байты (MP3) и их content-type, либо null если синтез недоступен/выключен.</returns>
    Task<(byte[] Audio, string ContentType)?> SynthesizeAsync(string text, CancellationToken cancellationToken = default);
}
