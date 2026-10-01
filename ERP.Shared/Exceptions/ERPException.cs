namespace ERP.Shared.Exceptions;

public class ERPException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public ERPException(string message, string errorCode = "GENERAL_ERROR", int statusCode = 500) 
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public ERPException(string message, Exception innerException, string errorCode = "GENERAL_ERROR", int statusCode = 500) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class ValidationException : ERPException
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message, "VALIDATION_ERROR", 400) 
    {
        Errors = new List<string> { message };
    }

    public ValidationException(string message, List<string> errors) : base(message, "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }

    /// <summary>Errors keyed by request field (camelCase), so forms can show them next to the input.</summary>
    public IReadOnlyDictionary<string, string[]>? FieldErrors { get; private init; }

    public static ValidationException ForField(string field, string message) =>
        new(message) { FieldErrors = new Dictionary<string, string[]> { [field] = [message] } };
}

public class NotFoundException : ERPException
{
    public NotFoundException(string message) : base(message, "NOT_FOUND", 404) { }
}

public class UnauthorizedException : ERPException
{
    public UnauthorizedException(string message) : base(message, "UNAUTHORIZED", 401) { }
}

public class ForbiddenException : ERPException
{
    public ForbiddenException(string message) : base(message, "FORBIDDEN", 403) { }
}

/// <summary>The request conflicts with current state, e.g. a record still referenced by others.</summary>
public class ConflictException : ERPException
{
    public ConflictException(string message) : base(message, "CONFLICT", 409) { }
}
