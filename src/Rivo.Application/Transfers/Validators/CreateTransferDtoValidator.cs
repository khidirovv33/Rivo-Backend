using FluentValidation;
using Rivo.Application.Transfers.Dtos;

namespace Rivo.Application.Transfers.Validators;

public class CreateTransferDtoValidator : AbstractValidator<CreateTransferDto>
{
    public CreateTransferDtoValidator()
    {
        RuleFor(x => x.SourceWarehouseId).NotEmpty();
        RuleFor(x => x.DestinationWarehouseId).NotEmpty()
            .NotEqual(x => x.SourceWarehouseId).WithMessage("Склад-источник и склад-получатель не могут совпадать.");
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateTransferItemDtoValidator());
    }
}

public class CreateTransferItemDtoValidator : AbstractValidator<CreateTransferItemDto>
{
    public CreateTransferItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
