namespace Rivo.Application.Assistant.Dtos;

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
}

public class AskAssistantRequestDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class AssistantReplyDto
{
    public string Reply { get; set; } = string.Empty;
}

/// <summary>
/// Личность и права вызывающего сотрудника — нужны, чтобы AI мог выполнять реальные действия
/// (создание сотрудника, запуск ревизии и т.п.) от его имени и с проверкой его прав, а не "от системы".
/// Собирается контроллером из JWT-claims, никогда не приходит от клиента напрямую.
/// </summary>
public record AssistantContext(Guid TenantId, Guid UserId, Guid RoleId);

/// <summary>Описание одного действия, которое AI может предложить модели вызвать (function calling).</summary>
public record AssistantToolDefinition(string Name, string Description, object Parameters);
