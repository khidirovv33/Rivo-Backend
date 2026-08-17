using FluentValidation;
using Rivo.Application.Products.Dtos;

namespace Rivo.Application.Products.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequestDto>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        RuleFor(x => x.Unit).NotEmpty();
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequestDto>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class CreateProductVariationRequestValidator : AbstractValidator<CreateProductVariationRequestDto>
{
    public CreateProductVariationRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProductVariationRequestValidator : AbstractValidator<UpdateProductVariationRequestDto>
{
    public UpdateProductVariationRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
    }
}
