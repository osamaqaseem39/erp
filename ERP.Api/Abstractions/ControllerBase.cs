using Microsoft.AspNetCore.Mvc;
using ERP.Shared.Exceptions;
using System.Net;
using FluentValidation;
using ValidationException = ERP.Shared.Exceptions.ValidationException;

namespace ERP.Api.Abstractions;

public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase, IControllerBase
{
    protected readonly ILogger<ControllerBase> _logger;

    protected ControllerBase(ILogger<ControllerBase> logger)
    {
        _logger = logger;
    }

    [NonAction]
    public async Task<IActionResult> HandleAsync<T>(Func<Task<T>> operation) where T : class?
    {
        try
        {
            var result = await operation();
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            return Unauthorized(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Forbidden access");
            return StatusCode((int)HttpStatusCode.Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An unexpected error occurred" });
        }
    }

    [NonAction]
    public async Task<IActionResult> HandleAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return Ok();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            return BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            return Unauthorized(new { message = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Forbidden access");
            return StatusCode((int)HttpStatusCode.Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An unexpected error occurred" });
        }
    }
}