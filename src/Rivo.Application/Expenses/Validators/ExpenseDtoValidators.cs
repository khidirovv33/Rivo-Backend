using FluentValidation;
using Rivo.Application.Expenses.Dtos;

namespace Rivo.Application.Expenses.Validators;

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseDtoValidator()
    {
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
