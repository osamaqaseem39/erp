using ERP.Infrastructure.Security;

namespace ERP.Api.Security.Permissions;

/// <summary>
/// Endpoint-derived permissions plus extras that are not bound to a single controller action
/// (e.g. per-report view rights used to hide individual reports in the catalog).
/// </summary>
public sealed class CompositePermissionCatalog : IPermissionCatalog
{
    public CompositePermissionCatalog(EndpointPermissions endpoints, IEnumerable<PermissionDefinition> extras)
    {
        Permissions = endpoints.Permissions
            .Concat(extras)
            .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<PermissionDefinition> Permissions { get; }
}

/// <summary>Reference-video style report rights (seeded into each tenant; enforced when listing/running reports).</summary>
public static class ReportPermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        Def("reports.stockAverage", "view", "Stock average report"),
        Def("reports.stockLedger", "view", "Stock ledger report"),
        Def("reports.salesSummary", "view", "Sales summary report"),
        Def("reports.purchaseSummary", "view", "Purchase summary report"),
        Def("reports.profitAndLoss", "view", "Profit and loss report"),
        Def("reports.trialBalance", "view", "Trial balance report"),
        Def("reports.balanceSheet", "view", "Balance sheet report"),
        Def("reports.arAging", "view", "AR aging report"),
        Def("reports.apAging", "view", "AP aging report"),
        Def("reports.saleOrderSchedule", "view", "Sale order schedule report"),
        Def("reports.pendingSoProduction", "view", "Pending SO for production report"),
        Def("reports.operational", "view", "Operational sales/purchase/stock reports"),
        Def("reports.operational", "create", "Run operational reports"),
        Def("reports.print", "view", "Document PDF prints (SO/SI/PO/DA/vouchers)"),
    ];

    private static PermissionDefinition Def(string resource, string action, string description) =>
        new(resource, action, description);
}
