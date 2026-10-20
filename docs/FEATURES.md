# Features — ERP SaaS

Status is derived from controllers, application services, and **actual DI registration via `ERP.Api/Extensions/ModuleServiceCollectionExtensions.cs`** (invoked from `Program.cs`). Code that exists but is not registered is marked **Not DI-wired**.

## Legend

| Tag | Meaning |
|-----|---------|
| **Wired** | Repository + service registered in DI; controllers can resolve |
| **Stub** | DI-wired with minimal / stub application service (expand business logic incrementally) |
| **UI** | Routes/pages exist under `frontend/src` |

---

## Platform / Core — Wired

| Feature | Surface |
|---------|---------|
| Super-admin bootstrap | `GET /api/setup/super-admin/exists`, `POST /api/setup/super-admin`; UI `/setup` |
| Auth login / refresh / logout | `POST /api/auth/*` |
| Change / reset password, verify email | `POST /api/auth/change-password`, `reset-password`, `verify-email` |
| Tenants CRUD | `GET/POST/PUT/DELETE /api/tenants` (+ by domain) |
| Users admin | `/api/admin/users` (+ activate/deactivate/reset-password) |
| Roles + role permissions | `/api/admin/roles` |
| Permission list + seed | `GET /api/admin/permissions`, `POST .../seed` |
| System settings | `/api/admin/system-settings` |
| Audit logs | `/api/admin/audit-logs` |
| System logs | `/api/admin/system-logs` |
| Tenant resolution | `TenantMiddleware` (JWT `TenantId` or `X-Tenant-Id`) → `HttpContextTenantProvider` |
| Exception middleware | `ExceptionMiddleware` |
| Swagger + JWT security scheme | Development |
| GraphQL host | `/graphql` with module type extensions |
| EF migrate / EnsureCreated fallback + SQL file deploy | On startup |

## Sales / POS — Wired

| Feature | Surface |
|---------|---------|
| Sales invoices | `/api/sales/invoices` (+ post/void) |
| POS sessions | `/api/sales/pos-sessions` (+ close) |
| POS transactions | `/api/sales/pos-transactions` |
| Frontend | `/pos/sessions`, `/pos/transactions` (+ new/edit) |

## CRM — Wired (most mature module)

See [CRM_MODULE_STATUS.md](CRM_MODULE_STATUS.md) for layer-by-layer detail.

| Area | REST base | Notable workflows |
|------|-----------|-------------------|
| Customers | `/api/crm/customers` | CRUD, search, filter by status/industry |
| Contacts | `/api/crm/contacts` | CRUD, by customer, search |
| Leads | `/api/crm/leads` | qualify/lose; convert → customer/opportunity (from Qualified) |
| Opportunities | `/api/crm/opportunities` | win/lose/cancel; convert → quote (from Won) |
| Quotes | `/api/crm/quotes` | send / approve / reject; convert → sales order (Accepted only); line items |
| Sales orders | `/api/crm/sales-orders` | approve / ship / invoice; line items |
| Activities | `/api/crm/activities` | complete / cancel / reschedule (Open-only edit) |
| Campaigns | `/api/crm/campaigns` | launch / pause / complete / cancel (Draft-only edit) |
| Customer groups, price lists, territories, return orders | `/api/crm/customer-groups`, `price-lists`, `sales-territories`, `return-orders` | CRUD |
| Frontend | `/crm/*` list/new/edit + `/crm/dashboard` | |

GraphQL: `CRMQueries` / `CRMMutations` registered (partial vs REST — see CRM status doc).

---

## Finance — Wired (honesty gaps)

DI-registered via `AddErpFinanceModule`. Controllers under `ERP.Api/Controllers/Finance/`:

| Area | Route prefix (examples) | Notes |
|------|-------------------------|-------|
| Accounting (CoA, journals, entries, TB/GL) | `/api/finance/accounting` | **Wired** via `AccountingService` |
| Chart of accounts | `/api/finance/chartofaccounts` | Wired |
| Currencies / exchange rates | `/api/finance/currencies`, `/api/finance/exchange-rates` | Wired |
| Banks | `/api/finance/banks` | Wired |
| Taxes | `/api/finance/taxes` | Wired |
| Invoices / bills / payments / journals | `/api/finance/invoices`, `bills`, `payments`, `journals` | Wired; DI’d `JournalService` post/reverse is **dead** (`false`) — use AccountingService |
| Fixed assets + depreciation | `/api/finance/fixed-assets` | Wired |
| Finance gaps (fund/credit/debit memo, …) | `/api/finance/gaps` (etc.) | CRUD + post creates journals (`PostedJournalId`); needs AR/AP defaults for memos |
| Finance reports | `/api/finance/reports` | Wired report service |

CRM sales-order **Invoice** action creates a Finance invoice header (`SalesOrderService.InvoiceAsync`).

Frontend routes exist under `/finance/*`.

## HRM — Wired (some Stub)

DI-registered via `AddErpHrmModule`. Routes include `/api/hrm/employees`, `departments`, `attendance`, `leaves`, `leave-requests`, `payroll`, `performance`, `recruitment`, `training`, `time-tracking`, `benefits`. Leave + time-tracking/attendance: tenant fail-closed. Frontend under `/hrm/*`.

## Inventory / SCM — Wired

DI-registered via `AddErpInventoryModule`. Controllers under `/api/inventory/...`. Stock level/adjust/transfer on `StockService` (tenant fail-closed via `ITenantProvider` + repository guards). Frontend under `/inventory/*` (companyId from auth session).

## Procurement — Wired

DI-registered via `AddErpProcurementModule`. Real PO/PR/GRN/PI/RFQ/return/debit note/landed cost/vendor contract services (not stub-heavy).

## Manufacturing — Wired

DI-registered via `AddErpManufacturingModule`. Production orders (SO FKs, warehouse, BOM explode, routing operations, QC gate), work centers (capacity warn), BOM, routing activate, scrap reasons + scrap→stock, release from SO, quality checks. UI: dashboard, BOM/routing/work-center lists, order detail, release-from-sales.

## Projects — Wired

DI-registered via `AddErpProjectsModule`. `/api/projects` plus document service.

## Reports — Wired (partial catalog)

DI-registered via `AddErpReportsModule`. `/api/reports`. Catalog ~90 codes; **23 codes `NotSupported` on generate** (must implement or hide). Hub UI in `frontend-new`.

---

## Frontend-only / template apps

Routes under `/apps/calendar`, `/apps/tickets`, `/apps/file-manager`, `/apps/kanban`, `/apps/project/*`, and `/pages/*` come from the Konrix template and are **not** ERP domain APIs.

## Gaps to call out honestly

1. Honesty leftovers: FinanceGap post without GL; `JournalService` post/reverse dead; report stub catalog codes; expense claim without GL.
2. Schema dual-path (EF migrations + SQL file deploy) still needs ongoing reconciliation.
3. Role/permission **authorization policies** need ongoing audit on every mutating endpoint (ERP-FIX-3).
4. Staging cross-flow E2E still open; local Wave2 O2C/P2P tests are green.
5. `SETUP_GUIDE.md` still describes older manual SQL bootstrap; prefer setup API + `/setup` UI.