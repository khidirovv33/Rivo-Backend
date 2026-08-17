using FluentValidation;
using Rivo.Application.StockMovements.Dtos;

namespace Rivo.Application.StockMovements.Validators;

public class CreateStockMovementDtoValidator : AbstractValidator<CreateStockMovementDto>
{
    public CreateStockMovementDtoValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Quantity).NotEqual(0).WithMessage("Quantity не может быть равен 0.");
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.ReferenceType).MaximumLength(100);
    }
}
