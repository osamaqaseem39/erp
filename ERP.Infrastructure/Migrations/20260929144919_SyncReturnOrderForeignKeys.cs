using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncReturnOrderForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Snapshot-only migration: brings the model snapshot in line with ERPDbContext, where the
            // ReturnOrder -> Customer and ReturnOrder -> SalesOrder relationships are Restrict.
            // No SQL is run on purpose. In the existing database these foreign keys were created by the
            // SQL deploy scripts on CRM.ReturnOrders as ON DELETE NO ACTION, which already matches Restrict.
            // Before this, the drift made EF Core's startup Migrate() refuse to run any migration.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo (see Up).
        }
    }
}
