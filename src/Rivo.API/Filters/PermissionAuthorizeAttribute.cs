using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rivo.Application.Common.Interfaces;

namespace Rivo.API.Filters;

/// <summary>
/// Проверка permission-claim'а (например "Inventory.Approve") поверх [Authorize].
/// Контракт claim'а "permission" согласован с Dev1 (Auth/Roles/Permissions issues the JWT).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public PermissionAuthorizeAttribute(string permission)
    {
        _permission = permission;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        if (!currentUser.HasPermission(_permission))
        {
            context.Result = new ObjectResult(new { success = false, message = $"Missing permission: {_permission}" })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        return Task.CompletedTask;
    }
}
