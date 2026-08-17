using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Rivo.API.Middlewares;

/// <summary>
/// Structured per-request log (method/path/user/status/duration) via Serilog. Distinct from the AuditLog table:
/// this is ops-facing request logging, while AuditSaveChangesInterceptor writes the business-level Who/What/OldValue/NewValue trail.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
        _logger.LogInformation(
            "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms (user={UserId}, ip={Ip})",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            userId,
            context.Connection.RemoteIpAddress);
    }
}
