using ERP.Api.GraphQL.HRM;

namespace ERP.Tests.HRM;

public class GraphQLBenefitsUnenrollSurfaceTests
{
    [Fact]
    public void Mutations_ExposeUnenrollEmployeeFromBenefit()
    {
        var mutations = typeof(HRMMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(HRMMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("EnrollEmployeeInBenefit", mutations);
        Assert.Contains("UnenrollEmployeeFromBenefit", mutations);
    }
}
