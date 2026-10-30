using System.Text.RegularExpressions;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

[DbContext(typeof(ERPDbContext))]
[Migration("20261026120000_PayrollStructures")]
public partial class PayrollStructures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ExecuteScript(migrationBuilder, "20261026_Payroll_Structures.sql");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
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
        var asm = typeof(PayrollStructures).Assembly;
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

        throw new FileNotFoundException($"Payroll schema script not found: {fileName}");
    }

    private static IEnumerable<string> SplitGoBatches(string sql) =>
        Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(b => b.Trim())
            .Where(b => b.Length > 0);
}
