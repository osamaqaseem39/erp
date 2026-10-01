namespace ERP.Api.Security.Permissions;

/// <summary>
/// Overrides the resource part of the permission code for every action in a controller
/// (normally derived from the route: api/crm/sales-orders → crm.salesOrders).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PermissionResourceAttribute(string resource) : Attribute
{
    public string Resource { get; } = resource;
}

/// <summary>The exact permission code an action requires, replacing the derived one.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequirePermissionAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}

/// <summary>
/// Any signed-in user may call this (e.g. reading your own session). A reason is required so every
/// exemption from permission checks is explained where it's declared.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SignedInOnlyAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
