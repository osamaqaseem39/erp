using ERP.Infrastructure.Services;

namespace ERP.Tests.Infrastructure;

public class DatabaseDeploymentIncrementalSchemaTests
{
    [Fact]
    public void ListIncrementalSchemaFileNames_Orders_Dated_Patches_And_Skips_Module_Schemas()
    {
        var dir = Path.Combine(Path.GetTempPath(), "erp-schema-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "CoreModule_Schema.sql"), "-- base");
            File.WriteAllText(Path.Combine(dir, "20261020_Z_Last.sql"), "-- z");
            File.WriteAllText(Path.Combine(dir, "20261020_A_First.sql"), "-- a");
            File.WriteAllText(Path.Combine(dir, "readme.txt"), "ignore");

            var names = DatabaseDeploymentService.ListIncrementalSchemaFileNames(dir);

            Assert.Equal(new[] { "20261020_A_First.sql", "20261020_Z_Last.sql" }, names);
            Assert.DoesNotContain(names, n => n.Contains("Module_Schema", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void ListIncrementalSchemaFileNames_Empty_When_Missing_Dir()
    {
        Assert.Empty(DatabaseDeploymentService.ListIncrementalSchemaFileNames(
            Path.Combine(Path.GetTempPath(), "erp-missing-" + Guid.NewGuid().ToString("N"))));
    }

    [Fact]
    public void Repo_Schemas_Folder_Contains_Dated_Incremental_Patches()
    {
        var schemas = FindSchemasPath();
        Assert.True(Directory.Exists(schemas), $"Schemas path not found: {schemas}");

        var names = DatabaseDeploymentService.ListIncrementalSchemaFileNames(schemas);
        Assert.NotEmpty(names);
        Assert.Contains(names, n => n.StartsWith("20261020_", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase), names);
    }

    private static string FindSchemasPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "ERP.Infrastructure", "Database", "Schemas");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "Database", "Schemas");
    }
}
