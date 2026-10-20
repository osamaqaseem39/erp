using ERP.Api.GraphQL.Admin;

namespace ERP.Tests.Admin;

public class GraphQLPermissionsQueryTests
{
    [Fact]
    public void Surface_ExposesGetPermissions()
    {
        var queries = typeof(AdminQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(AdminQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetPermissions", queries);
        Assert.Contains("GetRolePermissions", queries);
    }
}
