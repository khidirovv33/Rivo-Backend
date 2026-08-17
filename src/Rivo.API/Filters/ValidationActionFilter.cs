using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rivo.Application.Common.Models;

namespace Rivo.API.Filters;

/// <summary>
/// Runs the registered FluentValidation validator (if any) for every action argument before the action executes.
/// Keeps controllers free of manual "if (!ModelState.IsValid)" boilerplate on every endpoint.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", errors));
                return;
            }
        }

        await next();
    }
}
