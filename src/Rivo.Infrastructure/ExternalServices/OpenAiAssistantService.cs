using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rivo.Application.Assistant.Dtos;
using Rivo.Application.Assistant.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

/// <summary>
/// Помощник внутри админки Rivo — проксирует чат к OpenAI Chat Completions API и умеет выполнять
/// реальные действия в системе (tool calling) через тот же <see cref="IAssistantToolsService"/>, что и
/// GeminiAssistantService. Ключ живёт только здесь (конфиг/User Secrets на сервере), фронт его никогда
/// не видит.
/// </summary>
public class OpenAiAssistantService : IAssistantService
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
    private readonly ILogger<OpenAiAssistantService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiAssistantService(
        HttpClient httpClient,
        IAssistantToolsService toolsService,
        IConfiguration configuration,
        ILogger<OpenAiAssistantService> logger)
    {
        _httpClient = httpClient;
        _toolsService = toolsService;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
    }

    public async Task<AssistantReplyDto> AskAsync(AskAssistantRequestDto request, AssistantContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured (OpenAI:ApiKey).");
        }

        var messages = new List<object> { new { role = "system", content = SystemPrompt } };
        messages.AddRange(request.Messages.Select(m => (object)new { role = m.Role, content = m.Content }));

        var tools = await _toolsService.GetAvailableToolsAsync(context, cancellationToken);

        for (var round = 0; round < MaxToolRounds; round++)
        {
            string responseBody;
            try
            {
                responseBody = await SendAsync(messages, tools, cancellationToken);
            }
            catch (AssistantRateLimitedException ex)
            {
                return new AssistantReplyDto { Reply = ex.Message };
            }

            using var doc = JsonDocument.Parse(responseBody);
            var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

            if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array && toolCalls.GetArrayLength() > 0)
            {
                messages.Add(new
                {
                    role = "assistant",
                    content = (string?)null,
                    tool_calls = toolCalls.EnumerateArray().Select(tc => (object)new
                    {
                        id = tc.GetProperty("id").GetString(),
                        type = "function",
                        function = new
                        {
                            name = tc.GetProperty("function").GetProperty("name").GetString(),
                            arguments = tc.GetProperty("function").GetProperty("arguments").GetString(),
                        },
                    }).ToList(),
                });

                foreach (var toolCall in toolCalls.EnumerateArray())
                {
                    var id = toolCall.GetProperty("id").GetString() ?? string.Empty;
                    var name = toolCall.GetProperty("function").GetProperty("name").GetString() ?? string.Empty;
                    var argumentsJson = toolCall.GetProperty("function").GetProperty("arguments").GetString() ?? "{}";
                    var args = JsonDocument.Parse(argumentsJson).RootElement;

                    var result = await _toolsService.ExecuteAsync(name, args, context, cancellationToken);
                    messages.Add(new { role = "tool", tool_call_id = id, content = result });
                }

                continue;
            }

            var reply = message.TryGetProperty("content", out var contentEl) ? contentEl.GetString() ?? string.Empty : string.Empty;
            return new AssistantReplyDto { Reply = reply.Trim() };
        }

        return new AssistantReplyDto { Reply = "Не получилось довести действие до конца за разумное число шагов. Попробуйте переформулировать запрос." };
    }

    private async Task<string> SendAsync(List<object> messages, List<AssistantToolDefinition> tools, CancellationToken cancellationToken)
    {
        object payload = tools.Count == 0
            ? new { model = _model, messages, temperature = 0.4 }
            : new
            {
                model = _model,
                messages,
                temperature = 0.4,
                tools = tools.Select(t => new
                {
                    type = "function",
                    function = new { name = t.Name, description = t.Description, parameters = t.Parameters },
                }),
            };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = JsonContent.Create(payload),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("OpenAI rate limit hit: {Body}", body);
            throw new AssistantRateLimitedException(
                "Достигнут лимит запросов к OpenAI API (лимит тарифа или квота). Попробуйте снова через " +
                "минуту-две или проверьте баланс/лимиты в кабинете OpenAI.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI request failed: {Status} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Не удалось получить ответ от AI-помощника.");
        }

        return body;
    }
}
