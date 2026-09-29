using System.Diagnostics;
using System.Text.Json;
using System.Security.Claims;
using ERP.Api.Errors;
using ERP.Infrastructure.Logging;

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

    public async Task InvokeAsync(HttpContext context, ISystemLogWriter systemLog)
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
            {
                _logger.LogError(ex, "Unhandled exception {TraceId}", traceId);
                // Tenant admins see their own server errors (with the trace id users are shown) in the system log.
                if (context.Items.TryGetValue("TenantId", out var tenant) && tenant is Guid tenantId)
                {
                    _ = Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
                    await systemLog.WriteAsync("Error", SystemLogSources.Api, $"{context.Request.Method} {context.Request.Path} failed ({error.Status}). Trace {traceId}",
                        tenantId, userId == Guid.Empty ? null : userId, exception: ex.ToString(), method: context.Request.Method,
                        ipAddress: context.Connection.RemoteIpAddress?.ToString(), properties: new { traceId, path = context.Request.Path.Value });
                }
            }
            else
                _logger.LogWarning(ex, "Request failed with {Status} {ErrorCode} {TraceId}", error.Status, error.ErrorCode, traceId);

            context.Response.StatusCode = error.Status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(error, JsonOptions));
        }
    }
}
