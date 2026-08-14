using FluentValidation;
using Rivo.Application.Returns.Dtos;

namespace Rivo.Application.Returns.Validators;

public class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequestDto>
{
    public CreateReturnRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.OrderItemId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
