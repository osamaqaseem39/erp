using System.Text.RegularExpressions;

namespace ERP.Api.Security.Permissions;

/// <summary>
/// Derives the permission code for an endpoint: &lt;module&gt;.&lt;resource&gt;.&lt;action&gt;.
///  - module and resource come from the route (api/crm/sales-orders → crm.salesOrders), unless the
///    controller declares [PermissionResource];
///  - the action comes from the HTTP method (GET view, POST create, PUT/PATCH update, DELETE delete),
///    except that a non-GET call to a named step after an id is that step:
///    POST api/crm/leads/{id}/convert → crm.leads.convert.
/// </summary>
public static class PermissionConvention
{
    public static readonly IReadOnlyDictionary<string, string> MethodActions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["GET"] = "view",
        ["HEAD"] = "view",
        ["POST"] = "create",
        ["PUT"] = "update",
        ["PATCH"] = "update",
        ["DELETE"] = "delete",
    };

    public static string? Derive(string routeTemplate, string httpMethod, string? resourceOverride)
    {
        var segments = routeTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (segments.Count > 0 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)) segments.RemoveAt(0);
        if (segments.Count == 0) return null;

        string resource;
        List<string> rest;
        if (resourceOverride != null)
        {
            // Steps only count after a parameter, so the whole route can be scanned.
            resource = resourceOverride;
            rest = segments;
        }
        else
        {
            if (segments.Count < 2 || IsParameter(segments[1])) return null; // needs an explicit resource
            resource = $"{Camel(segments[0])}.{Camel(segments[1])}";
            rest = segments.Skip(2).ToList();
        }

        if (!MethodActions.TryGetValue(httpMethod, out var action)) return null;
        if (!action.Equals("view"))
        {
            var seenParameter = false;
            string? step = null;
            foreach (var segment in rest)
            {
                if (IsParameter(segment)) seenParameter = true;
                else if (seenParameter) step = segment;
            }
            if (step != null) action = Camel(step);
        }
        return $"{resource}.{action}";
    }

    private static bool IsParameter(string segment) => segment.StartsWith('{');

    /// <summary>"sales-orders" → "salesOrders", "Chart_Of_Accounts" → "chartOfAccounts".</summary>
    public static string Camel(string segment)
    {
        var parts = Regex.Split(segment, "[-_]").Where(p => p.Length > 0).ToArray();
        if (parts.Length == 0) return segment;
        return char.ToLowerInvariant(parts[0][0]) + parts[0][1..] + string.Concat(parts.Skip(1).Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
