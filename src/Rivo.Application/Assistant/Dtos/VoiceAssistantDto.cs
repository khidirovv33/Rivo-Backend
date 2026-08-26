namespace Rivo.Application.Assistant.Dtos;

/// <summary>
/// Голосовой ответ: то, что распознано из речи пользователя (Transcript), текстовый ответ помощника
/// (тот же function-calling pipeline, что и в текстовом чате) и, если синтез речи доступен, его
/// озвучка. AudioReplyBase64 может быть null — TTS не критичен, отсутствие звука не должно ронять
/// весь запрос, если распознавание речи и сам ответ уже получены.
/// </summary>
public class VoiceAssistantReplyDto
{
    public string Transcript { get; set; } = string.Empty;

    public string Reply { get; set; } = string.Empty;

    public string? AudioReplyBase64 { get; set; }

    public string? AudioReplyContentType { get; set; }
}
