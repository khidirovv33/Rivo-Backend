using FluentValidation;
using Rivo.Application.Stores.Dtos;

namespace Rivo.Application.Stores.Validators;

public class CreateStoreRequestValidator : AbstractValidator<CreateStoreRequestDto>
{
    public CreateStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.DefaultTaxRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class UpdateStoreRequestValidator : AbstractValidator<UpdateStoreRequestDto>
{
    public UpdateStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.DefaultTaxRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequestDto>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequestDto>
{
    public UpdateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).IsInEnum();
    }
}
