# ERP Growth Plan — Work Packages

**Goal:** Pilot-ready SaaS (honest APIs + `frontend-new` + cycle tests), then controlled v1.1 growth.  
**How to run:** [AUTONOMOUS_GROWTH_PROMPT.md](./AUTONOMOUS_GROWTH_PROMPT.md) · ledger [PROGRESS.md](./PROGRESS.md).  
**Law:** [rules.md](./rules.md) wins over this schedule.

Add follow-up WPs with the next free id in the correct phase. Do not leave gaps as prose only.

---

## Phase order

```text
Phase 0  Stabilize WIP / honesty
Phase 1  Backend gaps & tenant fail-closed
Phase 2  frontend-new parity
Phase 3  Reports & prints
Phase 4  Cycle integrity (O2C ‖ P2P ‖ stock ‖ GL ‖ MFG ‖ HR)
Phase 5  Staging E2E + go-live pack
Phase 6  Growth / v1.1
```

---

## Phase 0 — Stabilize WIP / honesty

### WP 0.1 — Inventory and finish WIP gap slices
- **Depends:** —
- **Do:** Inventory uncommitted/WIP schemas and services (Finance gaps, Manufacturing gaps, WMS, Projects depth, Reports). Commit or finish incomplete public routes; no half-wired Swagger ops that return empty success.
- **Accept:** Every public route touched in the WIP either persists/reads real data or is removed/`501` with a clear message; schema SQL numbered and aligned with EF where tables were touched.
- **Verify:** `dotnet build ERP.SaaS.sln -c Release`; targeted tests for finished slices.

### WP 0.2 — Documentation honesty pass
- **Depends:** 0.1
- **Do:** Align `FEATURES.md`, `BUSINESS_MODULES.md`, `CUSTOMER_DEPTH_PLAN.md` tracker, and related status docs with live DI (`ModuleServiceCollectionExtensions`) and controllers.
- **Accept:** No claim of “wired/complete” for stub or missing DI; depth tracker matches reality.
- **Verify:** Doc review against code; note evidence in PROGRESS.

### WP 0.3 — Baseline green
- **Depends:** —
- **Do:** Record baseline of `dotnet build`, `dotnet test ERP.Tests`, and `frontend-new` typecheck/lint/build. Fix only regressions you introduce later against this baseline.
- **Accept:** Baseline failures listed in PROGRESS Notes; commands runnable.
- **Verify:** Same three command groups; paste summary into Evidence.

### WP 0.4 — Legacy frontend WIP disposition
- **Depends:** 0.1
- **Do:** Confirm uncommitted `frontend/` MFG/Reports/CRM polish stays out of product path (primary UI = `frontend-new`). Discard or park; do not expand maintenance UI.
- **Accept:** No new product dependency on legacy `frontend/` for MFG/reports hubs; note in PROGRESS.
- **Verify:** Git status / coverage doc note.

**Exit gate 0:** WIP either finished or tracked as Phase 1 WPs; docs not lying; baseline recorded. PASS/FAIL in PROGRESS.

---

## Phase 1 — Backend gaps & tenant fail-closed

### WP 1.1 — Kill stubs on public REST
- **Depends:** 0.3
- **Do:** Implement or remove every remaining public stub/empty-success service on REST.
- **Accept:** No DI stub that returns empty success for a Swagger-visible mutating/read path in scope.
- **Verify:** Grep/review + tests; build.

### WP 1.2 — Tenant fail-closed audit
- **Depends:** 1.1
- **Do:** Fail-closed tenant checks on mutating paths in depth modules (Finance/MFG/WMS/Projects/HRM/Reports as touched).
- **Accept:** Cross-tenant id → 404/403; regression probes updated.
- **Verify:** Integration/unit tests per module touched.

### WP 1.3 — Transition maps + FIX-5 / FIX-6
- **Depends:** 1.1
- **Do:** `DocumentWorkflow` (or equivalent) on new document types: dispatch, landed cost, scrap, timesheet, advances, etc. Ignore free `Status` on update; approver ≠ creator on approve.
- **Accept:** Illegal transitions fail; creator cannot final-approve; posted immutable.
- **Verify:** Workflow tests including negatives.

### WP 1.4 — Single stock writer
- **Depends:** 1.1
- **Do:** WMS/MFG/GRN/DA/kit/cycle-count/reservation stock moves only via Inventory / `IStockService` (documented successors OK).
- **Accept:** No direct stock-table writes from other modules; MFG/WMS paths covered by tests.
- **Verify:** Stock movement tests; code review probe.

### WP 1.5 — Settings toggles
- **Depends:** 1.3
- **Do:** Wire CUSTOMER_DEPTH Admin keys (`procurement.threeWayMatch`, `sales.allowUnpost`, `finance.enforceBudget`, `finance.requireOpenPeriod`, `inventory.requireBin`, …) with tests.
- **Accept:** Default SMB-safe; when on, gates enforce; when off, documented behaviour.
- **Verify:** Setting on/off tests.

### WP 1.6 — Finance gap post → real GL
- **Depends:** 1.1
- **Do:** `FinanceGapService` fund transfer / credit memo / debit memo post must create journals + set `PostedJournalId` (same path as other finance posts). Add writers for `TaxTransactions` or remove from honesty claims. Kill or delete unused `JournalService` DI if still dead.
- **Accept:** Posted gap docs have non-empty `PostedJournalId` and balanced journals; no status-only post.
- **Verify:** Finance gap post/reverse tests including tenant fail-closed.
- **Done (2026-10-21):** Fund/credit/debit memo posting creates journals. `TaxTransactions` writers → WP 1.7.

### WP 1.7 — TaxTransactions write path
- **Depends:** 1.6
- **Do:** Persist `Finance.TaxTransactions` on tax-affecting posts (SI/PI/CN/DN/memos) or drop the table from honesty claims / reports until wired.
- **Accept:** TAX_SUMMARY report has writers or catalog note; no orphan schema-only claims.
- **Verify:** Tax post creates rows; tenant fail-closed.

**Exit gate 1:** Public REST honest; tenant fail-closed on touched paths; transitions + settings for depth docs. PASS/FAIL in PROGRESS.

---

## Phase 2 — frontend-new parity

### WP 2.1 — Resource coverage
- **Depends:** 1.1
- **Do:** Every complete backend resource has list/detail/create/edit + legal lifecycle actions via registry/engine. Update `FRONTEND_NEW_COVERAGE.md`.
- **Accept:** No dead buttons that fake success; missing API → hide/disable.
- **Verify:** `npm run typecheck && npm run lint && npm run build` in `frontend-new`.

### WP 2.2 — Permission-filtered shell
- **Depends:** 2.1
- **Do:** Menus/routes/actions respect permissions; unauthorized UI hidden and API still 403.
- **Accept:** Test role with narrow permission sees only allowed modules.
- **Verify:** Manual matrix note + any existing permission tests.

### WP 2.3 — Real dashboard KPIs
- **Depends:** 2.1
- **Do:** Dashboard from `/api/core/dashboard/summary` (and module summaries where used); no demo numbers.
- **Accept:** Empty tenant shows zeros/empty, not fake metrics.
- **Verify:** UI + API smoke.

**Exit gate 2:** Coverage doc matches app; permission shell; real KPIs. PASS/FAIL in PROGRESS.

---

## Phase 3 — Reports & prints

### WP 3.1 — Catalog honesty
- **Depends:** 1.1
- **Do:** Single catalog; no client SQL; permissions per report code; generate smoke per category ([REPORTS_COMPLETION_PLAN.md](../REPORTS_COMPLETION_PLAN.md)). Implement (or hide from `/available`) codes that currently throw `NotSupportedException` in `ModuleReportQueryService` (DISPATCH_ADVICE, STOCK_VALUATION, TAX_SUMMARY, …).
- **Accept:** Unauthorized code hidden/blocked; `Query` not client-writable SQL; no catalog code that empty-succeeds or throws for “available” rows.
- **Verify:** Report catalog/generate tests.

### WP 3.2 — Exports & schedule worker
- **Depends:** 3.1
- **Do:** CSV/Excel/PDF export paths; schedule worker emails with **mocked** SMTP in tests.
- **Accept:** Formats honest; schedule history written; SMTP mock asserted.
- **Verify:** Export + schedule tests.

### WP 3.3 — Document print pack
- **Depends:** 3.1
- **Do:** Shared PDF templates for SO/SI/CN/PO/DA/vouchers as listed in depth/reports plans.
- **Accept:** Print endpoints return PDF (or documented artifact) for each listed doc type.
- **Verify:** Print smoke tests or command evidence.

**Exit gate 3:** Catalog + export/schedule + prints meet reports DoD for pilot. PASS/FAIL in PROGRESS.

---

## Phase 4 — Cycle integrity

### WP 4.1 — O2C depth automated
- **Depends:** 1.4, 2.1
- **Do:** Tests: enquiry/pre-cost (if in product) → quote → SO → reserve → dispatch → SI → AR → payment (+ credit note path).
- **Accept:** Happy path green; illegal steps fail.
- **Verify:** Named integration tests in Evidence.

### WP 4.2 — P2P depth automated
- **Depends:** 1.4, 2.1
- **Do:** Tests: PR→PO→GRN→landed cost→PI (3-way when setting on)→AP→payment (+ return/DN).
- **Accept:** Happy path green; match failure blocks when enabled.
- **Verify:** Named integration tests.

### WP 4.3 — MFG / Projects / HR smoke
- **Depends:** 1.4
- **Do:** Production complete + scrap stock; timesheet + milestone bill; leave approve + advance recovery smoke.
- **Accept:** Stock and status transitions correct; tenant fail-closed.
- **Verify:** Named tests.

### WP 4.4 — GL depth
- **Depends:** 1.5
- **Do:** Opening balances, period close gate, budget warn/block, expense claims as in depth plan.
- **Accept:** Post into closed period blocked when setting on; budget behaviour matches setting.
- **Verify:** Finance tests.

**Exit gate 4:** O2C + P2P + MFG/Projects/HR + GL automated smokes green on scratch SQL. PASS/FAIL in PROGRESS.

---

## Phase 5 — Staging E2E + go-live pack

### WP 5.1 — Local scratch full smoke
- **Depends:** 4.1, 4.2, 4.3, 4.4
- **Do:** Run [STAGING_E2E_CHECKLIST.md](./STAGING_E2E_CHECKLIST.md) flows against scratch DB + local API + `frontend-new` where practical.
- **Accept:** All six checklist flows green locally (SMTP may be mocked).
- **Verify:** Checklist evidence in PROGRESS.

### WP 5.2 — Go-live evidence pack
- **Depends:** 5.1
- **Do:** Runbook stub, UAT checklist, permission/tenant matrix, schema deploy notes for pending `20*_*.sql`.
- **Accept:** Pack linked from PROGRESS; human signature lines blank.
- **Verify:** Files exist and are complete enough to hand off.

### WP 5.3 — Staging apply + human sign-off
- **Depends:** 5.2
- **Do:** Prepare exact human steps (scripts, tenant, SMTP keys). Do **not** deploy yourself.
- **Accept:** BLOCKED with exact human action until staging E2E signed; or DONE if human already signed and noted.
- **Verify:** Sign-off note or blocker text.

**Exit gate 5:** Local smoke PASS; evidence pack ready; staging either signed or BLOCKED with human steps. PASS/FAIL in PROGRESS.

---

## Phase 6 — Growth / v1.1

### WP 6.1 — SaaS billing (optional)
- **Depends:** 5.1
- **Do:** Plans/subscriptions only if still required by product; else mark N/A / DONE with note.
- **Accept:** Real persistence + permissions, or explicit deferral recorded.
- **Verify:** Tests or N/A evidence.

### WP 6.2 — GraphQL parity (finished REST only)
- **Depends:** 5.1
- **Do:** Module-by-module GraphQL honesty after REST; no new GraphQL ahead of REST.
- **Accept:** Parity checklist per module in Evidence.
- **Verify:** GraphQL tests / smoke.

### WP 6.3+ — Backlog picks
- **Depends:** 5.1
- **Do:** One WP per chosen item from `PRODUCT_COMPLETION_PLAN` v1.1 backlog (MRP depth, multi-company, portal, …). Add as 6.3, 6.4, …
- **Accept:** Per-item criteria when added.
- **Verify:** Per-item.

**Exit gate 6:** Chosen growth WPs DONE or BLOCKED; no open TODO without dependency reason.

---

## Out of scope (do not add as WPs without a new ERP-FIX / product decision)
- NestJS / Zallo / Postgres rewrite
- DB-per-tenant
- External ledger outbox (ERP-FIX-4)
- Taxi, manpower/hostel, lending, CMS, SMS
- Arbitrary client SQL reports
