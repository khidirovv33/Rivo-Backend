using Rivo.Application.Assistant.Dtos;

namespace Rivo.Application.Assistant.Interfaces;

public interface IAssistantService
{
    Task<AssistantReplyDto> AskAsync(AskAssistantRequestDto request, AssistantContext context, CancellationToken cancellationToken = default);
}
