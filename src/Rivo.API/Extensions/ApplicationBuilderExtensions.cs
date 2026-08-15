using Rivo.API.Middlewares;

namespace Rivo.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRivoPipeline(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<TenantMiddleware>();
        return app;
    }
}
