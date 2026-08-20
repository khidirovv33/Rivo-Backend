using FluentValidation;
using Rivo.Application.Accounts.Dtos;

namespace Rivo.Application.Accounts.Validators;

public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
