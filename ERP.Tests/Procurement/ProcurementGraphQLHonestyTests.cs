using ERP.Api.GraphQL.Procurement;

namespace ERP.Tests.Procurement;

/// <summary>Guards against reintroducing GraphQL mutations that lie about paid/submit/issue.</summary>
public class ProcurementGraphQLHonestyTests
{
    [Fact]
    public void WorkflowMutations_MatchRealRestActions()
    {
        var names = typeof(ProcurementMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ApprovePurchaseInvoice", names);
        Assert.Contains("RejectPurchaseInvoice", names);
        Assert.Contains("ApproveGoodsReceiptNote", names);
        Assert.Contains("RejectGoodsReceiptNote", names);
        Assert.Contains("ApprovePurchaseOrder", names);
        Assert.Contains("RejectPurchaseOrder", names);
        Assert.Contains("ApproveVendor", names);
        Assert.Contains("SuspendVendor", names);
        Assert.Contains("CompletePurchaseReturn", names);
        Assert.Contains("PostPurchaseDebitNote", names);

        Assert.DoesNotContain("MarkInvoiceAsPaid", names);
        Assert.DoesNotContain("MarkInvoiceAsPartiallyPaid", names);
        Assert.DoesNotContain("SubmitPurchaseRequisition", names);
        Assert.DoesNotContain("IssuePurchaseOrder", names);
        Assert.DoesNotContain("VerifyGoodsReceiptNote", names);
        Assert.DoesNotContain("PostPurchaseInvoice", names);
        Assert.DoesNotContain("CancelPurchaseOrder", names);
    }
}
