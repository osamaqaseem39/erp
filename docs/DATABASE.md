# Database — ERP SaaS

## Engine and access

| Item | Value |
|------|-------|
| Engine | Microsoft SQL Server |
| ORM | EF Core (`ERPDbContext`) |
| Secondary | Dapper via `IStoredProcedureExecutor` / `StoredProcedureExecutor` |
| Connection | `AppConstants.SQL_CONNECTION_STRING` (hardcoded; currently machine-specific `HELLOWORLD\\SQLEXPRESS`, database `erpnew`) |
| Migrations | Under `ERP.Infrastructure/Migrations/`; applied on API startup |
| Extra SQL | `Database/Schemas/*Module_Schema.sql` + dated `20yyMMdd_*.sql` patches (deployed after modules) + root maintenance scripts |

## EF model coverage

`ERPDbContext` exposes `DbSet`s across modules, including:

| Area | Examples |
|------|----------|
| Core | `Tenants`, `Users`, `Roles`, `UserRoles`, `Permissions`, `RolePermissions`, `Companies`, `Plans`, `Subscriptions`, `AuditLogs`, `Settings`, `UserSessions` |
| Admin extras | `SystemSettings`, `SystemLogs` |
| CRM | Customers, Contacts, Leads, Opportunities, Quotes/Lines, SalesOrders/Lines, Activities, Campaigns, CustomerGroups, PriceLists, SalesTerritories, ReturnOrders |
| Sales/POS | SalesInvoices/Lines, POSSessions, POSTransactions/Lines |
| HRM | Employees, addresses/documents/contracts, payroll lines/components, leave types/requests/balances, time entries, shifts, performance, recruitment entities, training, HR settings |
| Finance | Currencies, rates, CoA, journals/entries, invoices/bills/lines, payments, banks, taxes, fixed assets, budgets, cost centers, etc. |
| Inventory/SCM | Items, categories/groups/attributes/variants/pricing, UoM, warehouses, stock balances/ledgers, transfers, adjustments, barcodes, batch/serial |
| Procurement | Vendors, POs, GRNs, RFQs, requisitions, purchase invoices (+ details) |
| Manufacturing | WorkCenters, ProductionOrders/Lines, ProductionRuns |
| Projects | Projects, tasks, resources, documents, milestones |
| Reports | Reports, executions, schedules |

Core tables are mapped to schema **`Core`** (e.g. `Core.Tenants`, `Core.Users`). Some extended CRM tables map to default/`dbo` table names (aligned with migrations — see CRM status doc).

## SQL schema scripts

Present under `ERP.Infrastructure/Database/Schemas/`:

- Base: `CoreModule_Schema.sql`, `CRMModule_Schema.sql`, `FinanceModule_Schema.sql`, `HRMModule_Schema.sql`, `InventoryModule_Schema.sql`, `ProcurementModule_Schema.sql`
- Incremental (idempotent): `20yyMMdd_Feature.sql` — e.g. `20261020_Auth_TwoFactorTotp.sql`, BOM, Benefits, debit/credit note columns

Deployed optionally at startup by `DatabaseDeploymentService` (`deployTables: true`, `deployStoredProcedures: true`): module schemas first, then dated patches in name order. Failures are logged as warnings and do not abort host startup.

## Root SQL utilities

Repository root contains operational scripts such as `Create-Database.sql`, duplicate-table find/remove scripts, and PowerShell deploy helpers (`Deploy-StoredProcedures.ps1`). These are maintenance tools, not the primary EF migration path.

## Important caveats

- Having a `DbSet` and schema SQL does **not** mean the module’s application services are DI-registered.
- CRM status doc notes some SQL-only tables without C#/EF models — treat SQL scripts as broader than the runtime EF model where they diverge.
- Redis is separate from SQL but required for host construction (`localhost:6379` by default).
