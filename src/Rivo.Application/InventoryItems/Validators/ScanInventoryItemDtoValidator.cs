using FluentValidation;
using Rivo.Application.InventoryItems.Dtos;

namespace Rivo.Application.InventoryItems.Validators;

public class ScanInventoryItemDtoValidator : AbstractValidator<ScanInventoryItemDto>
{
    public ScanInventoryItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ActualQuantity).GreaterThanOrEqualTo(0);
    }
}
