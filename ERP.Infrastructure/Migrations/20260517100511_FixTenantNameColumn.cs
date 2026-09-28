using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTenantNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'Core'
                      AND t.name = N'Tenants'
                      AND c.name = N'Name'
                )
                BEGIN
                    EXEC sp_rename N'Core.Tenants.Name', N'TenantName', N'COLUMN';
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'Core'
                      AND t.name = N'Tenants'
                      AND c.name = N'TenantName'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = N'Core'
                      AND t.name = N'Tenants'
                      AND c.name = N'Name'
                )
                BEGIN
                    EXEC sp_rename N'Core.Tenants.TenantName', N'Name', N'COLUMN';
                END
                """);
        }
    }
}
