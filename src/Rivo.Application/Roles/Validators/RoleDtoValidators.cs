using FluentValidation;
using Rivo.Application.Roles.Dtos;

namespace Rivo.Application.Roles.Validators;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PermissionIds).NotNull();
    }
}

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequestDto>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PermissionIds).NotNull();
    }
}
