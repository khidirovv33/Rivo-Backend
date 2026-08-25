using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Rivo.Application.Assistant.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>Text-to-speech через OpenAI (audio/speech). Возвращает MP3. Тот же OpenAI:ApiKey, что и транскрипция.</summary>
public class OpenAiVoiceSynthesisService : IVoiceSynthesisService
{
    private const string ContentType = "audio/mpeg";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _voice;

    public OpenAiVoiceSynthesisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _model = configuration["OpenAI:TtsModel"] ?? "tts-1";
        _voice = configuration["OpenAI:TtsVoice"] ?? "alloy";
    }

    public async Task<(byte[] Audio, string ContentType)?> SynthesizeAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/speech")
        {
            Content = JsonContent.Create(new { model = _model, voice = _voice, input = text, response_format = "mp3" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"OpenAI speech synthesis failed ({(int)response.StatusCode}): {error}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return (bytes, ContentType);
    }
}
