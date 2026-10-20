using ERP.Application.Core;
using Xunit;

namespace ERP.Tests.CustomerDepth;

/// <summary>
/// Smoke coverage for Customer Depth Core masters (SQL Server when connection available).
/// Staging E2E checklist: O2C with Dispatch, P2P with 3-way match, period close, expense claim.
/// </summary>
public class CustomerDepthSmokeTests
{
    [Fact]
    public void PdfDocumentService_BuildsNonEmptyPdf()
    {
        var pdf = new SimplePdfDocumentService();
        var bytes = pdf.BuildSimpleDocument("Sales Invoice SI-1", ["Customer: Acme", "Total: 100.00"]);
        Assert.True(bytes.Length > 100);
        Assert.Equal('%', (char)bytes[0]);
        Assert.Equal('P', (char)bytes[1]);
    }

    [Fact]
    public void SettingsCatalog_ContainsCustomerDepthToggles()
    {
        Assert.NotNull(ERP.Application.Core.Settings.SettingsCatalog.Find("crm.requireSalesmanOnCustomer"));
        Assert.NotNull(ERP.Application.Core.Settings.SettingsCatalog.Find("inventory.requireBin"));
        Assert.NotNull(ERP.Application.Core.Settings.SettingsCatalog.Find("procurement.threeWayMatch"));
        Assert.NotNull(ERP.Application.Core.Settings.SettingsCatalog.Find("sales.allowUnpost"));
        Assert.NotNull(ERP.Application.Core.Settings.SettingsCatalog.Find("finance.enforceBudget"));
        Assert.NotNull(ERP.Application.Core.Settings.SettingsCatalog.Find("finance.requireOpenPeriod"));
    }
}
