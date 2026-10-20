using ERP.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.OpenApi;

/// <summary>Reusable OpenAPI response metadata (Platform, Sales, Procurement, Finance, CRM, Admin, …).</summary>
public static class OpenApiResponseTypes
{
    public const int Ok = StatusCodes.Status200OK;
    public const int Created = StatusCodes.Status201Created;
    public const int NoContent = StatusCodes.Status204NoContent;
    public const int BadRequest = StatusCodes.Status400BadRequest;
    public const int Unauthorized = StatusCodes.Status401Unauthorized;
    public const int Forbidden = StatusCodes.Status403Forbidden;
    public const int NotFound = StatusCodes.Status404NotFound;
    public const int Conflict = StatusCodes.Status409Conflict;
}

/// <summary>401 + 403 for [Authorize] controllers.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ProducesUnauthorizedAttribute : ProducesResponseTypeAttribute
{
    public ProducesUnauthorizedAttribute() : base(typeof(ApiError), OpenApiResponseTypes.Unauthorized) { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ProducesForbiddenAttribute : ProducesResponseTypeAttribute
{
    public ProducesForbiddenAttribute() : base(typeof(ApiError), OpenApiResponseTypes.Forbidden) { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ProducesBadRequestAttribute : ProducesResponseTypeAttribute
{
    public ProducesBadRequestAttribute() : base(typeof(ApiError), OpenApiResponseTypes.BadRequest) { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ProducesNotFoundAttribute : ProducesResponseTypeAttribute
{
    public ProducesNotFoundAttribute() : base(typeof(ApiError), OpenApiResponseTypes.NotFound) { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ProducesConflictAttribute : ProducesResponseTypeAttribute
{
    public ProducesConflictAttribute() : base(typeof(ApiError), OpenApiResponseTypes.Conflict) { }
}
