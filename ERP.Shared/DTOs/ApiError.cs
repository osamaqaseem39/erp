namespace ERP.Shared.DTOs;

/// <summary>
/// The single error body returned by every API endpoint.
/// Field keys in <see cref="Errors"/> are camelCase to match the frontend DTOs; "" holds errors not tied to a field.
/// </summary>
public class ApiError
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = ApiErrorCodes.InternalError;
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }
}

public static class ApiErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string BusinessRule = "BUSINESS_RULE";
    public const string NotImplemented = "NOT_IMPLEMENTED";
    public const string InternalError = "INTERNAL_ERROR";
}
