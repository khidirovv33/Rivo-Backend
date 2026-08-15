using FluentValidation;
using Rivo.Application.Inventories.Dtos;

namespace Rivo.Application.Inventories.Validators;

public class CreateInventoryDtoValidator : AbstractValidator<CreateInventoryDto>
{
    public CreateInventoryDtoValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
    }
}
