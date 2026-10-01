using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using ERP.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ERP.Api.Security.Permissions;

public enum EndpointAccess
{
    /// <summary>Requires the permission in <see cref="EndpointRule.Code"/>.</summary>
    Permission,
    /// <summary>Any signed-in user (declared with [SignedInOnly] and a reason).</summary>
    SignedIn,
    /// <summary>Platform administration ([Authorize(Roles = "SuperAdmin")]).</summary>
    SuperAdmin,
    /// <summary>[AllowAnonymous] (sign-in, setup).</summary>
    Anonymous,
    /// <summary>No permission could be determined: always denied.</summary>
    Unmapped,
}

public sealed record EndpointRule(EndpointAccess Access, string? Code, string HttpMethod, string Route, string Controller, string Action, string? Reason = null);

/// <summary>
/// Resolves the access rule of every controller action and builds the permission catalog from them,
/// so the catalog always matches what is enforced.
/// </summary>
public sealed class EndpointPermissions : IPermissionCatalog
{
    private static readonly Dictionary<string, string> ModuleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = "Admin", ["core"] = "Core", ["crm"] = "CRM", ["sales"] = "Sales / POS", ["finance"] = "Finance", ["hrm"] = "HRM",
        ["inventory"] = "Inventory", ["procurement"] = "Procurement", ["manufacturing"] = "Manufacturing", ["projects"] = "Projects", ["reports"] = "Reports",
    };

    private readonly IActionDescriptorCollectionProvider _actions;
    private readonly ConcurrentDictionary<string, EndpointRule> _rules = new();
    private IReadOnlyList<PermissionDefinition>? _permissions;

    public EndpointPermissions(IActionDescriptorCollectionProvider actions)
    {
        _actions = actions;
    }

    public IReadOnlyList<EndpointRule> AllRules =>
        _actions.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().SelectMany(RulesFor).ToList();

    public IReadOnlyList<PermissionDefinition> Permissions => _permissions ??= AllRules
        .Where(r => r.Access == EndpointAccess.Permission && r.Code != null)
        .Select(r => r.Code!)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(c => c)
        .Select(code =>
        {
            var lastDot = code.LastIndexOf('.');
            var resource = code[..lastDot];
            var action = code[(lastDot + 1)..];
            return new PermissionDefinition(resource, action, Describe(resource, action));
        })
        .ToList();

    public static string ModuleName(string module) => ModuleNames.GetValueOrDefault(module, module);

    /// <summary>The rule for one action (cached).</summary>
    public EndpointRule Resolve(ControllerActionDescriptor action, string httpMethod) =>
        _rules.GetOrAdd($"{action.Id}:{httpMethod}", _ => Build(action, httpMethod));

    private static IEnumerable<EndpointRule> RulesFor(ControllerActionDescriptor action)
    {
        var methods = action.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(c => c.HttpMethods).ToList() ?? [];
        if (methods.Count == 0) methods = ["GET"];
        return methods.Select(m => Build(action, m));
    }

    public static EndpointRule Build(ControllerActionDescriptor action, string httpMethod)
    {
        var method = action.MethodInfo;
        var type = action.ControllerTypeInfo;
        var route = action.AttributeRouteInfo?.Template ?? "";
        EndpointRule Rule(EndpointAccess access, string? code = null, string? reason = null) =>
            new(access, code, httpMethod, route, type.Name, method.Name, reason);

        if (method.GetCustomAttribute<AllowAnonymousAttribute>() != null || type.GetCustomAttribute<AllowAnonymousAttribute>() != null)
            return Rule(EndpointAccess.Anonymous);

        var roles = method.GetCustomAttributes<AuthorizeAttribute>().Concat(type.GetCustomAttributes<AuthorizeAttribute>())
            .Select(a => a.Roles).Where(r => !string.IsNullOrEmpty(r));
        if (roles.Any(r => r!.Contains("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            return Rule(EndpointAccess.SuperAdmin);

        var explicitCode = method.GetCustomAttribute<RequirePermissionAttribute>()?.Code;
        if (explicitCode != null) return Rule(EndpointAccess.Permission, explicitCode);

        var signedIn = method.GetCustomAttribute<SignedInOnlyAttribute>() ?? type.GetCustomAttribute<SignedInOnlyAttribute>();
        if (signedIn != null) return Rule(EndpointAccess.SignedIn, reason: signedIn.Reason);

        var classCode = type.GetCustomAttribute<RequirePermissionAttribute>()?.Code;
        if (classCode != null) return Rule(EndpointAccess.Permission, classCode);

        var code = PermissionConvention.Derive(route, httpMethod, type.GetCustomAttribute<PermissionResourceAttribute>()?.Resource);
        return code == null ? Rule(EndpointAccess.Unmapped) : Rule(EndpointAccess.Permission, code);
    }

    private static string Humanize(string camel) => Regex.Replace(camel, "([a-z])([A-Z])", "$1 $2").ToLowerInvariant();

    private static string Describe(string resource, string action)
    {
        var noun = Humanize(resource[(resource.IndexOf('.') + 1)..]);
        return action switch
        {
            "view" => $"View {noun}",
            "create" => $"Create {noun}",
            "update" => $"Edit {noun}",
            "delete" => $"Delete {noun}",
            _ => $"{char.ToUpperInvariant(noun[0])}{noun[1..]}: {Humanize(action)}",
        };
    }
}
