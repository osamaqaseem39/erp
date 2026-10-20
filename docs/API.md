# API — ERP SaaS

Base path for REST: `/api/...`. GraphQL: `/graphql`. Dev Swagger: `/swagger`. Root `/` redirects to Swagger in Development.

Auth: `Authorization: Bearer <jwt>`. Tenant: JWT claim `TenantId` and/or header `X-Tenant-Id`.

Controllers marked **Not DI-wired** will not resolve dependencies with the current `Program.cs` registrations.

---

## Setup (public)

| Method | Path |
|--------|------|
| `GET` | `/api/setup/super-admin/exists` |
| `POST` | `/api/setup/super-admin` |

## Auth

| Method | Path |
|--------|------|
| `POST` | `/api/auth/login` |
| `POST` | `/api/auth/refresh` |
| `POST` | `/api/auth/logout` |
| `POST` | `/api/auth/change-password` |
| `POST` | `/api/auth/reset-password` |
| `POST` | `/api/auth/verify-email` |

## Tenants

| Method | Path |
|--------|------|
| `GET` | `/api/tenants`, `/api/tenants/{id}`, `/api/tenants/domain/{domain}` |
| `POST` | `/api/tenants` |
| `PUT` | `/api/tenants/{id}` |
| `DELETE` | `/api/tenants/{id}` |

## Admin (authorized)

| Area | Base | Notable actions |
|------|------|-----------------|
| Users | `/api/admin/users` | CRUD, activate, deactivate, reset-password |
| Roles | `/api/admin/roles` | CRUD, get/set permissions |
| Permissions | `/api/admin/permissions` | list, `POST .../seed` |
| System settings | `/api/admin/system-settings` | CRUD, by key, by category, upsert |
| Audit logs | `/api/admin/audit-logs` | query filters, log-action |
| System logs | `/api/admin/system-logs` | query by level/user/date |

## Sales / POS — Wired

| Base | Actions |
|------|---------|
| `/api/sales/invoices` | CRUD, post, void |
| `/api/sales/pos-sessions` | CRUD, close |
| `/api/sales/pos-transactions` | CRUD |

## CRM — Wired

| Base | Extra actions |
|------|---------------|
| `/api/crm/customers` | search, status, industry |
| `/api/crm/contacts` | by customer, search |
| `/api/crm/leads` | `.../convert`, `.../convert-to-opportunity` |
| `/api/crm/opportunities` | filters; `.../convert` |
| `/api/crm/quotes` | filters; `.../send`, `.../approve`, `.../reject`, `.../convert` |
| `/api/crm/sales-orders` | filters; `.../approve`, `.../reject`, `.../ship`, `.../invoice` |
| `/api/crm/activities` | filters; `.../complete`, `.../reschedule` |
| `/api/crm/campaigns` | filters; `.../launch`, `.../pause`, `.../complete` |
| `/api/crm/customer-groups` | CRUD |
| `/api/crm/price-lists` | CRUD |
| `/api/crm/sales-territories` | CRUD |
| `/api/crm/return-orders` | CRUD |

Full CRM matrix: [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md).

## Finance — Not DI-wired

Prefixes include: `/api/finance/accounting`, `/api/finance/chartofaccounts`, `/api/finance/currencies`, `/api/finance/exchange-rates`, `/api/finance/banks`, `/api/finance/taxes`, `/api/finance/invoices`, `/api/finance/bills`, `/api/finance/payments`, `/api/finance/journals`, `/api/finance/fixed-assets`, `/api/finance/reports`.

## HRM — Not DI-wired

Prefixes include: `/api/hrm/employees` (action-style `addEmployee`, `getAllEmployees`, …), `/api/hrm/departments`, `/api/hrm/attendance`, `/api/hrm/leaves`, `/api/hrm/leave-requests`, `/api/hrm/payroll`, `/api/hrm/performance`, `/api/hrm/recruitment`, `/api/hrm/training`, `/api/hrm/time-tracking`, `/api/hrm/benefits`.

## Inventory — Not DI-wired

Prefixes under `/api/inventory` and nested resources: categories, items, item-groups, item-attributes, item-variants, item-pricing, warehouses, units-of-measure, stock, stock-transfers (`approve` / `complete` / `cancel`), adjustments, barcodes, batch-numbers, serial-numbers, reorder-rules, valuation-methods, plus aggregate inventory transfer/valuation endpoints.

## Procurement / Manufacturing / Projects / Reports — Not DI-wired

| Base |
|------|
| `/api/procurement/vendors` |
| `/api/procurement/purchase-orders` |
| `/api/manufacturing/production-orders` |
| `/api/projects` |
| `/api/reports` |

## GraphQL

Registered extensions in `Program.cs`: Core query/mutation, Sales, HRM, Inventory, Projects, Manufacturing, Reports, Admin, CRM. Resolvers that depend on unregistered services will fail when invoked.

## Error handling

API controllers inherit `ERP.Api.Abstractions.ControllerBase` with `HandleAsync` helpers; global `ExceptionMiddleware` wraps unhandled errors.
