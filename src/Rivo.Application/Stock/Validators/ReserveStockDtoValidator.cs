using FluentValidation;
using Rivo.Application.Stock.Dtos;

namespace Rivo.Application.Stock.Validators;

public class ReserveStockDtoValidator : AbstractValidator<ReserveStockDto>
{
    public ReserveStockDtoValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
