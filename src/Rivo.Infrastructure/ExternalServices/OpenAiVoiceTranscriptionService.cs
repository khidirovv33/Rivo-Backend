using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Rivo.Application.Assistant.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Speech-to-text через OpenAI Whisper (audio/transcriptions). Используется независимо от того, какой
/// провайдер ("Assistant:Provider") обслуживает сам текстовый чат — распознавание речи не завязано на
/// выбор модели для диалога, поэтому всегда идёт через OpenAI:ApiKey (тот же ключ, что и у
/// OpenAiAssistantService, если он сконфигурирован).
/// </summary>
public class OpenAiVoiceTranscriptionService : IVoiceTranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiVoiceTranscriptionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _model = configuration["OpenAI:TranscriptionModel"] ?? "whisper-1";
    }

    public async Task<string> TranscribeAsync(Stream audio, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured (OpenAI:ApiKey) — required for voice transcription.");
        }

        using var content = new MultipartFormDataContent();
        using var audioContent = new StreamContent(audio);
        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        content.Add(audioContent, "file", string.IsNullOrWhiteSpace(fileName) ? "audio.webm" : fileName);
        content.Add(new StringContent(_model), "model");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI transcription failed ({(int)response.StatusCode}): {body}");
        }

        using var document = System.Text.Json.JsonDocument.Parse(body);
        return document.RootElement.GetProperty("text").GetString() ?? string.Empty;
    }
}
