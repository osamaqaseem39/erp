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

`createBrowserRouter` in `frontend/src/routes/router.tsx`. `App` is the root route (setup and session guards around an `<Outlet/>`). Route definitions live in one file per module under `frontend/src/routes/modules/*.routes.tsx` and are assembled in `routes/index.tsx`. Authenticated pages go through `ProtectedRoute`, which handles the sign-in redirect with a safe `?next=`, layout choice, and an optional `permission` per route. Template demo pages only load with `VITE_SHOW_TEMPLATE_PAGES=true`. `routes/routes.test.tsx` checks that every URL from the pre-split table still resolves.

## ERP UI kit

- `frontend/src/components/erp/`: the shared components every module uses (PageHeader, DataTable and toolbar, Pagination, FilterPanel, StatusBadge, Empty/Error states, Confirm/Delete dialogs, KpiCard, ChartCard, WidgetBoundary, form fields, EntitySelect, LineItemsTable, detail-page pieces). See every component in every state at `/dev/components` (dev server only).
- `frontend/src/lib/erp/`: logic without UI (`parseApiError`, `useResource`/`usePagedQuery`/`useMutation`, paging and URL sync, status registry, formatters, permissions, line totals, unsaved-changes guard).
- Reference implementation: `modules/crm/entities/customer` (list, form, detail).
- `components/entity/*` and `modules/crm/components/*` are deprecated legacy layouts.

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
