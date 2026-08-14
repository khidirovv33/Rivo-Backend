using FluentValidation;
using Rivo.Application.Brands.Dtos;

namespace Rivo.Application.Brands.Validators;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequestDto>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequestDto>
{
    public UpdateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
