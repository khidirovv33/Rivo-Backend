using Rivo.API.Middlewares;

namespace Rivo.API.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>Must run before everything else so it can catch exceptions from any later middleware/controller.</summary>
    public static IApplicationBuilder UseRivoExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    /// <summary>Must run after UseAuthentication/UseAuthorization — both middlewares read claims off HttpContext.User.</summary>
    public static IApplicationBuilder UseRivoTenantAndAuditLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantMiddleware>();
        app.UseMiddleware<AuditLoggingMiddleware>();
        return app;
    }
}
