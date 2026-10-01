using ERP.Shared.DTOs;
using ERP.Shared.Exceptions;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ErpValidationException = ERP.Shared.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace ERP.Api.Errors;

/// <summary>
/// The one place that turns an exception into an HTTP status and <see cref="ApiError"/>.
/// Messages are only passed through when they are written for users (our own exceptions and
/// business-rule checks in ERP code); anything else gets a generic message so internals never leak.
/// </summary>
public static class ApiErrorMapper
{
    public const string GenericMessage = "An unexpected error occurred. Please try again or contact support.";

    public static ApiError Map(Exception exception, string? traceId = null)
    {
        var error = exception switch
        {
            FluentValidationException fv => ValidationError(fv.Errors),
            ErpValidationException v => new ApiError
            {
                Status = 400,
                ErrorCode = ApiErrorCodes.ValidationError,
                Message = v.Message,
                Errors = v.FieldErrors?.ToDictionary(e => e.Key, e => e.Value)
                    ?? (v.Errors.Count > 0 ? new Dictionary<string, string[]> { [""] = v.Errors.ToArray() } : null),
            },
            ERPException erp => new ApiError { Status = erp.StatusCode, ErrorCode = erp.ErrorCode, Message = erp.Message },
            UnauthorizedAccessException => new ApiError
            {
                Status = 401,
                ErrorCode = ApiErrorCodes.Unauthorized,
                Message = "Your session is missing required information. Please sign in again.",
            },
            KeyNotFoundException nf => new ApiError { Status = 404, ErrorCode = ApiErrorCodes.NotFound, Message = nf.Message },
            NotImplementedException => new ApiError
            {
                Status = 501,
                ErrorCode = ApiErrorCodes.NotImplemented,
                Message = "This feature is not available yet.",
            },
            DbUpdateException => new ApiError
            {
                Status = 409,
                ErrorCode = ApiErrorCodes.Conflict,
                Message = "The change conflicts with existing data, for example a duplicate code or a record that is still in use.",
            },
            InvalidOperationException io when IsThrownByErpCode(io) => new ApiError
            {
                Status = 409,
                ErrorCode = ApiErrorCodes.BusinessRule,
                Message = io.Message,
            },
            _ => new ApiError { Status = 500, ErrorCode = ApiErrorCodes.InternalError, Message = GenericMessage },
        };

        // ERPException subclasses carry their own status; never let a 5xx expose its message.
        if (error.Status >= 500 && error.ErrorCode != ApiErrorCodes.NotImplemented)
        {
            error.Message = GenericMessage;
        }

        error.TraceId = traceId;
        return error;
    }

    public static ApiError ValidationError(IEnumerable<ValidationFailure> failures, string message = "Validation failed")
    {
        var errors = failures
            .GroupBy(f => ToCamelCasePath(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        return new ApiError
        {
            Status = 400,
            ErrorCode = ApiErrorCodes.ValidationError,
            Message = message,
            Errors = errors,
        };
    }

    /// <summary>"Lines[0].UnitPrice" -> "lines[0].unitPrice", matching the JSON the client sent.</summary>
    public static string ToCamelCasePath(string? propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath)) return string.Empty;
        return string.Join('.', propertyPath.Split('.').Select(segment =>
            segment.Length == 0 || char.IsLower(segment[0]) ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
    }

    private static bool IsThrownByErpCode(Exception exception)
    {
        var assemblyName = exception.TargetSite?.DeclaringType?.Assembly.GetName().Name;
        return assemblyName is not null && assemblyName.StartsWith("ERP.", StringComparison.Ordinal);
    }
}
