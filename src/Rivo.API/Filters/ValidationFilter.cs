using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rivo.Application.Common.Models;

namespace Rivo.API.Filters;

/// <summary>
/// Прогоняет каждый action-параметр через зарегистрированный FluentValidation IValidator&lt;T&gt;,
/// если он есть в DI. Без этого фильтра классы в Application/*/Validators не вызываются нигде —
/// AddValidatorsFromAssembly только регистрирует их в контейнере.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            foreach (var group in result.Errors.GroupBy(e => e.PropertyName))
            {
                errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
            }
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", errors));
            return;
        }

        await next();
    }
}
