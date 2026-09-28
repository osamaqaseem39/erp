# Frontend — ERP SaaS

## Stack

| Item | Value |
|------|-------|
| Package name | `konrix-tailwindcss-react` (`frontend/package.json`) |
| UI | React 18, TypeScript, Vite 5 |
| State | Redux Toolkit + Redux-Saga |
| Forms | React Hook Form + Yup |
| Styling | Tailwind CSS, Sass, FrostUI (`@frostui/tailwindcss`) |
| HTTP | Axios; `jwt-decode` |
| Charts / grids | ApexCharts, Grid.js, FullCalendar, etc. |

Scripts: `npm run dev` (`vite --host`), `build` (`tsc && vite build`), `lint`, `preview`.

## API configuration

`frontend/src/config.ts`:

- `API_URL` = `import.meta.env.VITE_API_BASE_URL` or `https://localhost:7062/api`

## Routing

Defined in `frontend/src/routes/index.tsx`, rendered by `Routes.tsx` with `PrivateRoute` for authenticated areas. Setup flow: `SetupGuard` / setup pages; root redirect via `RootRedirect`.

### Module route groups (ERP)

| Prefix | Purpose |
|--------|---------|
| `/setup` | First-run super admin (when no super admin exists) |
| `/auth/*` | Login / register (template auth pages) |
| `/modules` | Module selection |
| `/core/settings/*` | Users, roles, tenants, system settings, audit/system logs |
| `/home`, `/dashboard`, `/erp/*` | Dashboard + alternate admin paths (tenants, users, roles, settings, logs) |
| `/crm/*` | CRM dashboard + entity CRUD (customers → return orders) |
| `/hrm/*` | Employees, departments, payrolls, leave requests, trainings, performance, time entries |
| `/pos/*` | POS sessions and transactions |
| `/finance/*` | CoA, currencies, journals, invoices, bills, payments, banks |
| `/inventory/*` | Categories, warehouses, UoM, items, transfers, adjustments |
| `/procurement/*` | Vendors, purchase orders |
| `/manufacturing/*` | Production orders |
| `/projects` | Project list/new/edit |

### Template / demo routes (not ERP API modules)

`/apps/calendar`, `/apps/tickets`, `/apps/file-manager`, `/apps/kanban`, `/apps/project/*`, `/pages/*` (starter, timeline, invoice, gallery, FAQs, pricing, errors), plus other Konrix demo pages remaining in the route table.

## CRM UI

Primary CRM pages live under `frontend/src/pages/crm/` and entities under `frontend/src/modules/crm/`. Detail and completion status: [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md).

## Honest UI ↔ API coupling

- Core/Admin/CRM/POS UIs target **DI-wired** backends.
- Finance/HRM/Inventory/Procurement/Manufacturing/Projects UIs exist but will call APIs that currently fail DI until those modules are registered in `Program.cs`.
