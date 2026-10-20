using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

/// <summary>
/// Projects entity depth: members, milestone billing columns, billable timesheet lines,
/// finance dimensions, invoice ProjectId. Applies additive SQL (idempotent).
/// </summary>
[DbContext(typeof(Database.ERPDbContext))]
[Migration("20261020183000_ProjectsEntityDepth")]
public partial class ProjectsEntityDepth : Migration
{
    private const string ScriptFile = "20261020_Projects_EntityDepth.sql";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var sql = LoadSchemaSql(ScriptFile);
        foreach (var batch in SplitGoBatches(sql))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            migrationBuilder.Sql(batch);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Additive depth — reverse not auto-generated; prefer forward fixes.
    }

    private static string LoadSchemaSql(string fileName)
    {
        var asm = typeof(ProjectsEntityDepth).Assembly;
        var baseDir = Path.GetDirectoryName(asm.Location) ?? AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(baseDir, "Database", "Schemas", fileName);
        if (File.Exists(path))
            return File.ReadAllText(path);

        var repoPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "ERP.Infrastructure", "Database", "Schemas", fileName));
        if (File.Exists(repoPath))
            return File.ReadAllText(repoPath);

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

        throw new FileNotFoundException($"Projects entity depth schema script not found: {fileName}");
    }

    private static IEnumerable<string> SplitGoBatches(string sql) =>
        Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(b => b.Trim())
            .Where(b => b.Length > 0);
}
