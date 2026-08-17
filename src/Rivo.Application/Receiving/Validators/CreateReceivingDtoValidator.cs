using FluentValidation;
using Rivo.Application.Receiving.Dtos;

namespace Rivo.Application.Receiving.Validators;

public class CreateReceivingDtoValidator : AbstractValidator<CreateReceivingDto>
{
    public CreateReceivingDtoValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Укажите хотя бы одну позицию для получения.");
        RuleForEach(x => x.Items).SetValidator(new CreateReceivingItemDtoValidator());
    }
}

public class CreateReceivingItemDtoValidator : AbstractValidator<CreateReceivingItemDto>
{
    public CreateReceivingItemDtoValidator()
    {
        RuleFor(x => x.PurchaseOrderItemId).NotEmpty();
        RuleFor(x => x.QuantityReceived).GreaterThan(0);
    }
}
