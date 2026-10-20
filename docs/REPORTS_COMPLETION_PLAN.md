# Reports Completion Plan

**Goal:** Turn the thin Reports metadata shell into a finished multi-module ERP reporting product — one catalog, real queries, hub UI, honest exports, and working schedule delivery.

**Status (partial):** `ReportCatalog` seeds ~90 codes; most ops/finance/module codes run; **23 catalog codes throw `NotSupportedException`** on generate (DISPATCH_ADVICE, SALE_RETURN, VENDOR_INFO, PURCHASE_REQUISITION, RFQ_SUMMARY, VENDOR_CONTRACT, STOCK_VALUATION, REORDER_ALERT, BATCH_EXPIRY, SERIAL_NUMBER_LIST, ITEM_KIT_LIST, PAYMENT_REGISTER, TAX_SUMMARY, FIXED_ASSET_REGISTER, DEPRECIATION_SCHEDULE, EXPENSE_CLAIM_SUMMARY, BANK_TRANSACTION, OPENING_BALANCE, QUOTE_LIST, ROUTING_LIST, BENEFIT_ENROLLMENT, TRAINING_SUMMARY, RECRUITMENT_PIPELINE). Hub UI exists. Export JSON/CSV/Excel/PDF when PDF service registered. Schedule SMTP when `email.*` set. `Query` stripped from DTOs. **Do not mark DoD #2 done until stub codes are implemented or removed from `/available`.**

**Out of scope:** Arbitrary client SQL, NestJS `apps/reports` Hello World, taxi/lending/CMS report packs.

---

## Definition of done

1. **Single catalog** — `/api/reports/available` lists every runnable report (sales, purchase, inventory, finance, HR, CRM, MFG, projects, POS) as whitelisted `ReportCode`s.
2. **Execute path** — `POST /api/reports/generate` runs every **non-stub** catalog code via registry (no free-form SQL; `Query` not exposed to clients); stub codes are implemented or hidden from `/available`.
3. **Hub UI** — `/reports` catalog by category → runner (filters, table, optional chart) → history → schedules.
4. **Exports** — CSV always; Excel for tabular; PDF for statement-style + document prints (SO/SI/CN/PO/DA/voucher) where listed.
5. **Schedules** — worker executes due rows, stores artifact/history, emails recipients via existing SMTP when configured.
6. **Permissions** — every catalog code maps to `reports.{codeCamel}.view` (plus coarse `reports.reports.view` / `reports.operational.*`); unauthorized hidden from available + blocked on generate.
7. **Tests** — catalog seed, generate per category smoke, tenant fail-closed, schedule email mocked, export format honesty, permission filter.
8. **Docs honesty** — `CUSTOMER_DEPTH_PLAN` §11 unchecked until PDF + SMTP done; stale comparison/module docs refreshed.

---

## Architecture decisions (lock these)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Catalog source of truth | Expand `BuiltInCatalog` (+ shared registry used by generate) | Already seeds `Report` rows per tenant; avoid dual truth |
| Finance routes | Keep `/api/finance/reports/*` for typed UX; **also** register codes that delegate to same services | One catalog for hub; finance page can keep deep links |
| Operational routes | Keep `/api/reports/operational/*` as thin typed wrappers; every ops report gets a catalog code | Backward compatible; hub uses generate |
| Custom SQL | Never | `Report.Query` → treat as `QueryKey` / ignore; strip from DTOs |
| New Domain entities | Optional Phase E only (`ReportFavorite`, `ReportSavedFilter`, export artifact fields) | Catalog/UI/export first |
| NestJS reports app | Ignore / delete later | Not product |

```text
Client (/reports hub)
    → GET  /api/reports/available          (permission-filtered catalog)
    → POST /api/reports/generate           (ReportCode + params JSON)
    → GET  /api/reports/history
    → CRUD schedules
    → GET  /api/reports/export/{id}?format=

ReportService
    → Registry[ReportCode] → query handler (OperationalReportService | Finance report services | module queries)
    → ReportExecution row (ResultData / OutputPath)
```

---

## Phase 0 — Honesty & foundation (1–2 days)

**Backend**

- [ ] Rename/stop exposing `Report.Query` on `ReportDto` (keep column for compat; map as internal `QueryKey` = `ReportCode` only).
- [ ] Align `ReportPermissionCatalog` codes with built-in `ReportCode`s (e.g. `reports.saleOrder.view` ↔ `SALE_ORDER`), keep aliases for existing seeds if needed.
- [ ] Uncheck `CUSTOMER_DEPTH_PLAN.md` tracker §11 Reports until Phase D exits.
- [ ] Fix stale claims: `BUSINESS_MODULES.md` (Reports DI = Yes), `REFERENCE_MODULE_COMPARISON.md` §5f (SO schedule + pending production = ✅ API).
- [ ] Note under `ENTITY_TREE.md` Reports: platform metadata only; catalog lives in `BuiltInCatalog` + operational + finance APIs.

**Exit:** Docs match reality; DTO no longer surfaces SQL-like Query; permission naming rule written.

---

## Phase A — Unify catalog + close Sales/Purchase/Stock gaps (3–5 days)

Register every **already-implemented** operational/finance query as a catalog code. Wire `RunBuiltInAsync` (or equivalent registry) to existing services — no new business logic where possible.

### A1. Sales codes to add (ops exist)

| Code | Maps to |
|------|---------|
| `SALE_SUMMARY_ITEM_CUSTOMER` | `/operational/sales/summary-item-customer` |
| `SALE_ITEM_RATE_DETAIL` | `/operational/sales/item-rate-detail` |
| `SALE_ORDER_SCHEDULE` | `/operational/sales/order-schedule` |
| `SALE_PENDING_FOR_PRODUCTION` | `/operational/sales/pending-for-production` |

Ensure existing 11 codes continue to work; prefer calling `OperationalReportService` from catalog handlers to avoid duplicated queries.

### A2. Purchase codes

| Code | Maps to / build |
|------|-----------------|
| `PURCHASE_INVOICE_SUMMARY` | existing ops route |
| `PURCHASE_ORDER` | PO list by period (mirror `SALE_ORDER`) |
| `GOODS_RECEIPT_SUMMARY` | GRN header totals by period |
| `PURCHASE_DEBIT_NOTE` | DN list by period |
| `PURCHASE_RETURN_SUMMARY` | returns by period |
| `LANDED_COST_SUMMARY` | landed cost vouchers by period |

### A3. Inventory codes

| Code | Maps to / build |
|------|-----------------|
| `STOCK_ON_HAND` | keep |
| `STOCK_LEDGER` | movements from `StockLedger` |
| `STOCK_MOVEMENT_SUMMARY` | transfers + adjustments by period |
| `STOCK_AVERAGE_RATE` | WAC / average (reuse inventory valuation API if present) |
| `STOCK_RESERVATION` | open reservations |
| `CYCLE_COUNT_VARIANCE` | last counts with variance |

### A4. Finance codes (delegate)

Seed catalog rows that call existing finance report services:

| Code | Existing route |
|------|----------------|
| `TRIAL_BALANCE` | trial-balance |
| `RUNNING_TRIAL_BALANCE` | running-trial-balance |
| `PROFIT_LOSS` | profit-loss |
| `BALANCE_SHEET` | balance-sheet |
| `GENERAL_LEDGER` | general-ledger |
| `VOUCHER_LIST` | vouchers |
| `CHART_OF_ACCOUNTS` | chart-of-accounts |
| `DAILY_FINANCIAL` | daily-financial |
| `CASH_BANK_VOUCHERS` | cash-bank-vouchers |
| `CASH_BANK_LEDGER` | cash-bank-ledger |
| `VOUCHER_TYPE_MONTHLY` | voucher-type-monthly |
| `PARTNER_BALANCES` | partner-balances |
| `AGED_RECEIVABLES` | aged-receivables |
| `AGED_PAYABLES` | aged-payables |
| `CASH_FLOW` | cash-flow |
| `BUDGET_VS_ACTUAL` | budget-vs-actual |
| `CHEQUE_REGISTER` | cheque-register |

**Exit:** `available` returns full Sales + Purchase + Inventory + Finance set; generate works for each; operational routes still pass tests; permission filter covers new codes.

---

## Phase B — Module catalog expansion (new queries) (1–2 weeks)

Add real EF queries + catalog codes. Priority order matches customer depth / O2C–P2P usefulness.

### B1. CRM (high)

| Code | Purpose |
|------|---------|
| `LEAD_PIPELINE` | Leads by status / owner / period |
| `OPPORTUNITY_PIPELINE` | Opp value by stage |
| `QUOTE_CONVERSION` | Quotes → SO conversion rate |
| `CAMPAIGN_ACTIVITY` | Campaigns + linked activities |
| `SALE_ENQUIRY_SUMMARY` | Packaging enquiries by period |
| `SALE_PRECOST_SUMMARY` | Pre-costs vs quote |

### B2. Manufacturing

| Code | Purpose |
|------|---------|
| `PRODUCTION_ORDER_STATUS` | Open/in-progress/completed POs |
| `BOM_USAGE` | Component usage vs BOM |
| `PRODUCTION_SCRAP` | Scrap qty/value by order/item |
| `WIP_SUMMARY` | Open production value |
| `WORK_CENTER_LOAD` | Orders/steps by work center |

### B3. Projects

| Code | Purpose |
|------|---------|
| `PROJECT_STATUS` | Active projects, % complete, budget |
| `PROJECT_TIMESHEET` | Hours by project/resource/period |
| `PROJECT_BURN` | Budget vs timesheet/expense |
| `MILESTONE_DUE` | Upcoming / overdue milestones |

### B4. HRM

| Code | Purpose |
|------|---------|
| `EMPLOYEE_ROSTER` | Active employees by dept/team |
| `LEAVE_BALANCE` | Balances by type |
| `LEAVE_UTILIZATION` | Requests by period/status |
| `ATTENDANCE_SUMMARY` | Time entries / attendance aggregate |
| `PAYROLL_SUMMARY` | Payroll runs + totals |
| `EMPLOYEE_ADVANCE` | Open advances / recoveries |

### B5. POS

| Code | Purpose |
|------|---------|
| `POS_SESSION_SUMMARY` | Sessions open/closed, totals |
| `POS_TENDER_SUMMARY` | Payments by tender type |
| `POS_ITEM_SALES` | Items sold in POS period |

### B6. Optional later (v1.1)

- Tax detail / GST-style packs (jurisdiction-specific)
- Intercompany / multi-branch consolidations
- Fixed asset register + depreciation schedule (if not already under finance UI)

**Exit:** Each B* code seeded, generate returns real data, tenant-scoped tests, at least one integration test per category.

---

## Phase C — Reports hub UI (1–1.5 weeks)

**Frontend** (`frontend` registry module `id: "reports"`)

- [ ] Routes: `/reports` (catalog), `/reports/:code` (runner), `/reports/history`, `/reports/schedules`
- [ ] Catalog page: group by Category (Sales, Purchase, Inventory, Finance, CRM, MFG, Projects, HR, POS); search; permission-hide
- [ ] Runner: parameter form from report `Parameters` schema (date range, warehouse, customer, account, etc.); run → table; optional simple chart for summary reports
- [ ] Drill-down links into existing module detail routes where IDs present
- [ ] History list from `/api/reports/history`; open prior `ResultData`
- [ ] Schedules CRUD UI bound to existing schedule APIs
- [ ] Keep `/finance/reports` as deep Accounts UI **or** redirect selected codes into hub runner (prefer keep finance page + link “Open in Reports”)

Reuse patterns from `AccountsReportsPage.tsx` where possible (param bar + result table).

**Exit:** Empty `sections: []` gone; smoke walkthrough: pick report → run → CSV download → schedule create.

---

## Phase D — Export, prints, schedule delivery (1–2 weeks)

### D1. Tabular export

- [ ] CSV for all catalog reports (already partially present — make universal)
- [ ] Excel (`.xlsx`) for tabular multi-column reports
- [ ] Persist file under tenant storage; set `ReportExecution.OutputPath`; download endpoint authenticated

### D2. Document PDF prints

| Document | Permission |
|----------|------------|
| Sales Order, Sales Invoice, Credit Note | `reports.print.view` |
| Purchase Order, Dispatch Advice | same |
| Journal / voucher print | same |

Reuse any existing PDF stack from Core (CUSTOMER_DEPTH claims Core PDF — wire here or add shared print service).

### D3. Schedule email

- [ ] `ReportScheduleWorker`: after successful execute, if `EmailRecipients` set and SMTP configured → send attachment (CSV/PDF per schedule format)
- [ ] On SMTP missing: mark execution Completed with warning in `ErrorMessage` / notes — do not pretend sent
- [ ] Tests with mocked mail sender

**Exit:** CUSTOMER_DEPTH §11 can be checked honestly; schedule email works on staging with real SMTP.

---

## Phase E — Platform polish (optional, 3–5 days)

Only if hub UX needs it:

| Item | Notes |
|------|-------|
| `ReportFavorite` | User + ReportId |
| `ReportSavedFilter` | User + ReportId + JSON params + name |
| Strip dead NestJS reports scaffold | Or leave untouched |
| Charts pack | Recharts/etc. for summary codes only |
| GraphQL parity | Mirror new codes already covered by generate |

Update `ENTITY_TREE.md` if favorites/filters are added.

---

## Suggested delivery waves

```text
Wave R0   Phase 0 honesty + Query/DTO + permission naming
Wave R1   Phase A unify catalog (sales ops leftovers + purchase + stock + finance codes)
Wave R2   Phase C hub UI against Wave R1 catalog (parallelizable after A1)
Wave R3   Phase B1–B3 CRM / MFG / Projects queries
Wave R4   Phase B4–B5 HR + POS
Wave R5   Phase D CSV/Excel/PDF + schedule SMTP
Wave R6   Phase E favorites/filters + docs/E2E smoke
```

Parallelism: **R2 UI** can start once A1 (sales+finance codes) lands; B modules can land behind feature flags / inactive catalog rows until UI ready.

---

## Testing checklist

| Layer | Coverage |
|-------|----------|
| Unit / app | Registry resolves every `BuiltInCatalog` code; unknown code → 404 |
| Integration | Generate each category on seeded tenant; empty tenant isolation |
| Permissions | User without `reports.X.view` omitted from available + 403 on generate |
| Worker | Due schedule → execution row; email invoked when recipients set |
| Export | CSV non-empty; PDF refused or succeeded per format honesty |
| FE smoke | Catalog → run → history → schedule |

---

## Doc updates (as phases land)

| Doc | Change |
|-----|--------|
| `ENTITY_TREE.md` | Reports note + link to this plan; entities if Phase E |
| `CUSTOMER_DEPTH_PLAN.md` §11 | Uncheck until D; rewrite “Complete when” to match DoD here |
| `BUSINESS_MODULES.md` | Reports DI Yes; UI hub when C done |
| `REFERENCE_MODULE_COMPARISON.md` §5f / §9 | Sync API ✅ / PDF gaps |
| `FEATURES.md` / `PRODUCT_COMPLETION_PLAN.md` | Point to this plan for Reports “must” |
| Optional `REPORTS_CATALOG.md` | Living table of Code → Category → handler → permission |

---

## Effort sketch

| Phase | Rough effort | Depends on |
|-------|--------------|------------|
| 0 | 1–2 d | — |
| A | 3–5 d | 0 |
| B | 8–12 d | A registry pattern |
| C | 5–8 d | A (partial OK) |
| D | 5–10 d | A + C; Core SMTP/PDF |
| E | 3–5 d | C |

**Total:** ~5–8 weeks calendar for one full-stack engineer; less if finance UI reuse + B modules staged as v1.1.

---

## Immediate next actions

1. Wire or hide the **23 NotSupported** catalog codes (WP 3.1); keep `/available` honest.
2. Refresh depth/features honesty docs when codes land (WP 0.2).
3. Keep staging E2E / live SMTP sign-off open (Phase 5).
4. Per-document print polish + EF favorites remain follow-ups.
