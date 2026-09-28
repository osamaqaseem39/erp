using System.Text.Json;
using ERP.Api.Middlewares;
using ERP.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests.Api;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task WritesApiErrorWithStatusAndCamelCaseJson()
    {
        var middleware = new ExceptionMiddleware(
            _ => throw new KeyNotFoundException("Vendor with ID 'v1' not found."),
            NullLogger<ExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

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

        await middleware.InvokeAsync(context);

        Assert.Equal(204, context.Response.StatusCode);
    }
}
