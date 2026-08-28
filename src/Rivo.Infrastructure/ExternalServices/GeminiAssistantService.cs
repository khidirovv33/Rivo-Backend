using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rivo.Application.Assistant.Dtos;
using Rivo.Application.Assistant.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Помощник внутри админки Rivo — проксирует чат к Google Gemini API и умеет выполнять реальные действия
/// в системе (function calling): смотрит список инструментов, доступных роли вызывающего сотрудника
/// (<see cref="IAssistantToolsService"/>), предлагает их модели, и если модель решает вызвать один из
/// них — выполняет его и отдаёт результат обратно модели, чтобы та сформулировала финальный ответ.
/// Ключ живёт только здесь (конфиг/User Secrets на сервере), фронт его никогда не видит. Альтернатива
/// OpenAiAssistantService — какая из двух реализаций активна, решает "Assistant:Provider" в конфиге
/// (см. DependencyInjection.cs).
/// </summary>
public class GeminiAssistantService : IAssistantService
{
    private const string SystemPromptBase =
        "Ты — встроенный AI-помощник в системе Rivo (SaaS POS/ERP для розничной торговли: касса, склад, " +
        "закупки, финансы, клиенты, сотрудники). Помогай сотруднику магазина (владельцу, менеджеру, кассиру, " +
        "бухгалтеру) быстро разобраться с интерфейсом и рабочими вопросами. У тебя есть инструменты для " +
        "выполнения реальных действий в системе — если пользователь просит что-то сделать (например, " +
        "добавить сотрудника или начать ревизию), а подходящий инструмент доступен, вызови его вместо того, " +
        "чтобы просто объяснять, как это сделать вручную. Если для вызова не хватает данных — сначала " +
        "уточни их у пользователя. Отвечай кратко и по делу, без лишних вступлений.";

    // Язык ответа подстраивается под культуру текущего запроса (Accept-Language, см. RequestLocalization
    // в Program.cs) — тот же язык, что выбран переключателем языка на фронте.
    private static string SystemPrompt => SystemPromptBase + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
    {
        "en" => " Respond in English.",
        "tg" => " Ҷавобро бо забони тоҷикӣ деҳ.",
        _ => " Отвечай на русском языке.",
    };

    private const int MaxToolRounds = 4;

    private readonly HttpClient _httpClient;
    private readonly IAssistantToolsService _toolsService;
    private readonly ILogger<GeminiAssistantService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiAssistantService(
        HttpClient httpClient,
        IAssistantToolsService toolsService,
        IConfiguration configuration,
        ILogger<GeminiAssistantService> logger)
    {
        _httpClient = httpClient;
        _toolsService = toolsService;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";
    }

    public async Task<AssistantReplyDto> AskAsync(AskAssistantRequestDto request, AssistantContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured (Gemini:ApiKey).");
        }

        var contents = request.Messages
            .Select(m => new GeminiContent(m.Role == "assistant" ? "model" : "user", [new { text = m.Content }]))
            .ToList();

        var tools = await _toolsService.GetAvailableToolsAsync(context, cancellationToken);

        for (var round = 0; round < MaxToolRounds; round++)
        {
            string responseBody;
            try
            {
                responseBody = await SendAsync(contents, tools, cancellationToken);
            }
            catch (AssistantRateLimitedException ex)
            {
                return new AssistantReplyDto { Reply = ex.Message };
            }

            using var doc = JsonDocument.Parse(responseBody);
            var contentEl = doc.RootElement.GetProperty("candidates")[0].GetProperty("content");
            var parts = contentEl.GetProperty("parts");

            var functionCallPart = FindPart(parts, "functionCall");
            if (functionCallPart is { } fc)
            {
                var name = fc.GetProperty("functionCall").GetProperty("name").GetString() ?? string.Empty;
                // .Clone() detaches these from `doc`/`resultDoc` below, which get disposed at the end of
                // this iteration — without it, serializing `contents` on the *next* round throws
                // ObjectDisposedException("JsonDocument") deep inside System.Text.Json's writer.
                var args = (fc.GetProperty("functionCall").TryGetProperty("args", out var a)
                    ? a
                    : JsonSerializer.SerializeToElement(new { })).Clone();

                var resultJson = await _toolsService.ExecuteAsync(name, args, context, cancellationToken);
                using var resultDoc = JsonDocument.Parse(resultJson);
                var response = resultDoc.RootElement.Clone();

                // "Thinking" models (gemini-3.x) require the thoughtSignature from the functionCall part to
                // be echoed back verbatim on replay, or the next request 400s with "missing thought_signature".
                var thoughtSignature = fc.TryGetProperty("thoughtSignature", out var ts) ? ts.GetString() : null;
                object modelPart = thoughtSignature is null
                    ? new { functionCall = new { name, args } }
                    : new { thoughtSignature, functionCall = new { name, args } };
                contents.Add(new GeminiContent("model", [modelPart]));
                // This model only accepts SYSTEM/USER/MODEL/DEVELOPER/CONTEXT roles — "function" (used by
                // some Gemini API versions/docs) gets rejected with 400 INVALID_ARGUMENT. The function
                // result goes back as a "user" turn instead.
                contents.Add(new GeminiContent("user", [new { functionResponse = new { name, response } }]));
                continue;
            }

            var textPart = FindPart(parts, "text");
            var reply = textPart is { } tp ? tp.GetProperty("text").GetString() ?? string.Empty : string.Empty;
            return new AssistantReplyDto { Reply = reply.Trim() };
        }

        return new AssistantReplyDto { Reply = "Не получилось довести действие до конца за разумное число шагов. Попробуйте переформулировать запрос." };
    }

    private async Task<string> SendAsync(List<GeminiContent> contents, List<AssistantToolDefinition> tools, CancellationToken cancellationToken)
    {
        object payload = tools.Count == 0
            ? new
            {
                systemInstruction = new { parts = new[] { new { text = SystemPrompt } } },
                contents,
                generationConfig = new { temperature = 0.4 },
            }
            : new
            {
                systemInstruction = new { parts = new[] { new { text = SystemPrompt } } },
                contents,
                tools = new[]
                {
                    new
                    {
                        functionDeclarations = tools.Select(t => new { name = t.Name, description = t.Description, parameters = t.Parameters }),
                    },
                },
                generationConfig = new { temperature = 0.4 },
            };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload),
        };
        httpRequest.Headers.Add("x-goog-api-key", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Gemini rate limit hit: {Body}", body);
            throw new AssistantRateLimitedException(
                "Достигнут дневной лимит бесплатного тарифа Gemini API. Попробуйте снова через минуту-две " +
                "(лимит обновляется по времени) или подключите платный тариф в Google AI Studio.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini request failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Не удалось получить ответ от AI-помощника.");
        }

        return body;
    }

    private static JsonElement? FindPart(JsonElement parts, string propertyName)
    {
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty(propertyName, out _))
            {
                return part;
            }
        }

        return null;
    }

    private sealed record GeminiContent(string role, List<object> parts);
}

/// <summary>Google's free-tier daily quota (e.g. 20 req/day on gemini-3.6-flash) — an expected, recoverable
/// condition during testing, not a bug. Caught in AskAsync and turned into a normal chat reply instead of
/// bubbling up as an unhandled 500.</summary>
internal sealed class AssistantRateLimitedException : Exception
{
    public AssistantRateLimitedException(string message) : base(message)
    {
    }
}
