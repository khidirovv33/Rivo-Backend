using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rivo.Application.Assistant.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Speech-to-text через мультимодальный вход Gemini (тот же бесплатный Gemini:ApiKey, что и текстовый чат) —
/// используется вместо платного OpenAI Whisper, когда Assistant:Provider = "Gemini" (см. DependencyInjection.cs).
/// </summary>
public class GeminiVoiceTranscriptionService : IVoiceTranscriptionService
{
    private const string TranscribePrompt =
        "Transcribe this audio recording exactly as spoken, in the original language. " +
        "Return only the transcript text, with no extra commentary, quotes, or formatting.";

    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiVoiceTranscriptionService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiVoiceTranscriptionService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiVoiceTranscriptionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
    }

    public async Task<string> TranscribeAsync(Stream audio, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured (Gemini:ApiKey) — required for voice transcription.");
        }

        using var memoryStream = new MemoryStream();
        await audio.CopyToAsync(memoryStream, cancellationToken);
        var base64Audio = Convert.ToBase64String(memoryStream.ToArray());
        var mimeType = string.IsNullOrWhiteSpace(contentType) ? "audio/webm" : contentType;

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { inline_data = new { mime_type = mimeType, data = base64Audio } },
                        new { text = TranscribePrompt },
                    },
                },
            },
            generationConfig = new { temperature = 0.0 },
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(payload) };
        request.Headers.Add("x-goog-api-key", _apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Gemini transcription rate limit hit: {Body}", body);
            throw new InvalidOperationException(
                "Достигнут дневной лимит бесплатного тарифа Gemini API. Попробуйте снова через минуту-две.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini transcription failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Gemini transcription failed ({(int)response.StatusCode}).");
        }

        using var document = JsonDocument.Parse(body);
        var parts = document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts");
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textEl))
            {
                return textEl.GetString()?.Trim() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
