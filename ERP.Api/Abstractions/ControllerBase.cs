using ERP.Api.Errors;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Abstractions;

public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase, IControllerBase
{
    protected readonly ILogger<ControllerBase> _logger;

    protected ControllerBase(ILogger<ControllerBase> logger)
    {
        _logger = logger;
    }

    // Exceptions are not caught here: ExceptionMiddleware maps every one of them through ApiErrorMapper,
    // so all endpoints share the same status codes and error body.
    [NonAction]
    public async Task<IActionResult> HandleAsync<T>(Func<Task<T>> operation) where T : class?
    {
        var result = await operation();
        return Ok(result);
    }

    [NonAction]
    public async Task<IActionResult> HandleAsync(Func<Task> operation)
    {
        await operation();
        return Ok();
    }

    /// <summary>400 with field-level errors from a FluentValidation result.</summary>
    [NonAction]
    public IActionResult ValidationFailed(ValidationResult result)
    {
        var error = ApiErrorMapper.ValidationError(result.Errors);
        error.TraceId = HttpContext?.TraceIdentifier;
        return BadRequest(error);
    }
}
