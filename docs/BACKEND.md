# Backend — ERP SaaS

## Runtime

| Item | Value |
|------|-------|
| Target | .NET 9 (`net9.0`) |
| Host project | `ERP.Api` |
| Solution | `ERP.SaaS.sln` |

## Wired application services (`Program.cs`)

| Area | Services (representative) |
|------|---------------------------|
| Core | `TenantService`, `AuthService`, `SetupService`, `PermissionService`, `AuditService` |
| Admin | `UserService`, `RoleService`, `SystemSettingService`, `AuditLogService`, `SystemLogService` |
| Sales | `SalesInvoiceService`, `POSSessionService`, `POSTransactionService` |
| CRM | Customer, Contact, Lead, Opportunity, Quote, SalesOrder, Activity, Campaign + extended group/price-list/territory/return-order services |
| API façade | `TenantApiService`, `AuthApiService` |

Repositories registered for the same areas (Core, Admin, Sales, CRM including quote/SO lines and extended CRM).

## Present in Application layer but not registered in host DI

Examples of service classes/interfaces that exist under `ERP.Application` without matching `AddScoped` in `Program.cs`:

- **Finance** — `CurrencyService`, `ExchangeRateService`, `ChartOfAccountsService`, `JournalService`, `InvoiceService`, `BillService`, `PaymentService`, `BankService`, `TaxService`, fixed-asset related services, etc.
- **HRM** — `EmployeeService`, `LeaveService`, `PayrollService`, `RecruitmentService`, `TrainingService`, `PerformanceService`, `TimeTrackingService`, department/benefits interfaces, …
- **Inventory / SCM** — stock, warehouse, item*, serial/batch/reorder/valuation services (note duplicate `StockService` namespaces under `Services` and `Inventory`)
- **Procurement** — e.g. `VendorService`
- **Manufacturing** — `ProductionOrderService`, `WorkCenterService`
- **Projects** — `ProjectService`, `ProjectDocumentService`
- **Reports** — `ReportService`

Until these are registered with their repositories and validators, corresponding controllers are non-functional at runtime.

## Infrastructure services

| Component | Role |
|-----------|------|
| `ERPDbContext` | EF Core model + migrations |
| `StoredProcedureExecutor` | Dapper SP execution |
| `DatabaseDeploymentService` | Apply schema/SP SQL files at startup |
| Redis multiplexer | Connected on startup |

## Configuration source

`ERP.Shared/Constants/AppConstants.cs` holds JWT issuer/audience/secret, refresh token expiry days, Redis connection, and SQL connection string as **compile-time constants** — not `appsettings.json` overrides for those values.

## Logging

Serilog configured in `Program.cs` (console + daily rolling file `logs/erp-.txt`).

## Tests

`ERP.Tests` includes `CrmLineCalculatorTests`, `LeadConversionTests`, and a placeholder `UnitTest1.cs`. Coverage is narrow relative to the module surface.
