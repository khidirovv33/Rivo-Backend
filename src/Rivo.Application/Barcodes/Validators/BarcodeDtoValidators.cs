using FluentValidation;
using Rivo.Application.Barcodes.Dtos;

namespace Rivo.Application.Barcodes.Validators;

public class GenerateBarcodeDtoValidator : AbstractValidator<GenerateBarcodeDto>
{
    public GenerateBarcodeDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class RegisterBarcodeDtoValidator : AbstractValidator<RegisterBarcodeDto>
{
    public RegisterBarcodeDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Type).IsInEnum();
    }
}
