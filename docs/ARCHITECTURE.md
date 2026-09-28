# Architecture — ERP SaaS

## Style

Clean Architecture / layered .NET solution with a React SPA client.

```text
frontend (React + Redux Toolkit + Redux-Saga)
        │  HTTPS JWT + optional X-Tenant-Id
        ▼
ERP.Api  Controllers · GraphQL · Middleware · Validators · Mappers
        │
        ▼
ERP.Application  Use-case services (per module)
        │
        ▼
ERP.Infrastructure  EF Core DbContext · Repositories · Dapper SP executor · SQL deploy
        │
        ▼
SQL Server (+ Redis connection required at host startup)
```

Shared contracts and constants: `ERP.Shared`. Tests: `ERP.Tests`.

## Projects

| Project | Role |
|---------|------|
| `ERP.Api` | Host (`Program.cs`), REST controllers, HotChocolate GraphQL, FluentValidation, JWT, CORS, Swagger, middlewares |
| `ERP.Application` | Application services and interfaces (`Core`, `Admin`, `CRM`, `Sales`, `Finance`, `HRM`, `Inventory`, `SCM`, `Procurement`, `Manufacturing`, `Projects`, `Reports`) |
| `ERP.Domain` | Entities by module (`Core`, `CRM`, `Finance`, `HRM`, `SCM`, `Procurement`, `Manufacturing`, `Projects`, `Sales`, `Reports`, `Admin`) |
| `ERP.Infrastructure` | `ERPDbContext`, EF migrations, repositories, `DatabaseDeploymentService`, schema SQL under `Database/Schemas/` |
| `ERP.Shared` | DTOs, `AppConstants`, JWT helpers, abstractions (`ITenantProvider`) |
| `ERP.Tests` | Limited unit tests |
| `frontend` | Vite React admin UI |

## Composition root (`ERP.Api/Program.cs`)

Registered today:

- EF `ERPDbContext` → SQL Server (`AppConstants.SQL_CONNECTION_STRING`)
- `IStoredProcedureExecutor`
- Redis `IConnectionMultiplexer`
- Core/Admin/Sales/CRM repositories and services
- `ITenantProvider` → `HttpContextTenantProvider`
- FluentValidation validators for tenants, auth, sales, CRM
- JWT Bearer auth
- GraphQL server with Core + Sales + HRM + Inventory + Projects + Manufacturing + Reports + Admin + CRM extensions
- CORS `AllowAll`
- Swagger with Bearer scheme

Pipeline: HTTPS → CORS → `ExceptionMiddleware` → `TenantMiddleware` → Authentication → Authorization → Controllers → GraphQL.

Startup: `Database.Migrate()` with `EnsureCreated()` fallback, then `DatabaseDeploymentService.DeployAllAsync` for SQL tables/SPs.

## Multi-tenancy

1. Authenticated request: JWT claim `TenantId` (or header `X-Tenant-Id`) set into `HttpContext.Items["TenantId"]` by `TenantMiddleware`.
2. `HttpContextTenantProvider.GetTenantId()` reads that item for repositories/services that inject `ITenantProvider`.
3. CRM repositories use tenant-filtered base patterns (see `CrmRepositoryBase` and [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md)).
4. Tenant entity can store per-tenant `DatabaseName` / `ConnectionString` fields — runtime still uses the shared `AppConstants` connection for the host DbContext (database-per-tenant switching is not implemented in the composition root).

## Controllers without DI

Finance, HRM, Inventory, Procurement, Manufacturing, Projects, and Reports controllers inject application services that are **not** `AddScoped` in `Program.cs`. Architecture includes those layers as code, but the host does not compose them yet.

## Cross-cutting

| Concern | Implementation |
|---------|----------------|
| Logging | Serilog → console + `logs/erp-.txt` |
| Errors | `ExceptionMiddleware` |
| Validation | FluentValidation (registered assemblies / explicit CRM & sales validators) |
| AuthN | JWT; refresh token fields on `User` |
| AuthZ | `[Authorize]` on admin/CRM controllers (module-dependent) |
| Mapping | API mappers (e.g. `TenantMapper`, sales mappers); CRM mapping helpers |
