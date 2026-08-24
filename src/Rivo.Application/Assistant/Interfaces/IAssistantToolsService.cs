using System.Text.Json;
using Rivo.Application.Assistant.Dtos;

namespace Rivo.Application.Assistant.Interfaces;

/// <summary>
/// Каталог действий, которые AI-помощник может выполнить по запросу сотрудника (function calling),
/// и их исполнение с проверкой прав вызывающего. Провайдер-агностично — используется и Gemini-, и
/// OpenAI-реализациями IAssistantService.
/// </summary>
public interface IAssistantToolsService
{
    /// <summary>Инструменты, доступные конкретной роли (отфильтровано по правам) — то, что уходит в модель.</summary>
    Task<List<AssistantToolDefinition>> GetAvailableToolsAsync(AssistantContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет вызванный моделью инструмент. Возвращает JSON-строку с результатом — она уходит обратно
    /// в модель как functionResponse, чтобы та сформулировала финальный ответ пользователю.
    /// </summary>
    Task<string> ExecuteAsync(string toolName, JsonElement arguments, AssistantContext context, CancellationToken cancellationToken = default);
}
