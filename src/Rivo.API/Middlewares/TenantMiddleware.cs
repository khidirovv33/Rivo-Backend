namespace Rivo.API.Middlewares;

/// <summary>
/// Defense in depth: an authenticated request without a tenant_id claim (malformed/legacy token) is rejected
/// here, before it ever reaches a controller or the EF Core tenant query filter.
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
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out _))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token is missing a valid tenant claim." });
                return;
            }
        }

        await _next(context);
    }
}
