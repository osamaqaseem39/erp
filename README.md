## Documentation

Full documentation lives in [`docs/`](docs/). Start with [Project Overview](docs/PROJECT_OVERVIEW.md).

| Document | Description |
|----------|-------------|
| [Project Overview](docs/PROJECT_OVERVIEW.md) | Purpose, layout, what is DI-wired |
| [Features](docs/FEATURES.md) | Feature matrix (wired vs scaffolded) |
| [Architecture](docs/ARCHITECTURE.md) | Clean Architecture + multi-tenancy |
| [Database](docs/DATABASE.md) | EF model, schemas, migrations |
| [API](docs/API.md) | REST + GraphQL route map |
| [Frontend](docs/FRONTEND.md) | React SPA routes and stack |
| [Backend](docs/BACKEND.md) | Services and DI registration |
| [User Roles](docs/USER_ROLES.md) | SuperAdmin, RBAC, permissions |
| [Business Modules](docs/BUSINESS_MODULES.md) | Module inventory |
| [Deployment](docs/DEPLOYMENT.md) | Run, config, bootstrap |
| [Portfolio Content](docs/PORTFOLIO_CONTENT.md) | Case-study copy |
| [Changelog](docs/CHANGELOG.md) | Doc / state notes |
| [CRM Module Status](docs/CRM_MODULE_STATUS.md) | Living CRM completion tracker |
| [ERP UI Program](docs/erp-ui-program/PLAN.md) | Module-by-module dashboard/UX plan, checkpoints, agent prompts |

Also useful: [SETUP_GUIDE.md](SETUP_GUIDE.md) (older manual steps; prefer setup API + `/setup` UI).

---

# ERP SaaS Platform

A full-stack, multi-tenant ERP system with a modern React frontend and a layered ASP.NET Core backend.

This repository contains:

- A backend API built with ASP.NET Core (`net9.0`) following Domain/Application/Infrastructure layering.
- A frontend admin portal built with React + TypeScript + Vite + Tailwind.
- Core ERP modules (users, roles, tenants, inventory, finance, CRM, HRM, procurement, manufacturing, projects, POS).

**Runtime honesty:** Core, Admin, CRM, Sales/POS, Finance, HRM, Inventory/SCM, Procurement, Manufacturing, Projects, and Reports are DI-registered (see [Features](docs/FEATURES.md)). Some Finance/HRM/Procurement services are still stubs pending full business logic.

## Architecture Overview

### Backend (`.NET`)

- `ERP.Api`: HTTP API host, auth, middleware, Swagger, GraphQL endpoint.
- `ERP.Application`: application services/use cases and orchestration.
- `ERP.Domain`: core business entities and domain logic.
- `ERP.Infrastructure`: data access (EF Core SQL Server + Dapper), persistence concerns.
- `ERP.Shared`: shared contracts/utilities.
- `ERP.Tests`: automated test project.

### Frontend (`React`)

- `frontend`: SPA admin UI built with React 18, TypeScript, Vite, Redux, Redux-Saga, Tailwind, and FrostUI components.
- Route-driven module structure under `frontend/src` (routes in `frontend/src/routes`).
- Theming and UI styling under `frontend/src/assets/scss`.

## Tech Stack

- **Backend**: ASP.NET Core Web API, EF Core, SQL Server, Dapper, FluentValidation, JWT auth, Swagger, HotChocolate GraphQL
- **Frontend**: React, TypeScript, Vite, Redux Toolkit, Redux-Saga, Tailwind CSS, Sass
- **Tooling**: npm, .NET SDK, ESLint, TypeScript compiler

## Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/)
- [Node.js 18+](https://nodejs.org/) and npm
- SQL Server instance (local or remote)
- Redis (default `localhost:6379` — required by current host startup)

## Getting Started

### 1) Backend setup

From repository root:

```bash
dotnet restore ERP.SaaS.sln
dotnet build ERP.SaaS.sln
dotnet run --project ERP.Api
```

Update `ERP.Shared/Constants/AppConstants.cs` SQL/Redis/JWT values for your machine before first run.

Default dev URLs (from launch settings):

- `https://localhost:7062`
- `http://localhost:5252`

Useful endpoints:

- Swagger: `https://localhost:7062/swagger`
- GraphQL: `https://localhost:7062/graphql`

### 2) Frontend setup

In a new terminal:

```bash
cd frontend
npm install
npm run dev
```

Vite default URL: `http://localhost:5173`

Frontend API base URL:

- `frontend/src/config.ts`
- Fallback default: `https://localhost:7062/api`
- Override via env: `VITE_API_BASE_URL`

### 3) Initial data/bootstrap (automated)

On first run, the frontend detects whether a super admin exists and redirects you to setup automatically.

1. Start the backend API (database schema deploys on startup).
2. Start the frontend (`npm run dev`).
3. Open `http://localhost:5173` — you will be sent to `/setup` if the system is not initialized.
4. Complete the super admin setup form (creates the first tenant + super admin user).
5. You are redirected to login with tenant ID and email pre-filled; enter your password and sign in.

API alternative: `GET /api/setup/super-admin/exists` and `POST /api/setup/super-admin` (Swagger).

### Core module API (after login)

| Area | Endpoints |
|------|-----------|
| Permissions | `GET /api/admin/permissions`, `POST /api/admin/permissions/seed` |
| Users | `GET/POST/PUT/DELETE /api/admin/users` |
| Roles | `GET/POST/PUT/DELETE /api/admin/roles`, `GET/POST .../permissions` |
| Tenants | `GET /api/tenants` (public read), mutations require auth |
| Settings | `/api/admin/system-settings` |
| Audit / logs | `/api/admin/audit-logs`, `/api/admin/system-logs` |
| CRM | `/api/crm/*` (see docs) |
| POS / sales invoices | `/api/sales/*` |

## Project Structure

```text
ERP.Api/
ERP.Application/
ERP.Domain/
ERP.Infrastructure/
ERP.Shared/
ERP.Tests/
frontend/
docs/
```

## Available Scripts

### Frontend (`frontend/package.json`)

- `npm run dev` — start Vite dev server
- `npm run build` — type-check + production build
- `npm run lint` — run ESLint
- `npm run preview` — preview production build

### Backend

- `dotnet run --project ERP.Api` — start API
- `dotnet test ERP.SaaS.sln` — run tests

## Notes

- This is an actively evolving ERP codebase; modules and UI flows may change frequently.
- Keep backend and frontend running together for full feature validation.
- Ensure CORS/API URL configuration is aligned when running across different hosts/ports.
- Treat Finance/HRM/Inventory/etc. UI as preview until DI registration is completed.
