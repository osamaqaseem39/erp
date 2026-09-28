using System.Diagnostics;
using System.Text.Json;
using ERP.Api.Errors;

namespace ERP.Api.Middlewares;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Unhandled exception after the response started");
                throw;
            }

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var error = ApiErrorMapper.Map(ex, traceId);

            if (error.Status >= 500)
                _logger.LogError(ex, "Unhandled exception {TraceId}", traceId);
            else
                _logger.LogWarning(ex, "Request failed with {Status} {ErrorCode} {TraceId}", error.Status, error.ErrorCode, traceId);

            context.Response.StatusCode = error.Status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(error, JsonOptions));
        }
    }
}
