# ERP UI Program — Plan and Checkpoints

**Goal:** every ERP module gets a real dashboard, server-paged lists, sectioned forms, detail pages, safe delete/archive, workflows, RBAC and audit, all on one shared design system. The work runs on the existing React SPA and .NET API.

**Prompts:** [MASTER_PROMPT.md](MASTER_PROMPT.md) (standing context) and [MODULE_PROMPT.md](MODULE_PROMPT.md) (one per checkpoint).
**Last updated:** 2026-09-28

---

## How checkpoints work

Each checkpoint has two gates:

- **Gate A (plan review):** the agent writes `docs/modules/<MODULE>_PLAN.md` and stops. You approve or correct it. No code is written before this.
- **Gate B (done review):** the agent reports against the exit criteria and the Definition of Done in the master prompt. You walk the flows in the running app. The next checkpoint doesn't start until Gate B passes.

Update the tracker table at the bottom when a gate passes.

## Why this order differs from the original prompt

The original order was Core, Admin, HRM, Finance, Inventory, CRM, Manufacturing, Procurement, Projects, Reports. This plan changes it for two reasons:

1. **CRM goes third, as the pilot.** It is the only module whose backend is fully wired and whose UI is about 90% done. Retrofitting it proves the design system against real APIs before it meets stub-heavy modules.
2. **Inventory comes before Finance, HRM and Procurement.** Items, warehouses and stock are what CRM lines, Procurement receipts and Manufacturing consume. Finance comes next because Procurement and Sales post into it. HRM has no downstream dependents, so it can move later without blocking anything.

Sales/POS, which is in the codebase but was missing from the original list, has been added after CRM.

---

## Checkpoint 0 — Baseline and foundation

**Why first:** there are hundreds of uncommitted changes on `master`, and every later module depends on the shared kit and on RBAC plumbing.

Scope
- Commit or stash the current working tree. Branch `feature/erp-ui-foundation`. Confirm `npm run build`, `npm run lint`, `npm test`, `dotnet build`, `dotnet test` all run, and record the current failures as the baseline.
- Split `routes/index.tsx` into `routes/modules/<module>.routes.tsx`. Move template demo routes (`/apps/*`, `/pages/*`) behind a dev-only flag.
- Create `components/erp/`: PageHeader, DataTable (+Toolbar, SearchInput, FilterPanel, Pagination, ColumnSelector), StatusBadge, EmptyState/ErrorState/Skeletons, WidgetBoundary, KpiCard, ChartCard, ConfirmDialog, DeleteDialog, Toast, FormCard/FormSection/FormField, EntitySelect, CurrencyInput, DateInput/DateRangePicker, LineItemsTable, DetailHeader, DetailTabs, FieldGrid, ActivityTimeline, PermissionGuard.
- Create `lib/erp/`: statusRegistry, parseApiError, usePagedQuery/useResource/useMutation, format helpers, useUnsavedChangesGuard.
- Absorb `components/entity/*` and `modules/crm/components/*` into the kit, keeping re-exports so current pages still compile.
- Fix the `apiCore.ts` 403 interceptor so it no longer hard-redirects the whole app.
- Backend: standardize the error response shape in ExceptionMiddleware and FluentValidation (field errors). Add a `PagedResult` helper for controllers.
- `/dev/components` gallery showing every component in every state.

Exit criteria
- Gallery renders every component in loading, empty, error, disabled and filled states. Keyboard-only use works for dialogs, menus and tables.
- Vitest tests for parseApiError (every status code plus legacy shapes), statusRegistry, format helpers and LineItemsTable math.
- One existing list (for example CRM Customers) migrated onto DataTable as a proof, with no regression.
- All five build and test commands pass, with no failures beyond the recorded baseline.

---

## Checkpoint 1 — Core / Settings

Scope: sectioned Settings page; organization (companies, branches, cost centers; departments surfaced from HRM); reference data wired to existing Finance and Inventory endpoints; numbering sequences; audit and system log explorers with diff viewer; System health dashboard. Delete the `pages/erp/Admin/*` and `pages/erp/Tenants/*` duplicates and redirect `/erp/*`.

Exit criteria
- One canonical screen per entity, and old URLs redirect.
- Encrypted settings are never returned or shown in plain text, and read-only settings can't be edited.
- The audit log filters by date range, user, entity and action with server paging.

---

## Checkpoint 2 — Admin and platform RBAC ⚠ security milestone

Scope: user list, form and detail (roles, sessions, login history, audit, lock/unlock, reset password, activate/deactivate); roles with a permission matrix; the full permission catalog for **all** modules (`<module>.<resource>.<action>`); the backend `[RequirePermission]` policy provider and handler; effective permissions in the login/refresh response or `GET /api/auth/me`; frontend `usePermission`, filtered menu and route guards; Admin dashboard.

Exit criteria
- ERP.Tests prove that a user without `core.users.create` gets 403 from `POST /api/admin/users`, and that SuperAdmin still passes.
- A test role with only `crm.customers.view` sees only CRM › Customers in the menu, gets no create/edit/delete buttons, and gets 403 from the API if the calls are made by hand.
- Every permission change writes an audit entry.
- **After this checkpoint, each later module adds `[RequirePermission]` to its own controllers as part of its Definition of Done.**

---

## Checkpoint 3 — CRM (pilot retrofit)

Scope: migrate the 12 entities to the kit and server paging; add detail pages (Customer 360, Opportunity, Quote, Sales Order); opportunity pipeline Kanban; `/crm` dashboard backed by `/api/crm/dashboard/summary`; crm.* permissions; audit on create, update, convert and workflow actions. Quote and sales order lines stay as a recorded gap for the Inventory item picker.

Exit criteria
- The full chain works in the app: Lead → convert → Opportunity → drag through stages → Quote → send → convert → Sales Order → approve → ship → invoice, with each step visible in the activity timeline.
- Dashboard KPIs match hand-counted database values for a test tenant.
- CRM_MODULE_STATUS.md has been moved to `docs/modules/CRM_STATUS.md` and updated. Frontend and permissions rows are ✅.
- **Design-system freeze review:** list any kit changes CRM needed. Breaking kit changes after this point need a note in the checkpoint report.

---

## Checkpoint 4 — Sales / POS

Scope: POS sessions (open/close, cash variance), transactions with receipt-style detail, sales invoice post/void, POS dashboard, and the documented link between sales invoices and Finance invoices.

Exit criteria
- A session can't close with open carts, and the variance is shown and audited.
- Posted invoices can only be voided (not deleted), and the void is audited.

---

## Checkpoint 5 — Inventory

Scope: Item detail with tabs (pricing, stock by warehouse, ledger, variants, barcodes, batch/serial, reorder); categories, groups, UoM and warehouses on the kit; transfer and adjustment workflows; Inventory dashboard; the Item EntitySelect. Then close the CRM gap by wiring items into quote and sales order lines.

Exit criteria
- The stock ledger and balance agree after a transfer and an adjustment, verified by an ERP.Tests case.
- Low-stock and out-of-stock counts match the reorder rules.
- The CRM quote line item picker works, and the CRM status doc records the gap as closed.

---

## Checkpoint 6 — Finance

Scope: **replace the stubs first** (journal posting with balanced debit and credit, Bank, Bill, FixedAsset), then CoA tree, journal entry form, AR invoice and AP bill detail with payment application, bank accounts (masked), currencies and FX, taxes, and a Finance dashboard with aging.

Exit criteria
- An unbalanced journal can't be posted (backend test), and posted journals are immutable (reversal only).
- The Invoice → partial payment → paid flow updates the balance and status. Overdue is derived from the due date.
- Receivables aging buckets on the dashboard add up to total receivables.

---

## Checkpoint 7 — HRM

Scope: a real Department entity (hierarchy with cycle prevention); replace the LeaveRequest, Attendance and Benefits stubs; employee form sections and detail tabs; leave approval with balance updates; payroll run and approval; the HR, Manager and Employee dashboards; masking of sensitive fields.

Exit criteria
- Approving leave reduces the balance and rejecting it doesn't (backend tests). The approval panel shows who decided, when, and why.
- A user without `hrm.payroll.view` sees no salary anywhere: not in lists, detail pages, exports or API responses.
- An employee with payroll history can only be terminated or deactivated, not deleted.

---

## Checkpoint 8 — Procurement

Scope: the state machine in services (requisition → approval → RFQ → PO approve → GRN with partial receipt → purchase invoice), a My Approvals queue, RFQ comparison, 3-way match indicators, and a Procurement dashboard.

Exit criteria
- Illegal transitions are rejected by the backend (tests), for example receiving against an unapproved PO.
- A partial GRN leaves the PO in Partially Received, and posting the GRN updates Inventory stock.

---

## Checkpoint 9 — Manufacturing

Scope: new BOM and Routing entities (backend and UI); the production order lifecycle (Draft → Planned → Released → In Progress → Quality Check → Completed / Cancelled); material requirements from the BOM with shortages from Inventory; production runs; work centers; Manufacturing dashboard.

Exit criteria
- Releasing an order computes its material requirements from the BOM, and shortages match Inventory balances.
- Completing an order records produced and scrapped quantities and triggers the finished-goods stock movement.

---

## Checkpoint 10 — Projects

Scope: project detail tabs, task list and Kanban with parent/child tasks, milestones, team, timesheets from HRM TimeEntry, expenses, documents, server-side progress, and a Projects dashboard.

Exit criteria
- Project progress and budget-vs-actual are computed server-side and match their inputs.
- A project's customer and manager are chosen with EntitySelect and shown by name, never by GUID.

---

## Checkpoint 11 — Reports

Scope: report catalog, runner (filters, grouping, chart and table, drill-down), saved filters, schedules, execution history, and server-side export jobs (CSV, Excel, PDF). Seed the catalog with reports built from each module's dashboard queries.

Exit criteria
- Reports can only run whitelisted server-side keys. A test proves the client can't submit SQL.
- Exports respect both permissions and tenant scope (test), and drill-down links open the correct detail pages.

---

## Checkpoint 12 — Hardening

Scope: a cross-module walk-through, performance checks (list pages under 1 s at 10k rows with server paging; dashboard widgets load in parallel), an accessibility pass using the `design:accessibility-review` checklist, a security review of permissions on every controller, cleanup of the leftover template demo code, and docs refresh (FEATURES.md, FRONTEND.md, USER_ROLES.md).

Exit criteria
- Every controller action carries `[RequirePermission]` or an explicit `[AllowAnonymous]` or authenticated-only justification, checked by an automated test that reflects over the controllers.
- No module status doc has an open P0 gap.

---

## Tracker

| # | Checkpoint | Gate A (plan) | Gate B (done) | Status doc | Notes |
|---|------------|---------------|---------------|------------|-------|
| 0 | Baseline & foundation | ☐ | ☐ | — | |
| 1 | Core / Settings | ☐ | ☐ | ☐ | |
| 2 | Admin + RBAC | ☐ | ☐ | ☐ | security milestone |
| 3 | CRM (pilot) | ☐ | ☐ | ☐ | design-system freeze review |
| 4 | Sales / POS | ☐ | ☐ | ☐ | |
| 5 | Inventory | ☐ | ☐ | ☐ | closes CRM line-item gap |
| 6 | Finance | ☐ | ☐ | ☐ | stubs first |
| 7 | HRM | ☐ | ☐ | ☐ | stubs first |
| 8 | Procurement | ☐ | ☐ | ☐ | stubs first |
| 9 | Manufacturing | ☐ | ☐ | ☐ | new BOM/routing |
| 10 | Projects | ☐ | ☐ | ☐ | |
| 11 | Reports | ☐ | ☐ | ☐ | |
| 12 | Hardening | ☐ | ☐ | — | |

## Risks to watch

| Risk | Mitigation |
|------|------------|
| Stub backends tempt the agent into fake dashboard numbers | Master prompt rule 3. Gate A requires every widget to be marked REAL or GAP. |
| The design system churns while modules are built on it | Freeze review at checkpoint 3, with breaking changes called out in reports. |
| RBAC rollout locks users out | Keep the SuperAdmin bypass, seed a default Admin role with all codes, and gate behind tests at checkpoint 2. |
| Two API styles (REST and RPC) leak into the UI | Services own the URLs, and the UI only calls typed service methods. |
| Uncommitted work on master gets tangled with this program | Checkpoint 0 starts by committing a clean baseline and branching. |
| Scope creep from the Zallo/NestJS migration docs | Stated out of scope in the master prompt. |
