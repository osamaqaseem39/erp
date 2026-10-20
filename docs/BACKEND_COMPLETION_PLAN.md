# Backend Completion Plan

> **Product-level DoD (backend + `frontend-new` + cycles):** see [PRODUCT_COMPLETION_PLAN.md](./PRODUCT_COMPLETION_PLAN.md).  
> This file is the **API implementation detail** for Waves 0–7 of that plan.

Scope: **.NET API only** (`ERP.Api` / `ERP.Application` / `ERP.Infrastructure` / `ERP.Shared` / `ERP.Domain`).  
Live surface today: [Swagger All modules](https://erp-api.tekvers.com/swagger/index.html?urls.primaryName=All%20modules) — **765** operations (local export). Completeness is about **real persistence + workflows**, not route count.

Each wave must satisfy [operating/rules.md](./operating/rules.md). Schedule lives here; hard constraints live in the [operating constitution](./operating/PROJECT_CONSTITUTION.md).

---

## Principles

1. **No new Swagger routes until the service behind them writes real data** (or explicitly documents read-only).
2. **Fix contract lies first** — DTOs that accept fields the service ignores (e.g. Sales `lines`) either persist them or remove them from the public schema.
3. **One vertical slice per entity**: Domain → Repository → Service (no stub) → Controller → FluentValidation → tenant filter → tests.
4. **Shared host DB + `TenantId`** remains the tenancy model; do not implement database-per-tenant until Platform entitlements exist.
5. Prefer finishing **procure-to-pay** and **order-to-cash** before BOM/manufacturing depth.
6. Obey operating rules on every touched entity: transition maps, posted immutability, permissions + tenant scope, SQL Server tests.

---

## Phase 0 — Contract & security hygiene (1–3 days)

**Status:** contract DTO fixes done; Sales/POS + MFG BOM permission resources applied; OpenAPI method-typed for Auth/Sales/Procurement/Finance/CRM/Admin/HRM/Inventory/MFG/Projects/Reports (+ Core/Setup/Tenant).

| Item | Action |
|------|--------|
| `ChangePasswordDto.UserId` | Removed from DTO; caller = JWT only |
| Tenant create/update `connectionString` / `databaseName` | Rejected from write DTOs; host assigns |
| Strip `ConnectionString` from all public tenant responses | Done in DTO/mapper |
| Response schemas | ✅ Platform + Sales + Procurement + Finance + CRM + Admin + HRM + Inventory + MFG + Projects + Reports + Core (method-level) |
| Permission enforcement | ✅ Sales / Procurement / MFG / Projects / Reports use `[PermissionResource]` where convention needs override; route convention covers the rest (EndpointPermissionTests green) |

**Exit:** Platform + Sales OpenAPI match runtime behavior; no client-writable secrets on tenants. ✅ (+ all module REST controllers method-typed)

---

## Phase 1 — Make published routes honest (1–2 weeks)

Close the gap where Swagger advertises working APIs but services are stubs or ignore payload.

### 1A — Sales (highest ROI; 3 real controllers already)

**Status:** invoice + POS transaction **lines persist** on create/update; GET-by-id returns lines; totals recalculated from lines; posted/voided invoices reject update/delete. Finance AR on post + reverse on void ✅.

| Work | Detail |
|------|--------|
| Invoice lines | ✅ Persist `CreateSalesInvoiceDto.Lines` / replace on update; return lines on GET |
| POS transaction lines | ✅ Same for `CreatePOSTransactionDto.Lines` |
| Post / void | ✅ Post creates Finance AR; void cancels unpaid AR (blocks if payments applied) |
| Line repos | ✅ Wired `ISalesInvoiceLineRepository` / `IPOSTransactionLineRepository` in DI |

### 1B — Procurement

**Status:** Vendor + PO + PR + GRN + PI + RFQ are real (CRUD, lines, approve/reject, tenant filter, REST). Forward GRN stock + PI→AP glued; PI cancel voids unpaid AP ✅.

| Work | Detail |
|------|--------|
| Replace `PurchaseOrderService` stub | ✅ Real CRUD + approve/reject against `IPurchaseOrderRepository` |
| PO lines | ✅ Persist details (`PurchaseOrderDetail`) |
| Then | ✅ `PurchaseRequisition` → `GoodsReceiptNote` → `PurchaseInvoice` (+ REST controllers) |
| RFQ | ✅ Draft→Open→Awarded/Cancelled; vendor responses; REST `/api/procurement/rfqs` |
| PI reject → AP | ✅ Cancel Approved PI voids unpaid AP bill (blocks if payments/credits applied) |

### 1C — Reports

| Work | Detail |
|------|--------|
| Report definition CRUD REST | ✅ `/api/reports` available/generate/schedule/history |
| Execute / generate | ✅ Built-in codes (`SALE_*`, `STOCK_ON_HAND`, `PURCHASE_ORDER_SUMMARY`, …) return JSON payloads |
| Financial report methods | ✅ Delegated to Finance accounting/invoice services |
| Schedules | ✅ Metadata CRUD + `ReportScheduleWorker` polls due rows → execute → history + next run |

### 1D — Projects & Manufacturing (surface already thin)

| Work | Detail |
|------|--------|
| Projects | ✅ Task + Milestone + Resource + Document services + REST under `/api/projects/{id}/tasks|milestones|resources|documents` (upload/download via `App_Data/project-docs`); flat document controller tenant-scoped |
| Manufacturing | ✅ Work centers + production orders/runs; BOM + stock on complete in Phase 4 |

**Exit:** Every route under Sales, Procurement (PO+vendor), Projects, Manufacturing, Reports either works end-to-end or is removed/`501` with a clear message.

---

## Phase 2 — Order-to-cash & procure-to-pay glue (2–3 weeks)

Cross-module integrity (still backend-only).

**Status:** ✅ Done for backend happy-path glue. GRN→stock+PO qty; SO ship→stock; PI approve→AP bill; SI post→AR invoice; payments apply to AR/AP (`PartiallyPaid`/`Paid` + balances).

| Flow | Steps to harden |
|------|-----------------|
| CRM quote → sales order → ship → invoice | ✅ Stock on ship; ✅ Sales invoice post creates Finance AR invoice |
| PO → GRN → stock in → purchase invoice → AP bill | ✅ GRN → stock + PO qty; ✅ PI approve → Finance bill |
| Payments | ✅ Apply payment to AR invoice / AP bill; update balances + status |

**Exit:** Happy-path integration tests for one O2C and one P2P scenario per tenant. ✅ (Wave2* tests)

---

## Phase 3 — Replace Finance / HRM stubs (3–4 weeks)

Work module-by-module; prefer accounting core before HRM niceties.

### Finance (priority order)

1. Journals + journal entries (post/unpost, balanced entries) — ✅ `AccountingService` + `ApplicationJournalRepositoryAdapter` (CRUD entries, post requires balance, reverse creates opposite JV, trial balance from posted)
2. Bills + payments (AP) — ✅ (Phase 2)
3. Banks / bank accounts — ✅ real `BankService` (bank + account CRUD, transactions, match/unmatch, recon open/tick/close)
4. Fixed assets — ✅ thin CRUD + dispose + period depreciation + GL journal (Dr expense / Cr accum)
5. Real report queries used by Reports Phase 1C — ✅ TB/GL/P&L/BS/R2/cash-flow/budget-vs-actual
6. Chart of accounts — ✅ real CRUD on `AccountingService` (unique code, parent check, block delete with children/journal entries)
7. **Accounts functional parity vs `reference/POS`** — see [REFERENCE_MODULE_COMPARISON.md](./REFERENCE_MODULE_COMPARISON.md) + [ACCOUNTS_MODULE_PLAN.md](./ACCOUNTS_MODULE_PLAN.md): cost centers, account defaults, typed cash/bank journals, approve/unpost, honest TB/ledger/running TB/P&L/BS, R2 cash/partner summaries, AR/AP aging, bank recon, cash-flow, fixed assets, payment void, dep GL, cheque register ✅. Optional later: cheque-book stock / deposit-return roles.

### HRM (priority order)

1. Employees + departments (real) — ✅ Department CRUD (soft-delete)
2. Leave requests (approve/reject) — ✅ Pending→Approved/Rejected/Cancelled
3. Attendance check-in/out — ✅ TimeEntry check-in/out/summary/adjust
4. Payroll run (can remain simplified) — ✅ Draft→Processed (lines + totals)→Posted; soft-delete Draft only
5. Recruitment — ✅ tenant CRUD + workflows (Open→Closed/Filled; Applied→Shortlisted→Interviewed→Hired/Rejected; interview Scheduled→Completed/Cancelled); soft-delete; hire fills opening
6. Benefits — ✅ catalog CRUD + enroll/unenroll (`HRM.Benefits` / `EmployeeBenefitEnrollments`)
7. Training — ✅ tenant CRUD + real enroll/complete via `TrainingEnrollment` (capacity, Scheduled→In Progress→Completed)
8. Performance — ✅ tenant CRUD + Draft→Submitted→Approved→Completed (soft-delete Draft only)

**Exit:** No `Task.FromResult(null/false/Empty)` stub services left in Finance accounting path or core HRM (employee/leave/attendance/payroll/benefits/training/performance/recruitment).

---

## Phase 4 — Manufacturing depth & inventory coupling (2–3 weeks)

| Item | Detail |
|------|--------|
| BOM header/lines | ✅ `BillOfMaterial` + lines; Draft→Active→Obsolete; REST `/api/manufacturing/boms` |
| Routing / work center capacity | Optional v1: routing steps only |
| Production complete | ✅ Complete with BOM issues components + receives FG via `IStockService` |
| Serial/batch | ✅ `StockService` receive creates batch/serial (+ tracking); issue FIFO-consumes; MFG complete inherits via Issue/Receive |

**Exit:** Complete a production order that moves stock correctly.

---

## Phase 5 — Platform product gaps (parallel-friendly, 2–3 weeks)

**User module (reference video):** [USER_MODULE_PLAN.md](./USER_MODULE_PLAN.md) — Role-as-User-Type, fine-grained permissions, Admin Email, Company contact, Employee link. **U0–U6 done.**

Only what the architecture expects and Swagger still lacks:

| Endpoint / feature | Notes |
|--------------------|-------|
| Forgot-password + email token flow | ✅ `POST /api/auth/forgot-password`; hashed token; notifier hook (log until SMTP) |
| Session list / revoke | ✅ `GET/DELETE /api/auth/sessions`; sign-out-everywhere kept |
| Optional 2FA (TOTP) | ✅ setup/enable/disable + login challenge `/api/auth/2fa/*` (apply `20261020_Auth_TwoFactorTotp.sql`) |
| Billing / plans / subscriptions | Only if product requires SaaS billing now; else defer |
| Dashboard summary | ✅ Core `/api/core/dashboard/summary` + Admin `/api/admin/dashboard/summary` |

**Exit:** Platform Swagger matches a minimal SaaS identity story (login, reset, sessions, me, optional 2FA). ✅

---

## Phase 6 — Hardening (ongoing alongside 1–5)

| Area | Action |
|------|--------|
| Tenant isolation | ✅ Projects + Reports + MFG + RFQ/VendorBankAccount + PurchaseReturn fail-closed; Sales/most Procurement already OK |
| Audit | ✅ Vendor + Warehouse + AuditService migrated SP→EF (interceptor + explicit Log/Get on same AuditLogs store) |
| Tests | Per-module: service unit + one API integration test for create→lifecycle |
| OpenAPI | ✅ All module REST controllers method-typed `ProducesResponseType` (Auth/Sales/Procurement/Finance/CRM/Admin/HRM/Inventory/MFG/Projects/Reports + Core) |
| InvoiceService honesty | ✅ AR invoice lines + AP bills/lines persist via EF adapter (no fake success); header totals recalc; posted/locked reject line edits |
| Fiscal year | ✅ `/api/core/fiscal-years` CRUD + close; Active FY required on journal post when configured; `Journal.FinancialYearId` set on post |
| SQL vs EF | ✅ `DatabaseDeploymentService` applies dated `20*_*.sql` patches after module schemas (aligns SQL deploy with EF-touched tables) |

---

## Suggested delivery order (if one stream)

```text
P0 hygiene
  → P1A Sales lines
  → P1B Procurement PO (+ lines)
  → P2 O2C/P2P glue (minimal)
  → ~~P1D Projects children + MFG work centers~~ ✅
  → P1C Reports honesty
  → P3 Finance journals/AP
  → P3 HRM employee/leave
  → ~~P4 BOM~~ ✅
  → ~~P5 Platform forgot-password/sessions~~ ✅
  → ~~P5 dashboard summaries~~ ✅
  → ~~P5 optional 2FA (TOTP)~~ ✅
  → ~~P6 Inventory stubs (categories + adjustments)~~ ✅
  → ~~P1B RFQ + user activate/deactivate~~ ✅
  → ~~P2 PurchaseReturn.Complete → PurchaseDebitNote~~ ✅
  → ~~P2 SI void / PI cancel reverse AR/AP~~ ✅
  → ~~P6 VendorRepository SP→EF (audit path)~~ ✅
  → ~~P6 WarehouseRepository SP→EF + Inventory warehouse DI/tenant~~ ✅
  → ~~P6 AuditService SP→EF~~ ✅
  → ~~P6 TaxCode → GL (`/api/finance/tax-codes`)~~ ✅
  → ~~P6 SalesOrder approve/reject workflow (PO parity)~~ ✅
  → ~~P6 OpenAPI Auth + Setup/Tenant + Sales~~ ✅
  → ~~P6 OpenAPI Procurement~~ ✅
  → ~~P6 OpenAPI Finance~~ ✅
  → ~~P6 OpenAPI CRM + Admin~~ ✅
  → ~~P6 OpenAPI HRM~~ ✅
  → ~~P6 SQL↔EF incremental schema deploy~~ ✅
  → ~~P6 Quote approve/reject/convert gate (SO parity)~~ ✅
  → ~~P6 OpenAPI Inventory/MFG/Projects/Reports method polish~~ ✅
  → ~~P6 InvoiceService AR lines + AP bills/lines (no fake success)~~ ✅
  → ~~P6 StockTransfer tenant + workflow + stock move~~ ✅
  → ~~P6 Opportunity win/lose/cancel + convert-from-Won (FIX-5)~~ ✅
  → ~~P1D ProjectDocument REST + tenant file store~~ ✅
  → ~~P6 Lead qualify/lose + convert-from-Qualified (FIX-5)~~ ✅
  → ~~P6 StockService + StockBalance/Transfer/Ledger tenant fail-closed~~ ✅
  → ~~P4 serial/batch on stock receive/issue (MFG complete)~~ ✅
  → ~~P5 report schedule background worker~~ ✅
  → ~~P6 GraphQL bank accounts → IBankService~~ ✅
  → ~~P6 FiscalYear master + journal post enforce + invoice IsLockedAsync honesty~~ ✅
  → ~~P6 GraphQL journal workflow (approve/reject/unpost/cheque) + SO approve/reject~~ ✅
  → ~~P6 Procurement GraphQL honesty + vendor approve/suspend real~~ ✅
  → ~~P6 InventoryService facade + valuation report~~ ✅
  → ~~P6 Campaign launch/pause/complete/cancel (FIX-5)~~ ✅
  → ~~P6 GraphQL Sales credit notes (CRUD + post/void)~~ ✅
  → ~~P6 Activity complete/cancel/reschedule (FIX-5)~~ ✅
  → ~~P6 AR SendInvoice delivery notifier (log-until-SMTP; Draft→Issued)~~ ✅
  → ~~P6 JournalsController empty-id honesty + bill mark-paid via PaymentService + report export json/csv~~ ✅
  → ~~P6 GraphQL RFQ workflow (CRUD + send/cancel/close/response/award)~~ ✅
  → ~~P6 GraphQL purchase return + debit note queries (+ update/delete)~~ ✅
  → ~~P6 GraphQL FiscalYear (CRUD + close + covering)~~ ✅
  → ~~P6 Default Voucher last-used (cash/bank + cost center settings)~~ ✅
  → ~~P6 GraphQL ReturnOrder (CRUD + approve/complete/cancel)~~ ✅
  → ~~P6 GraphQL SalesOrder ship/invoice~~ ✅
  → ~~P6 Leave request tenant fail-closed + DocumentWorkflow + GraphQL reject/cancel~~ ✅
  → ~~P6 GraphQL BOM CRUD + activate/obsolete + ProductionRun query/cancel~~ ✅
  → ~~P6 GraphQL SendInvoice + ExportReport (json/csv honesty)~~ ✅
  → ~~P6 GraphQL CRM entity CRUD (Customer/Lead/Opp/Quote/SO/Activity + Campaign delete)~~ ✅
  → ~~P6 GraphQL TaxCode + CostCenter + AccountDefaults + voucher-defaults~~ ✅
  → ~~P6 Journal header InvoiceNumber + EntityReference (voucher↔AR/AP)~~ ✅
  → ~~P6 GraphQL Warehouse (CRUD + activate/deactivate + search)~~ ✅
  → ~~P6 GraphQL Attendance (check-in/out + summary + adjust)~~ ✅
  → ~~P6 GraphQL TimeTracking tenant-scoped + total hours~~ ✅
  → ~~P6 GraphQL Admin activate/deactivate + AdminEmail CRUD + role permissions~~ ✅
  → ~~P6 TimeEntry/Attendance repo+service tenant fail-closed~~ ✅
  → ~~P6 GraphQL Core company/settings/sequences + Core/Admin dashboards~~ ✅
  → ~~P6 GraphQL Projects children (milestone/resource update/delete + documents)~~ ✅
  → ~~P6 GraphQL ChequeRegister query + Benefits Unenroll~~ ✅
  → ~~P6 Project task/milestone/resource tenant fail-closed + Complete DocumentWorkflow~~ ✅
  → ~~P6 Employee tenant fail-closed + GraphQL CRUD~~ ✅
  → ~~P6 GraphQL Currency + ExchangeRate (CRUD + convert/current)~~ ✅
  → ~~P6 GraphQL bank reconciliation (get/list + tick/untick + close)~~ ✅
  → ~~P6 GraphQL FixedAsset depreciation depth (entries/methods/calculate/NBV)~~ ✅
  → ~~P6 GraphQL ItemGroup + UnitOfMeasure → SCM tenant services~~ ✅
  → ~~P6 GraphQL Bills queries (get/search/lines; mutations already present)~~ ✅
  → ~~P6 GraphQL FixedAsset depreciation depth (entries/methods/calculate/NBV)~~ ✅
  → ~~P6 GraphQL Stock receive/issue~~ ✅
  → ~~P6 GraphQL Warehouse → SCM tenant service (CRUD + search + activate)~~ ✅
  → ~~P6 GraphQL ItemVariant + ItemPricing + Barcode~~ ✅
  → ~~P6 Department tenant fail-closed + parent filter honesty~~ ✅
  → ~~P6 GraphQL Serial/Batch/Reorder/Attribute~~ ✅
  → ~~P6 GraphQL StockBalance → tenant IStockService (explicit tenantId)~~ ✅
  → ~~P6 GraphQL InventoryValuationMethod~~ ✅
  → ~~P6 GraphQL OperationalReports + Admin GetPermissions~~ ✅
  → ~~P6 GraphQL Item → SCM tenant service + repo fail-closed~~ ✅
  → ~~P6 Recruitment tenant fail-closed (JobOpening/Application/Candidate)~~ ✅
  → ~~P6 GraphQL Stock ledger + average rate + inventory valuation~~ ✅
  → ~~P6 TaxCode tenant fail-closed (repo + service)~~ ✅
  → ~~P6 GraphQL Bank master CRUD + tenant fail-closed~~ ✅
  → ~~P6 Invoice/Bill repo adapter tenant fail-closed~~ ✅
  → ~~P6 CRM masters (Salesman/CustomerGroup/PriceList/Territory) tenant + GraphQL~~ ✅
  → ~~P6 Remove dead InventoryQuery twin (InventoryQueries is registered)~~ ✅
  → ~~P6 CostCenter repo/service tenant fail-closed (was soft-filter)~~ ✅
  → ~~P6 Payment repo/service/REST tenant fail-closed~~ ✅
  → ~~P6 Customer tenant fail-closed + Activate/Deactivate (FIX-5)~~ ✅
  → ~~P6 AccountDefaults + Payment GraphQL tenant honesty~~ ✅
  → ~~P6 ReturnOrder GraphQL/REST/service tenant fail-closed~~ ✅
  → ~~P6 FixedAsset service tenant fail-closed (was soft-filter)~~ ✅
  → ~~P6 Vendor bank accounts GraphQL mutations + REST nested routes~~ ✅
  → ~~P6 BillService + BankService (accounts/tx/recon) tenant fail-closed~~ ✅
  → ~~P6 AccountingService cheque/budget/voucher-defaults tenant fail-closed~~ ✅
  → ~~P6 ReportService built-in operational reports tenant fail-closed~~ ✅
  → ~~P6 OperationalReportService + Quote/SO scoped + CN/DN account defaults fail-closed~~ ✅
  → ~~P6 Leave AddLeaveRequest GraphQL/REST tenant args + Payroll fail-closed~~ ✅
  → P6 continuous (remaining stubs / GraphQL parity)
```

---

## Out of scope (this file only — covered in product plan or deferred)

| Item | Where |
|------|--------|
| `frontend-new` screens | [PRODUCT_COMPLETION_PLAN.md](./PRODUCT_COMPLETION_PLAN.md) Waves 0–9 |
| NestJS / Zallo rewrite | Deferred post-v1 |
| DB-per-tenant | Deferred post-v1 |
| GraphQL parity | After REST services are real |
| SaaS billing, full BI | v1.1 backlog in product plan |

---

## Definition of “backend complete” (v1)

1. Every Swagger operation under the 12 modules either **persists/reads real data** or is **removed**.
2. O2C and P2P happy paths work across CRM ↔ Sales ↔ Inventory ↔ Finance ↔ Procurement.
3. No DI stub classes remain for entities that have public REST routes.
4. Tenant isolation + permission attributes on all mutating endpoints.
5. Platform supports login, refresh, logout, change/reset password, me, sessions, tenant CRUD without client-supplied connection strings.
6. Light MFG (work center + BOM + stock move), Projects children, catalog Reports, HRM leave/attendance/simple payroll — as in the product capability map.

Together with `frontend-new` wired to those APIs, that is **product complete** per [PRODUCT_COMPLETION_PLAN.md](./PRODUCT_COMPLETION_PLAN.md).
