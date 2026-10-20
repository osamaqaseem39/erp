using ERP.Api.GraphQL.Finance;

namespace ERP.Tests.Finance;

public class GraphQLChequeRegisterSurfaceTests
{
    [Fact]
    public void Queries_ExposeChequeRegister()
    {
        var queries = typeof(FinanceQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(FinanceQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("GetChequeRegister", queries);
    }
}
