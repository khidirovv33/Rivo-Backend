namespace Rivo.API.Middlewares;

/// <summary>
/// Требует TenantId в JWT-claim'ах аутентифицированного пользователя (кроме anonymous-эндпоинтов,
/// например /api/auth/login). Реальное чтение tenant'а — в Infrastructure/Multitenancy/TenantService
/// (ICurrentTenantService), это лишь guard, чтобы запрос без tenant_id не прошёл дальше.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;

        if (!allowAnonymous && context.User.Identity?.IsAuthenticated == true)
        {
            var hasTenant = context.User.Claims.Any(c => c.Type == "tenant_id");
            if (!hasTenant)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Missing tenant context.");
                return;
            }
        }

        await _next(context);
    }
}
