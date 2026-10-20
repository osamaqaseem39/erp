using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

/// <summary>
/// Customer Depth: applies additive dated SQL scripts (idempotent IF NOT EXISTS).
/// Avoids EF scaffolded DropColumn/DropTable drift that would risk data loss.
/// Model snapshot is still updated to the current <see cref="Database.ERPDbContext"/> shape.
/// </summary>
public partial class CustomerDepth : Migration
{
    private static readonly string[] Scripts =
    [
        "20261020_CustomerDepth_Core_Masters.sql",
        "20261020_CustomerDepth_CRM_Packaging.sql",
        "20261020_CustomerDepth_Sales_Dispatch.sql",
        "20261020_CustomerDepth_SCM.sql",
        "20261020_CustomerDepth_Procurement.sql",
        "20261020_CustomerDepth_Finance.sql",
        "20261020_CustomerDepth_Manufacturing.sql",
        "20261020_CustomerDepth_Projects.sql",
        "20261020_CustomerDepth_HRM.sql",
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var script in Scripts)
            ExecuteScript(migrationBuilder, script);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Additive depth tables — reverse not auto-generated; drop only if you intentionally roll back.
        // Prefer forward fixes. Scripts are IF NOT EXISTS so re-applying Up is safe.
    }

    private static void ExecuteScript(MigrationBuilder migrationBuilder, string fileName)
    {
        var sql = LoadSchemaSql(fileName);
        foreach (var batch in SplitGoBatches(sql))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            migrationBuilder.Sql(batch);
        }
    }

    private static string LoadSchemaSql(string fileName)
    {
        var asm = typeof(CustomerDepth).Assembly;
        var baseDir = Path.GetDirectoryName(asm.Location) ?? AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(baseDir, "Database", "Schemas", fileName);
        if (File.Exists(path))
            return File.ReadAllText(path);

        // Design-time / source tree fallback
        var repoPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "ERP.Infrastructure", "Database", "Schemas", fileName));
        if (File.Exists(repoPath))
            return File.ReadAllText(repoPath);

        // Embedded-style: walk up from cwd
        var cwd = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(cwd, "ERP.Infrastructure", "Database", "Schemas", fileName),
            Path.Combine(cwd, "Database", "Schemas", fileName),
            Path.Combine(cwd, "..", "ERP.Infrastructure", "Database", "Schemas", fileName),
        };
        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (File.Exists(full))
                return File.ReadAllText(full);
        }

        throw new FileNotFoundException($"Customer Depth schema script not found: {fileName}");
    }

    private static IEnumerable<string> SplitGoBatches(string sql) =>
        Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(b => b.Trim())
            .Where(b => b.Length > 0);
}
