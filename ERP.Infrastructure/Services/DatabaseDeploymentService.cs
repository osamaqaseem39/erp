using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Infrastructure.Database;

namespace ERP.Infrastructure.Services;

public class DatabaseDeploymentService
{
    private readonly ERPDbContext _context;
    private readonly string _connectionString;
    private readonly ILogger<DatabaseDeploymentService> _logger;
    private readonly string _schemasPath;
    private readonly string _storedProceduresPath;

    public DatabaseDeploymentService(
        ERPDbContext context,
        string connectionString,
        ILogger<DatabaseDeploymentService> logger)
    {
        _context = context;
        _connectionString = connectionString;
        _logger = logger;
        
        // Get paths relative to the Infrastructure project
        var basePath = Path.GetDirectoryName(typeof(DatabaseDeploymentService).Assembly.Location) 
            ?? AppDomain.CurrentDomain.BaseDirectory;
        _schemasPath = Path.Combine(basePath, "Database", "Schemas");
        _storedProceduresPath = Path.Combine(basePath, "Database", "StoredProcedures");
    }

    public async Task DeployAllAsync(bool deployTables = true, bool deployStoredProcedures = true)
    {
        _logger.LogInformation("Starting database deployment...");

        try
        {
            // Ensure database exists
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database ensured to exist");

            if (deployTables)
            {
                await DeployTablesAsync();
            }

            if (deployStoredProcedures)
            {
                await CleanupMasterDatabaseStrayProceduresAsync();
                await DeployStoredProceduresAsync();
            }

            _logger.LogInformation("Database deployment completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database deployment");
            throw;
        }
    }

    private async Task DeployTablesAsync()
    {
        _logger.LogInformation("Deploying table schemas...");

        // Base module schemas first (idempotent CREATE), then dated incremental patches
        // (20261020_*.sql) that align SQL Server with EF-touched tables.
        var schemaFiles = new List<string>
        {
            "CoreModule_Schema.sql",
            "HRMModule_Schema.sql",
            "FinanceModule_Schema.sql",
            "InventoryModule_Schema.sql",
            "CRMModule_Schema.sql",
            "ProcurementModule_Schema.sql"
        };
        schemaFiles.AddRange(ListIncrementalSchemaFileNames(_schemasPath));

        foreach (var file in schemaFiles)
        {
            var filePath = Path.Combine(_schemasPath, file);
            if (File.Exists(filePath))
            {
                try
                {
                    _logger.LogInformation("Executing schema file: {File}", file);
                    await ExecuteSqlFileAsync(filePath);
                    _logger.LogInformation("Successfully executed: {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error executing schema file {File}, continuing...", file);
                    // Continue with other files even if one fails
                }
            }
            else
            {
                _logger.LogWarning("Schema file not found: {File}", file);
            }
        }
    }

    /// <summary>
    /// Dated incremental patches: <c>yyyyMMdd_Feature.sql</c> (e.g. <c>20261020_Auth_TwoFactorTotp.sql</c>).
    /// Sorted by file name so dependent patches apply in chronological order.
    /// </summary>
    public static IReadOnlyList<string> ListIncrementalSchemaFileNames(string schemasPath)
    {
        if (string.IsNullOrWhiteSpace(schemasPath) || !Directory.Exists(schemasPath))
            return Array.Empty<string>();

        return Directory.GetFiles(schemasPath, "20*_*.sql")
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Cast<string>()
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task DeployStoredProceduresAsync()
    {
        _logger.LogInformation("Deploying stored procedures...");

        var spFiles = new[]
        {
            "CoreModule_StoredProcedures.sql",
            "HRMModule_StoredProcedures.sql",
            "FinanceModule_StoredProcedures.sql",
            "InventoryModule_StoredProcedures.sql",
            "CRMModule_StoredProcedures.sql",
            "ProcurementModule_StoredProcedures.sql",
            "ManufacturingModule_StoredProcedures.sql",
            "ProjectsModule_StoredProcedures.sql",
            "SalesPOSModule_StoredProcedures.sql",
            "ReportsModule_StoredProcedures.sql",
            "AdminModule_StoredProcedures.sql"
        };

        foreach (var file in spFiles)
        {
            var filePath = Path.Combine(_storedProceduresPath, file);
            if (File.Exists(filePath))
            {
                try
                {
                    _logger.LogInformation("Executing stored procedure file: {File}", file);
                    await ExecuteSqlFileAsync(filePath);
                    _logger.LogInformation("Successfully executed: {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error executing stored procedure file {File}, continuing...", file);
                    // Continue with other files even if one fails
                }
            }
            else
            {
                _logger.LogWarning("Stored procedure file not found: {File}", file);
            }
        }
    }

    /// <summary>
    /// Drops dbo procedures in master that start with sp_Projects_ or sp_Reports_.
    /// Unqualified sp_* names resolve to master first and break CREATE OR ALTER in the app database.
    /// </summary>
    private async Task CleanupMasterDatabaseStrayProceduresAsync()
    {
        const string cleanupSql = """
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += N'DROP PROCEDURE IF EXISTS dbo.' + QUOTENAME(p.name) + N';'
            FROM master.sys.procedures p
            WHERE p.schema_id = SCHEMA_ID('dbo')
              AND (p.name LIKE 'sp_Projects[_]%' OR p.name LIKE 'sp_Reports[_]%');
            IF LEN(@sql) > 0
                EXEC sp_executesql @sql;
            """;

        try
        {
            var masterConnectionString = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(cleanupSql, connection) { CommandTimeout = 60 };
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not clean stray sp_Projects_/sp_Reports_ procedures from master database");
        }
    }

    private async Task ExecuteSqlFileAsync(string filePath)
    {
        var sqlContent = await File.ReadAllTextAsync(filePath);
        
        // Split by GO statements (SQL Server batch separator)
        var batches = Regex.Split(
            sqlContent,
            @"^\s*GO\s*?$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var batch in batches)
        {
            var trimmedBatch = batch.Trim();
            if (string.IsNullOrWhiteSpace(trimmedBatch))
                continue;

            try
            {
                using var command = new SqlCommand(trimmedBatch, connection)
                {
                    CommandTimeout = 300 // 5 minutes timeout for large scripts
                };
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                // Ignore benign "already exists" errors for idempotent redeploys
                if (ex.Number is 1913 or 2714 or 2759)
                {
                    _logger.LogDebug("Skipping batch ({ErrorNumber}): {Message}", ex.Number, ex.Message);
                    continue;
                }
                throw;
            }
        }
    }
}

