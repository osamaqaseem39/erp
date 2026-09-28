# Project Overview — ERP SaaS

## What this is

**ERP** is a full-stack, multi-tenant SaaS ERP platform: an ASP.NET Core (`net9.0`) Clean Architecture API plus a React 18 / TypeScript / Vite admin portal. Solution file: `ERP.SaaS.sln`.

| Attribute | Value |
|-----------|-------|
| Backend | `ERP.Api`, `ERP.Application`, `ERP.Domain`, `ERP.Infrastructure`, `ERP.Shared`, `ERP.Tests` |
| Frontend | `frontend/` (Konrix/FrostUI-based SPA) |
| Database | SQL Server via EF Core + Dapper stored-procedure executor |
| Auth | JWT Bearer; refresh tokens on `User` |
| Multi-tenancy | Shared DB; `TenantId` on entities; `TenantMiddleware` + `ITenantProvider` |
| GraphQL | HotChocolate at `/graphql` |
| Bootstrap | `GET/POST /api/setup/super-admin*` + frontend `/setup` |

## What is actually wired in DI (`Program.cs`)

Dependency injection registers **Core**, **Admin**, **Sales (POS + sales invoices)**, and **CRM** repositories/services. Those modules can run end-to-end when the API starts.

Controllers, domain entities, application services, and EF `DbSet`s also exist for **Finance, HRM, Inventory/SCM, Procurement, Manufacturing, Projects, and Reports** — but their services/repositories are **not registered** in `Program.cs`. Hitting those REST controllers will fail at runtime with DI resolution errors until registration is completed. See [FEATURES.md](FEATURES.md) and [BACKEND.md](BACKEND.md).

CRM maturity is tracked separately in [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md) (do not treat that file as obsolete).

## Repository layout

```text
erp/
├── ERP.SaaS.sln
├── README.md
├── SETUP_GUIDE.md                 # Setup notes (partially superseded by setup API + /setup UI)
├── docs/                          # This suite (+ CRM_MODULE_STATUS.md)
├── ERP.Api/                       # HTTP host, controllers, GraphQL, middleware, validators
├── ERP.Application/               # Application services (many modules; only some registered)
├── ERP.Domain/                    # Entities by module folder
├── ERP.Infrastructure/            # EF DbContext, migrations, repositories, SQL schemas
├── ERP.Shared/                    # DTOs, AppConstants, helpers
├── ERP.Tests/                     # Limited unit tests
└── frontend/                      # React SPA
```

## Quick start

```bash
dotnet restore ERP.SaaS.sln
dotnet run --project ERP.Api
# separate terminal
cd frontend && npm install && npm run dev
```

Dev API: `https://localhost:7062` (Swagger), GraphQL `/graphql`. Frontend: `http://localhost:5173`. API base: `VITE_API_BASE_URL` or default `https://localhost:7062/api` (`frontend/src/config.ts`).

Connection strings and JWT secrets currently live as **constants** in `ERP.Shared/Constants/AppConstants.cs` (machine-specific SQL Server name) — see [DEPLOYMENT.md](DEPLOYMENT.md).

## Related docs

[Features](FEATURES.md) · [Architecture](ARCHITECTURE.md) · [Database](DATABASE.md) · [API](API.md) · [Frontend](FRONTEND.md) · [Backend](BACKEND.md) · [User Roles](USER_ROLES.md) · [Business Modules](BUSINESS_MODULES.md) · [Deployment](DEPLOYMENT.md) · [Portfolio](PORTFOLIO_CONTENT.md) · [Changelog](CHANGELOG.md) · [CRM Module Status](CRM_MODULE_STATUS.md)
