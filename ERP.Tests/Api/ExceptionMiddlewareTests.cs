using System.Text.Json;
using ERP.Api.Middlewares;
using ERP.Infrastructure.Logging;
using ERP.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Api;

public class ExceptionMiddlewareTests
{
    private readonly RecordingLog _log = new();

    [Fact]
    public async Task WritesApiErrorWithStatusAndCamelCaseJson()
    {
        var middleware = new ExceptionMiddleware(
            _ => throw new KeyNotFoundException("Vendor with ID 'v1' not found."),
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _log);

        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"errorCode\":\"NOT_FOUND\"", json);
        var body = JsonSerializer.Deserialize<ApiError>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(404, body.Status);
        Assert.Equal("Vendor with ID 'v1' not found.", body.Message);
        Assert.False(string.IsNullOrEmpty(body.TraceId));
    }

    [Fact]
    public async Task PassesThroughWhenNothingThrows()
    {
        var middleware = new ExceptionMiddleware(ctx => { ctx.Response.StatusCode = 204; return Task.CompletedTask; },
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, _log);

        Assert.Equal(204, context.Response.StatusCode);
    }

    [Fact]
    public async Task ServerErrorsAreWrittenToTheTenantsSystemLog_WithTheTraceIdTheUserSees()
    {
        var tenant = Guid.NewGuid();
        var middleware = new ExceptionMiddleware(_ => throw new InvalidCastException("boom"), NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Items["TenantId"] = tenant;
        context.Request.Method = "POST";
        context.Request.Path = "/api/crm/customers";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _log);

        var entry = Assert.Single(_log.Entries);
        Assert.Equal(("Error", tenant), (entry.Level, entry.Tenant));
        Assert.Contains("POST /api/crm/customers failed (500)", entry.Message);
        context.Response.Body.Position = 0;
        var body = JsonSerializer.Deserialize<ApiError>(await new StreamReader(context.Response.Body).ReadToEndAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Contains(body.TraceId!, entry.Message);
    }

    [Fact]
    public async Task ClientErrorsAreNotSystemLogged()
    {
        var middleware = new ExceptionMiddleware(_ => throw new KeyNotFoundException("x"), NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Items["TenantId"] = Guid.NewGuid();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, _log);

        Assert.Empty(_log.Entries);
    }

    private sealed class RecordingLog : ISystemLogWriter
    {
        public List<(string Level, string Message, Guid Tenant)> Entries { get; } = new();

        public Task WriteAsync(string level, string source, string message, Guid tenantId, Guid? userId = null,
            string? exception = null, string? method = null, string? ipAddress = null, object? properties = null)
        {
            Entries.Add((level, message, tenantId));
            return Task.CompletedTask;
        }
    }
}
