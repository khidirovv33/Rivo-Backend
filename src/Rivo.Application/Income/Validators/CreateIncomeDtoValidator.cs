using FluentValidation;
using Rivo.Application.Income.Dtos;

namespace Rivo.Application.Income.Validators;

public class CreateIncomeDtoValidator : AbstractValidator<CreateIncomeDto>
{
    public CreateIncomeDtoValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
