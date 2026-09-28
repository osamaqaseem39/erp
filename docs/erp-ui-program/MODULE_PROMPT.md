# ERP UI Program — Per-Module Prompt

Use after [MASTER_PROMPT.md](MASTER_PROMPT.md) is in context. Replace `<MODULE>` and paste the matching brief below.
Run it twice per module: once to get the plan (stop), then again with "Plan approved, implement".

---

## Template

```text
Module: <MODULE>
Checkpoint: <CHECKPOINT ID from docs/erp-ui-program/PLAN.md>

Read first:
- docs/erp-ui-program/PLAN.md (this checkpoint's scope and exit criteria)
- docs/modules/<MODULE>_STATUS.md if it exists, otherwise docs/FEATURES.md
- frontend/src/modules/<module>/ (every entity)
- ERP.Api/Controllers/<Module>/, ERP.Application/<Module>/, ERP.Shared/DTOs/<Module>/, ERP.Domain/<Module>/
- frontend/src/components/erp/ (the shared design system: use it, extend it, do not fork it)

Step 1 (this run): produce docs/modules/<MODULE>_PLAN.md with all 15 sections from the master prompt §9.
Mark every dashboard widget and every action as REAL (endpoint exists and works) or GAP (stub/missing),
with the backend file responsible. Then STOP and wait for approval.

Step 2 (after approval): implement the plan in the master-prompt order, commit in small steps,
satisfy the Definition of Done, update docs/modules/<MODULE>_STATUS.md, and report.

Module brief:
<paste the brief for this module from below>
```

---

## Module briefs

### Core / Settings
Entities: Tenant, Company, SystemSetting, reference data (Currency, Country/City, Unit of Measure, Payment Method, Tax Rate), numbering sequences, AuditLog, SystemLog.
- Settings is a sectioned page (General, Organization, Localization, Numbering, Notifications, Email, Storage), not one flat list of key/value rows. SystemSetting stays the store; group by `Category`, render by `SettingType`, respect `IsReadOnly`/`IsEncrypted` (never echo encrypted values).
- Organization: Companies, Branches/Locations, Departments (shared with HRM: one Department entity, owned by HRM, surfaced here), Cost Centers (Finance entity exists).
- Reference data pages reuse existing Finance/Inventory endpoints (currencies, taxes, UoM). Do not create duplicates.
- Audit: Audit log and System log lists with server paging, date-range, user, entity and action filters; before/after diff viewer.
- Dashboard (admin-facing "System health"): tenants, users active in last 7 days, failed logins, recent config changes, recent errors from SystemLog.
- Canonical screens live in modules/core. Delete pages/erp/Admin/* and pages/erp/Tenants/* duplicates; redirect /erp/* routes.

### Admin (users, roles, permissions)
Entities: User, Role, Permission, RolePermission, UserRole, UserSession.
- Users: list, create, edit, detail (profile, roles, sessions, login history, audit), activate/deactivate, reset password, assign roles, lockout status and unlock.
- Roles: list, detail with members and a permission matrix (rows = module › resource, columns = view/create/update/delete/export/approve/post); system roles are read-only.
- This checkpoint also delivers RBAC enforcement for the whole platform: full permission catalog for all modules, `[RequirePermission]` policy on the backend, effective permissions in the auth session, `usePermission`/`PermissionGuard` on the frontend, 403 handled in place.
- Dashboard: users (active/inactive/locked), roles, users without a role, recent permission changes, recent logins.

### CRM (reference module, retrofit)
Entities: Customer, Contact, Lead, Opportunity, Quote(+lines), SalesOrder(+lines), Activity, Campaign, CustomerGroup, PriceList, SalesTerritory, ReturnOrder.
- Already ~90% on the frontend. The job is migration onto the shared design system, adding detail pages (Customer 360, Opportunity, Quote, Sales Order), server paging, permissions (crm.*), and audit.
- Add an Opportunity pipeline Kanban (drag = stage change via API, with probability update).
- Quote/SO lines: LineItemsTable with Inventory item EntitySelect once Inventory is done (record as a GAP until then).
- Keep the Dynamics-style theme in modules/crm/theme; fold its layout components into components/erp where they are generic.
- Dashboard: move /crm/dashboard to /crm, back it with /api/crm/dashboard/summary (pipeline value, open opportunities by stage, won/lost this period, conversion rate, overdue activities, quotes expiring).

### Sales / POS
Entities: SalesInvoice(+lines), POSSession, POSTransaction(+lines).
- Session open/close with cash variance, transaction list and receipt-style detail, sales invoice post/void.
- Dashboard: today's sales, open sessions, average basket, payment method mix, voids.
- Decide and document the relationship between SalesInvoice and Finance Invoice (read-only link in UI; do not merge models here).

### Inventory
Entities: Item (+variants, attributes, pricing, barcodes), ItemCategory, ItemGroup, UnitOfMeasure, Warehouse, StockBalance, StockLedger, StockTransfer(+details), InventoryAdjustment, BatchNumber, SerialNumber, ReorderRule, InventoryValuationMethod.
- 17 backend controllers, 6 UI entities. Add UIs where they carry business value: variants/pricing/barcodes as Item detail tabs; batch/serial as tabs + lookup; reorder rules per item/warehouse.
- Item detail tabs: General, Pricing, Stock by warehouse, Movements (ledger), Variants, Barcodes, Batches/Serials, Reorder, History.
- Movement pages (transfer, adjustment) show reference, item, qty, from/to, date, user, status. Workflow via endpoints.
- Dashboard: SKUs, inventory value, low stock (below reorder point), out of stock, incoming/outgoing (transfers in transit), top movers.
- Provide the Item EntitySelect used by CRM, Procurement, Manufacturing.

### Finance
Entities: ChartOfAccount, Journal(+entries), Invoice(+lines, AR), Bill(+lines, AP), Payment(+applications), Bank, BankAccount, Currency, ExchangeRate, TaxCode/TaxRate, FixedAsset, Budget, CostCenter.
- Stubs to replace first (backend): Accounting (journal post with Σdebit = Σcredit), Bank, Bill, FixedAsset. List them in the plan with file paths.
- Invoice detail: customer, number, dates, lines, taxes, discounts, subtotal, total, payments applied, balance, status timeline, activity. Posted documents are void/reverse only.
- CoA as a tree with balances. Journal entry form with balanced-lines validation and running totals.
- Dashboard: revenue, expenses, cash balance, receivables and payables with aging buckets, overdue invoices, bills due this week, unposted journals.
- Sensitive: bank account numbers masked without finance.bank-accounts.view.

### HRM
Entities: Department (design a real entity: currently DTO-only/stub), Employee (+addresses, documents, contracts), LeaveType, LeaveBalance, LeaveRequest, Shift, TimeEntry/Attendance, Payroll(+lines, components), PerformanceReview, Training(+enrollment), JobOpening/Candidate/Application/Interview.
- Stubs to replace: Department, LeaveRequest (approve/reject with balance update), Attendance, Benefits.
- Employee form sections: Personal, Contact, Employment (department, job title, manager via EntitySelect, type, hire date, status), Compensation (permission-gated), Documents.
- Employee detail tabs: Overview, Employment, Compensation (gated), Leave, Attendance, Documents, Performance, Training, Activity.
- Terminate/deactivate instead of delete for any employee with history.
- Dashboards: HR (headcount, new hires, attrition, on leave today, pending leave approvals, payroll status), Manager (my team, approvals), Employee self-service (my leave balance, my requests) — driven by permissions.
- Payroll and national IDs never visible without hrm.payroll.view / hrm.employees.sensitive.view.

### Procurement
Entities: Vendor (+bank accounts), PurchaseRequisition(+details), RequestForQuotation(+details, vendor responses), PurchaseOrder(+details), GoodsReceiptNote(+details), PurchaseInvoice(+details).
- Mostly stubs: implement the state machine in services before building rich UI.
- Approval-first UI: "My approvals" queue, ApprovalPanel on requisition and PO, RFQ vendor-response comparison table, PO → GRN (partial receipts) → invoice, 3-way match indicators.
- Dashboard: pending requisitions, pending approvals, open RFQs, open POs, pending receipts, spend by vendor this period.

### Manufacturing
Entities: WorkCenter, ProductionOrder(+lines), ProductionRun, and new BillOfMaterials (+lines) and Routing/Operations (missing in backend: design + build).
- Production order detail: product, BOM, planned vs produced qty, materials (required/consumed/short), operations, work center, status timeline, quality checks, runs.
- Dashboard: orders by status, in progress, delayed (past planned end), material shortages (from Inventory), output this period, scrap rate.

### Projects
Entities: Project, ProjectTask (parent/child), ProjectMilestone, ProjectResource, ProjectDocument; time from HRM TimeEntry (ProjectId).
- Project detail tabs: Overview (progress, budget vs actual), Tasks (list + Kanban), Milestones, Team, Timesheets, Expenses, Documents, Activity.
- Progress computed server-side from tasks (weighted by estimated hours).
- Dashboard: active/completed projects, overdue tasks, upcoming milestones (14 days), billable hours this period, budget burn.

### Reports
Entities: Report (definition), ReportExecution, ReportSchedule.
- Not a CRUD list. Report catalog grouped by module, a report runner (date range, filters, grouping, sort, chart + table, drill-down links into module detail pages), saved filters, schedules, execution history.
- Reports run registered server-side queries only (whitelisted report keys). Never send SQL from the client; do not surface Report.Query.
- Export CSV/Excel/PDF as a server job with a download link, gated by <module>.reports.export.
- Seed a first set that reuses each module's dashboard summary queries.
