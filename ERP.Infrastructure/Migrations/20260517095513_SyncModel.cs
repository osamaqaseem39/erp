using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Companies");

            migrationBuilder.EnsureSchema(
                name: "Core");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "UserRoles",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "Tenants",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Subscriptions",
                newName: "Subscriptions",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Settings",
                newName: "Settings",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Roles",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "RolePermissions",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Plans",
                newName: "Plans",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Permissions",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "Companies",
                newSchema: "Core");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "AuditLogs",
                newSchema: "Core");

            migrationBuilder.RenameColumn(
                name: "AssignedAt",
                schema: "Core",
                table: "UserRoles",
                newName: "AssignedDate");

            migrationBuilder.RenameColumn(
                name: "TenantName",
                schema: "Core",
                table: "Tenants",
                newName: "Name");

            migrationBuilder.AddColumn<bool>(
                name: "IsTenantOwner",
                schema: "Core",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CustomerGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceListName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnOrders_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesTerritories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerritoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTerritories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrders_CustomerId",
                table: "ReturnOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnOrders_SalesOrderId",
                table: "ReturnOrders",
                column: "SalesOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerGroups");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropTable(
                name: "ReturnOrders");

            migrationBuilder.DropTable(
                name: "SalesTerritories");

            migrationBuilder.DropColumn(
                name: "IsTenantOwner",
                schema: "Core",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "Core",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "Core",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "Tenants",
                schema: "Core",
                newName: "Tenants");

            migrationBuilder.RenameTable(
                name: "Subscriptions",
                schema: "Core",
                newName: "Subscriptions");

            migrationBuilder.RenameTable(
                name: "Settings",
                schema: "Core",
                newName: "Settings");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "Core",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                schema: "Core",
                newName: "RolePermissions");

            migrationBuilder.RenameTable(
                name: "Plans",
                schema: "Core",
                newName: "Plans");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "Core",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "Companies",
                schema: "Core",
                newName: "Companies");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "Core",
                newName: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "AssignedDate",
                table: "UserRoles",
                newName: "AssignedAt");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tenants",
                newName: "TenantName");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedBy",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
