using ERP.Shared.Exceptions;
using System.Net;
using System.Text.Json;

namespace ERP.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "An error occurred while processing your request.",
            errorCode = "INTERNAL_ERROR",
            statusCode = 500
        };

        switch (exception)
        {
            case ValidationException valEx:
                context.Response.StatusCode = 400;
                response = new
                {
                    message = valEx.Message,
                    errorCode = "VALIDATION_ERROR",
                    statusCode = 400
                };
                break;
            case NotFoundException notFoundEx:
                context.Response.StatusCode = 404;
                response = new
                {
                    message = notFoundEx.Message,
                    errorCode = "NOT_FOUND",
                    statusCode = 404
                };
                break;
            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = 401;
                response = new
                {
                    message = unauthorizedEx.Message,
                    errorCode = "UNAUTHORIZED",
                    statusCode = 401
                };
                break;
            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = 403;
                response = new
                {
                    message = forbiddenEx.Message,
                    errorCode = "FORBIDDEN",
                    statusCode = 403
                };
                break;
            case ERPException erpEx:
                context.Response.StatusCode = erpEx.StatusCode;
                response = new
                {
                    message = erpEx.Message,
                    errorCode = erpEx.ErrorCode,
                    statusCode = erpEx.StatusCode
                };
                break;
            default:
                context.Response.StatusCode = 500;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}