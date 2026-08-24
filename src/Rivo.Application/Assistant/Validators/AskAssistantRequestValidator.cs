using FluentValidation;
using Rivo.Application.Assistant.Dtos;

namespace Rivo.Application.Assistant.Validators;

public class AskAssistantRequestValidator : AbstractValidator<AskAssistantRequestDto>
{
    public AskAssistantRequestValidator()
    {
        RuleFor(x => x.Messages).NotEmpty();
        RuleForEach(x => x.Messages).ChildRules(message =>
        {
            message.RuleFor(m => m.Role)
                .NotEmpty()
                .Must(r => r == "user" || r == "assistant")
                .WithMessage("Role must be 'user' or 'assistant'.");
            message.RuleFor(m => m.Content).NotEmpty().MaximumLength(4000);
        });
    }
}
