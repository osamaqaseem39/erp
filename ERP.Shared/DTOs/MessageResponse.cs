namespace ERP.Shared.DTOs;

/// <summary>Typed body for endpoints that only return a status message (OpenAPI).</summary>
public sealed class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>Typed body when an endpoint returns a newly created id.</summary>
public sealed class IdResponse
{
    public Guid Id { get; set; }
}

/// <summary>Typed body for simple success flags.</summary>
public sealed class SuccessResponse
{
    public bool Success { get; set; }
}
