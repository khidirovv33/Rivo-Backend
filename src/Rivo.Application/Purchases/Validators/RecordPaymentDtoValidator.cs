using FluentValidation;
using Rivo.Application.Purchases.Dtos;

namespace Rivo.Application.Purchases.Validators;

public class RecordPaymentDtoValidator : AbstractValidator<RecordPaymentDto>
{
    public RecordPaymentDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
