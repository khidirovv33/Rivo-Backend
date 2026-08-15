using System.Net;
using System.Text.Json;
using Rivo.Application.Common.Models;
using Rivo.Domain.Exceptions;

namespace Rivo.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, response) = ex switch
        {
            ValidationAppException validationEx => (HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("Validation failed.", validationEx.Errors)),
            NotFoundException => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(ex.Message)),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(ex.Message)),
            TenantMismatchException => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(ex.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(ex.Message)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred.")),
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Unhandled exception");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
