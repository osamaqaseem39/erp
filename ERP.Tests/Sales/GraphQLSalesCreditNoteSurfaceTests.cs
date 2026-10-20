using ERP.Api.GraphQL.Sales;

namespace ERP.Tests.Sales;

/// <summary>Guards GraphQL Sales credit-note parity with REST.</summary>
public class GraphQLSalesCreditNoteSurfaceTests
{
    [Fact]
    public void QueriesAndMutations_ExposeCreditNoteWorkflow()
    {
        var queries = typeof(SalesQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(SalesQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        var mutations = typeof(SalesMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(SalesMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetSalesCreditNote", queries);
        Assert.Contains("GetSalesCreditNotes", queries);
        Assert.Contains("GetSalesCreditNotesByCustomer", queries);
        Assert.Contains("GetSalesCreditNotesByStatus", queries);

        Assert.Contains("AddSalesCreditNote", mutations);
        Assert.Contains("UpdateSalesCreditNote", mutations);
        Assert.Contains("DeleteSalesCreditNote", mutations);
        Assert.Contains("PostSalesCreditNote", mutations);
        Assert.Contains("VoidSalesCreditNote", mutations);
    }
}
