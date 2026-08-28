using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Rivo.API.Resources;
using Rivo.Application.Common.Models;
using Rivo.Domain.Exceptions;

namespace Rivo.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IStringLocalizer<SharedResources> localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(exception.Message)),
            ValidationAppException validationEx => (HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(_localizer["ValidationFailed"], validationEx.Errors.SelectMany(e => e.Value))),
            AuthenticationFailedException => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(exception.Message)),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(exception.Message)),
            TenantMismatchException => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(exception.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(_localizer["Unauthorized"])),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail(_localizer["UnexpectedError"]))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
