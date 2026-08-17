using FluentValidation;
using Rivo.Application.Pos.Dtos;

namespace Rivo.Application.Pos.Validators;

public class SendReceiptRequestValidator : AbstractValidator<SendReceiptRequestDto>
{
    public SendReceiptRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequestDto>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.DiscountAmount).GreaterThanOrEqualTo(0);
        });
        RuleFor(x => x.Payments).NotEmpty();
        RuleForEach(x => x.Payments).ChildRules(payment =>
        {
            payment.RuleFor(p => p.Amount).GreaterThan(0);
            payment.RuleFor(p => p.Method).IsInEnum();
        });
    }
}
