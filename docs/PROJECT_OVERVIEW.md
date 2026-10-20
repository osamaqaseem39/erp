# Project Overview — ERP SaaS

## What this is

**ERP** is a full-stack, multi-tenant SaaS ERP platform: an ASP.NET Core (`net9.0`) Clean Architecture API plus a React 18 / TypeScript / Vite admin portal. Solution file: `ERP.SaaS.sln`.

| Attribute | Value |
|-----------|-------|
| Backend | `ERP.Api`, `ERP.Application`, `ERP.Domain`, `ERP.Infrastructure`, `ERP.Shared`, `ERP.Tests` |
| Frontend | **`frontend-new/`** (supported SPA) · `frontend/` (legacy Konrix template — deprecated) |
| Database | SQL Server via EF Core + Dapper stored-procedure executor |
| Auth | JWT Bearer; refresh tokens on `User` |
| Multi-tenancy | Shared DB; `TenantId` on entities; `TenantMiddleware` + `ITenantProvider` |
| GraphQL | HotChocolate at `/graphql` |
| Bootstrap | `GET/POST /api/setup/super-admin*` + frontend `/setup` |

## Supported UI

Use **`frontend-new`** (Vite + registry engine). Point `VITE_API_URL` at `https://erp-api.tekvers.com` or local API. Coverage inventory: [frontend-new/docs/FRONTEND_NEW_COVERAGE.md](../frontend-new/docs/FRONTEND_NEW_COVERAGE.md). The older `frontend/` tree remains for reference only — do not add product screens there.

## Backend modules

Backend v1 DoD (tenant fail-closed, O2C/P2P, FIX-5 workflows) is closed. Entity map: [ENTITY_TREE.md](ENTITY_TREE.md). Staging E2E: [operating/STAGING_E2E_CHECKLIST.md](operating/STAGING_E2E_CHECKLIST.md).

CRM maturity is tracked separately in [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md).

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
├── ERP.Tests/                     # Unit / in-memory workflow tests
├── frontend-new/                  # Supported React SPA (registry-driven)
└── frontend/                      # Legacy SPA (deprecated)
```

## Quick start

```bash
dotnet restore ERP.SaaS.sln
dotnet run --project ERP.Api
# separate terminal
cd frontend-new && npm install && npm run dev
```

Dev API: `https://localhost:7062` (Swagger), GraphQL `/graphql`. Frontend-new: Vite default port. Set `VITE_API_URL` to the API origin.

Connection strings and JWT secrets currently live as **constants** in `ERP.Shared/Constants/AppConstants.cs` (machine-specific SQL Server name) — see [DEPLOYMENT.md](DEPLOYMENT.md).

## Related docs

[Features](FEATURES.md) · [Architecture](ARCHITECTURE.md) · [Database](DATABASE.md) · [API](API.md) · [Frontend](FRONTEND.md) · [Backend](BACKEND.md) · [User Roles](USER_ROLES.md) · [Business Modules](BUSINESS_MODULES.md) · [Deployment](DEPLOYMENT.md) · [Portfolio](PORTFOLIO_CONTENT.md) · [Changelog](CHANGELOG.md) · [CRM Module Status](CRM_MODULE_STATUS.md)
