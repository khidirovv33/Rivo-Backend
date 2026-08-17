using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Permissions.Interfaces;

namespace Rivo.API.Filters;

/// <summary>
/// Checks the caller's role against the live permission catalog (not baked into the JWT, so revoking a
/// permission takes effect immediately instead of waiting for the token to expire). Usage: [PermissionAuthorize("Products.Create")].
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PermissionAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _permission;

    public PermissionAuthorizeAttribute(string permission)
    {
        _permission = permission;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var roleIdClaim = context.HttpContext.User.FindFirst("role_id")?.Value;
        if (!Guid.TryParse(roleIdClaim, out var roleId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissionsRepository = context.HttpContext.RequestServices.GetRequiredService<IPermissionsRepository>();
        var permissions = await permissionsRepository.GetByRoleIdAsync(roleId, context.HttpContext.RequestAborted);

        if (!permissions.Any(p => p.Name == _permission))
        {
            context.Result = new ObjectResult(ApiResponse<object>.Fail($"Missing permission: {_permission}"))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
