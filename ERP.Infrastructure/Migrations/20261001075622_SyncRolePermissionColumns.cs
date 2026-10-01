using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncRolePermissionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing databases already have this shape (the table predates the model); only
            // databases built purely from migrations change. Hence the existence checks.
            migrationBuilder.Sql(@"
IF COL_LENGTH('Core.RolePermissions', 'AssignedAt') IS NOT NULL
    ALTER TABLE [Core].[RolePermissions] DROP COLUMN [AssignedAt];
IF COL_LENGTH('Core.RolePermissions', 'AssignedBy') IS NOT NULL
    ALTER TABLE [Core].[RolePermissions] DROP COLUMN [AssignedBy];
IF COL_LENGTH('Core.RolePermissions', 'IsAllowed') IS NULL
    ALTER TABLE [Core].[RolePermissions] ADD [IsAllowed] bit NOT NULL CONSTRAINT [DF_RolePermissions_IsAllowed] DEFAULT 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversed: the older columns were never present in deployed databases.
        }
    }
}
