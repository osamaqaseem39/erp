using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ERP.Infrastructure.Database;

/// <summary>
/// Lets `dotnet ef migrations add --project ERP.Infrastructure --startup-project ERP.Infrastructure` build the
/// model without starting (or rebuilding) the API. It never connects: adding a migration only compares models.
/// Apply migrations with the API's real configuration.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ERPDbContext>
{
    public ERPDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ERPDbContext>()
            .UseSqlServer("Server=design-time-only;Database=erp;Trusted_Connection=True;TrustServerCertificate=True")
            .Options);
}
