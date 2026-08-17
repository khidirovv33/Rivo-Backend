using FluentValidation;
using Rivo.Application.Loyalty.Dtos;

namespace Rivo.Application.Loyalty.Validators;

public class CreateLoyaltyLevelRequestValidator : AbstractValidator<CreateLoyaltyLevelRequestDto>
{
    public CreateLoyaltyLevelRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinimumSpend).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateLoyaltyLevelRequestValidator : AbstractValidator<UpdateLoyaltyLevelRequestDto>
{
    public UpdateLoyaltyLevelRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinimumSpend).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
    }
}

public class IssueLoyaltyCardRequestValidator : AbstractValidator<IssueLoyaltyCardRequestDto>
{
    public IssueLoyaltyCardRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
