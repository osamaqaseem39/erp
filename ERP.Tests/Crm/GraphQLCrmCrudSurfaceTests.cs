using ERP.Api.GraphQL.CRM;

namespace ERP.Tests.CRM;

/// <summary>Guards GraphQL CRM entity CRUD parity with REST services.</summary>
public class GraphQLCrmCrudSurfaceTests
{
    [Fact]
    public void Mutations_ExposeEntityUpdateDelete_AndMissingCreates()
    {
        var mutations = typeof(CRMMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(CRMMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("UpdateCustomer", mutations);
        Assert.Contains("DeleteCustomer", mutations);
        Assert.Contains("UpdateLead", mutations);
        Assert.Contains("DeleteLead", mutations);
        Assert.Contains("CreateOpportunity", mutations);
        Assert.Contains("UpdateOpportunity", mutations);
        Assert.Contains("DeleteOpportunity", mutations);
        Assert.Contains("CreateQuote", mutations);
        Assert.Contains("UpdateQuote", mutations);
        Assert.Contains("DeleteQuote", mutations);
        Assert.Contains("CreateSalesOrder", mutations);
        Assert.Contains("UpdateSalesOrder", mutations);
        Assert.Contains("DeleteSalesOrder", mutations);
        Assert.Contains("CreateActivity", mutations);
        Assert.Contains("UpdateActivity", mutations);
        Assert.Contains("DeleteActivity", mutations);
        Assert.Contains("DeleteCampaign", mutations);
    }
}
