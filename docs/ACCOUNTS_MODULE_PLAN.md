# Accounts Module Plan (reference checklist)

> **Source:** Functional capabilities from `reference/POS` (not UI/menus).  
> **Module map:** [REFERENCE_MODULE_COMPARISON.md](./REFERENCE_MODULE_COMPARISON.md).  
> **Fits under:** Product Wave 4 / Backend Phase 3 Finance + Phase 1C Reports.  
> Obey [operating/rules.md](./operating/rules.md).

---

## Goal (functionality only)

1. **Account setup** — hierarchical CoA rules, cost centers, system default posting accounts, opening balances.  
2. **Vouchers / GL entry** — cash/bank payment & receipt effects, JV, approve → post → unpost (or reverse).  
3. **Financial queries** — ledger, TB, running TB, P&L, BS, aging, voucher listings from **posted** data (no placeholder payloads).

UI layout is out of scope for this plan; any screen is only a consumer of these APIs.

**Design decisions (locked for v1):**

- CoA: keep flexible `ParentAccountId` tree; **compute** depth for filters/validation. Do not require rigid 4-level tables.
- Vouchers: one `Journal` + entries; `JournalType` discriminates cash/bank/JV rules. No `AC_Voucher_*` port. AR/AP settlement stays on `Payment` but must produce correct GL when posted.

---

## What’s already done

| Area | Status |
|------|--------|
| `ChartOfAccount` domain (code, name, type, parent, `IsPostingAllowed`) | ✅ |
| CoA CRUD via `AccountingService` + controllers | ✅ |
| Journals (CRUD, balanced post, reverse → opposite JV, TB from posted) | ✅ |
| Trial balance + GL balance on `AccountingService` | ✅ real (posted journals) |
| Finance `/api/finance/reports/*` via `ReportService` | ✅ TB / running TB / P&L / BS / GL / vouchers / CoA + R2 daily/cash-bank/monthly/partner/aging + cash-flow + budget-vs-actual |
| P&L / Balance Sheet on `AccountingService` | ✅ classified P&L + BS with net-income equity plug |
| Journal statuses today | `Draft` → `Approved` (Closed UX) → `Posted` → `Reversed` / `Cancelled` — approve/reject/unpost ✅ |
| `JournalType` values today | ✅ `CashPayment` / `CashReceipt` / `BankPayment` / `BankReceipt` + JV screens; legacy Sales/Purchase/Bank/Adjustment/OpeningBalance kept |
| Unpost | ✅ `Posted` → `Draft` (unlock); reverse remains for corrections |
| AR invoices / AP bills / payments apply | ✅ |
| `Payment.PaymentType` / method | `Bank`, `Card`, `Stripe`, `Cash` — no dedicated voucher screens |
| Banks + accounts (CRUD; recon open/tick/close) | ✅ |
| Frontend CoA / journal / invoice / bill / payment / bank / currency screens | ✅ + L1–L4, Account Information, Setup, CPV, Approve/Closed queues |
| `CostCenter` domain + repository + DTO | ✅ service + REST + UI |
| `AccountDefaults` | ✅ purpose → posting CoA |
| CoA `Level` / filters / seed L1 | ✅ computed depth; `?level=` `?postingOnly=` |
| Finance setup settings (`finance.*`) | ✅ SettingsCatalog + `/finance/accounts/setup` |
| Journal lines accept `CostCenterId` | ✅ field exists |

---

## What’s missing (gap list)

### A. Reference checklist gaps (Accounts Setup)

| # | Reference item | Gap |
|---|---|---|
| 1 | **Account Setup** | No setup wizard / seed of L1 groups (Asset, Liability, Equity, Income, Expense). No “first-time CoA” flow. |
| 2 | **Setup Configuration** | No tenant settings for CoA: max depth, code mask/pattern, whether only leaf can post, require cost center on expense posts. |
| 3 | **Second Level** | No level-filtered list/create (“parent must be level 1”). Level is not stored or returned. |
| 4 | **Third Level** | Same for level 3. |
| 5 | **Fourth Level** | Same for level 4; no rule that posting accounts must be leaves / level ≥ N. |
| 6 | **Accounts Information** | List exists but not tree/indented browse, search by level, rollup view, or inactive filter UX matching reference. |
| 7 | **Cost Center** | No service, REST controller, validators, permissions, or UI. DTO has parent/manager fields domain does not persist. |
| 8 | **Default Fourth Level Account** | No mapping table: system purpose → posting CoA account (per tenant). |
| 9 | **Default Account** | No defaults used by O2C/P2P/journal auto-post (AR control, AP control, cash, sales, COGS, tax, inventory, retained earnings). |

### B. Reference checklist gaps (Vouchers)

| # | Reference item | Gap |
|---|---|---|
| 1 | **Cash Payment** | No voucher type / screen that forces Dr expense (or AP) / Cr **Cash** default account. Generic journal + generic payment only. |
| 2 | **Cash Receipt** | Same for Dr Cash / Cr income (or AR). |
| 3 | **Bank Payment** | No typed bank-payment voucher; bank account + Cr bank CoA not guided. |
| 4 | **Bank Receipt** | Same for bank receipts. |
| 5 | **Journal Voucher** | Generic journal exists; not branded/filtered as “JV”; no dedicated menu entry UX. |
| 6 | **Approved Voucher** | No `Approved` status or approve endpoint. Status can be set free-form on update (violates ERP-FIX-5). |
| 7 | **Post Voucher** | Post exists (`POST …/journals/{id}/post`) but only from `Draft`, not from `Approved`. No bulk/queue “post approved” UI. |
| 8 | **UnPost Voucher** | **Missing.** Only `reverse` (creates opposite posted JV + marks original `Reversed`). No return-to-draft/unpost while period open. |

### C. Reference checklist gaps (Accounts Reports)

Routes exist under `/api/finance/reports` but `ReportService` returns stub messages. Prefer **delegate to `AccountingService` / dedicated query services** and seed report catalog codes.

| # | Reference report | Maps to / intent | Gap |
|---|------------------|------------------|-----|
| 1 | **Chart of Account** | Indented/summary CoA tree | No print/export report; list UI only |
| 2 | **Chart of Account With Detail Accounts** | CoA + posting leaves expanded | Missing; needs level/tree (A3) |
| 3 | **Accounts Ledger** | GL movements for an account | `AccountingService.GetGeneralLedgerBalanceAsync` exists; `ReportService` stub; no UI |
| 4 | **Fourth Level Accounts Ledger** | Ledger filtered to level-4 / posting accounts | Missing (needs computed level) |
| 5 | **Accounts Trial Balance** | Period TB | Real in AccountingService; Finance reports API stub; no UI |
| 6 | **Accounts Running Trial Balance** | TB with opening + period + closing per account | Missing |
| 7 | **Voucher Report** | List/filter vouchers by date/type/status | Journal list API partial; no dedicated report/export |
| 8 | **Other Receivables Balance** | Non-trade / other receivable accounts balances | Missing (needs AccountPurpose or account-type tag) |
| 9 | **Income Statement Report** | P&L by revenue/expense | Route exists; returns TB-shaped / stub — not real P&L |
| 10 | **Financial Statement Report** | Balance sheet (assets/liab/equity) | Same — not classified BS |
| 11 | **Daily Financial** | Day cash/bank + voucher summary | Missing |
| 12 | **Customer / Supplier Summary** | AR/AP partner balances | Aged AR/AP routes stub; CRM/procurement data exists |
| 13 | **Cash/Bank Payment/Receipt Summary** | Totals by voucher type in period | Missing (needs typed vouchers V1) |
| 14 | **Cash/Bank Ledger Summary** | Cash & bank account rollup | Missing |
| 15 | **Voucher Type Monthly Summary** | Counts/amounts by `JournalType` × month | Missing |
| 16 | **Calculate Item Stock Average Rate** | Inventory WAC utility | ✅ `/api/inventory/stock/average-rate` (+ ledger); Finance links only |

**v1 must (R1):** #1–7, #9–10 (honest CoA/ledger/TB/running TB/voucher list + real P&L/BS).  
**v1 should (R2):** #11–15 after vouchers typed.  
**Defer / other module:** #8 (after defaults tagging), #16 (Inventory).

### D. Adjacent Finance gaps (still needed for Wave 4)

| Item | Gap |
|------|-----|
| Bank reconciliation | ✅ open / tick / close when difference = 0 |
| Wire `ReportsController` → real accounting queries | ✅ financial + R2 + cash-flow + budget-vs-actual |
| Fixed assets | ✅ CRUD / dispose / undo / period dep (no GL journal post yet) |
| Duplicate CoA surface | `ChartOfAccountsController` + `AccountingController` overlap — consolidate |
| Cost center on journal UI | Types have `costCenterId`; no picker (no API to load centers) |
| Payment void | ✅ unapply + status Voided |
| `frontend-new` Finance | Not wired; Wave 4 UI target |

### E. Data model proposals (gate before code)

Propose in [DATA_MODEL.md](./operating/DATA_MODEL.md) before implementing:

| Table / change | Purpose |
|----------------|---------|
| `Finance.AccountDefaults` (or `TenantAccountMapping`) | `TenantId`, `Purpose` (enum/string), `AccountId` (FK CoA, posting-only), unique `(TenantId, Purpose)` |
| Optional: extend `CostCenters` | Align domain with DTO: `ParentCostCenterId`, `Manager`, `Department`, `Notes` — or shrink DTO to match domain |
| Optional: `Core.Settings` keys | `Finance.CoA.MaxDepth`, `Finance.CoA.PostingLeafOnly`, `Finance.RequireCostCenterOnExpense` |
| Journal transition map (no new table) | `Draft` → `Approved` → `Posted`; `Posted` → `Unposted`/`Draft` **or** `Reversed` (pick one unpost policy in V0) |
| `Journal.JournalType` vocabulary | Add: `CashPayment`, `CashReceipt`, `BankPayment`, `BankReceipt`, `Journal` (keep legacy types for system-generated) |
| `Journal` header extensions (or `JournalVoucherDetail` 1:1) | Fields for typed vouchers (see CPV form below): cash/bank account, reference date, cheque no, invoice no, paid/received by, default-voucher flag |
| Numbering | Prefix per type: `CPV`, `CRV`, `BPV`, `BRV`, `JV` + period sequence (e.g. `CPV-250700021`) via existing numbering service |

No new table required for levels 1–4 if level = depth from root (computed).  
No separate `Voucher` table — typed journals + screens.

---

## Form spec: Cash Payment Voucher (CPV) — reference

Reference screen (Quest CPV, e.g. `CPV-250700021`). Two sections: **header** then **Voucher Detail** grid. Canonical for Cash Payment; mirror with side/account swaps for CRV / BPV / BRV.

Label note: reference shows “Recieved By” — our product uses **Received By**.

### Header

| UI field | Required | Maps to (proposed) | Gap today |
|----------|----------|-------------------|-----------|
| **Voucher No** (e.g. `CPV-250700021`) | system | `Journal.JournalNumber` + type prefix | No CPV prefix / voucher numbering scheme |
| **Cash Account** | yes | Header cash CoA → credit line on post (`AccountDefaults.Cash` or picker) | No header cash-account field |
| **Voucher Date** | yes | `Journal.JournalDate` | ✅ exists |
| **Reference No** | no | `Journal.ReferenceNo` | ✅ exists |
| **Reference Date** | no | `Journal.ReferenceDate` | ✅ |
| **Credit** (amount) | yes | Header total; must equal sum of detail debits; drives Cr cash line | Generic multi-line only; no single-amount CPV mode |
| **Cheque Number** | yes | `Journal.ChequeNumber` + register status | ✅ + Outstanding/Cleared/Bounced/Cancelled |
| **Invoice No** | no | Link text and/or `EntityReference` to AR/AP invoice | ✅ `InvoiceNumber` + `EntityReferenceTable`/`EntityReferenceId` on `Journal` |
| **Paid By** | no | `Journal.PaidBy` | ✅ |
| **Received By** | no | `Journal.ReceivedBy` | ✅ |
| **Cost Center** | no | Default for new detail rows; `+` opens quick-add modal | ✅ API + UI + journal picker |
| **Default Voucher** | no | Remember last cash account (and maybe cost center) for next CPV | ✅ last-used settings + `GET /api/finance/journals/voucher-defaults` |
| **Remarks** | no | `Journal.Description` | ✅ exists |

### Voucher Detail (line grid — below Remarks)

Reference UI shows a **Voucher Detail** section under the header. Columns not fully visible in screenshot; v1 grid = our `JournalEntry` rows (offset / debit side for CPV):

| Column | Required | Maps to |
|--------|----------|---------|
| Account | yes | `JournalEntry.AccountId` (posting CoA; not the header cash account) |
| Debit | yes* | `JournalEntry.Debit` (*sum of debits = header Credit) |
| Credit | no | Usually 0 on CPV detail rows (cash credit is system line) |
| Narration | no | `JournalEntry.Narration` |
| Cost Center | no | `JournalEntry.CostCenterId` (defaults from header) |

**Save rules (Cash Payment):**

1. User enters header Credit + one or more detail debit rows.  
2. On save, system ensures Σ detail Debit = header Credit.  
3. System adds (or updates) a balancing **Cr Cash Account** line = header Credit (hidden or shown as read-only system line).  
4. Reject post if unbalanced or cash account missing.

```text
Dr  Detail account(s)     Σ = Credit   [+ CostCenter]
Cr  Cash Account (header)     Credit   ← system line
```

**Cash Receipt:** invert (Dr Cash system line / Cr detail).  
**Bank Payment / Receipt:** same with **Bank Account** instead of Cash; cheque no emphasized.

**Locked decision:** header cash/bank + editable detail grid **(b)** — not single-account-only header.

### F. CPV field gaps (summary)

| Missing on `Journal` / product | Priority |
|--------------------------------|----------|
| Type-prefixed voucher numbers (`CPV-…`) | P0 for V1 |
| Header cash/bank `AccountId` (or always derive from defaults + first line) | P0 |
| `ReferenceDate`, `ChequeNumber`, `PaidBy`, `ReceivedBy` | P1 |
| Invoice link on voucher | P1 |
| Single-amount guided entry + auto cash credit line | P0 UX |
| Cost center picker + quick-add | Blocked on A1 |
| Default Voucher behavior (define in V0) | P2 |

---

## Delivery plan

### Phase A0 — Spec & hygiene (0.5–1 day)

1. Record design decision: **computed levels**, max depth default **4**, posting allowed only on leaves when setting `PostingLeafOnly=true`.
2. Add gated rows to `DATA_MODEL.md` for `AccountDefaults` (+ CostCenter column align if needed).
3. Collapse duplicate CoA controllers to one public REST surface (keep aliases only if clients need them).
4. Define `AccountPurpose` codes (v1 minimum):

```text
AccountsReceivable, AccountsPayable, Cash, BankClearing,
SalesRevenue, SalesReturns, CostOfGoodsSold, Inventory,
InputTax, OutputTax, RetainedEarnings, ExchangeGain, ExchangeLoss
```

**Exit:** DATA_MODEL updated; purpose list agreed; no code yet that invents tables.

---

### Phase A1 — Cost Center vertical slice (2–3 days)

**Backend**

1. Align `CostCenter` domain ↔ DTO (implement parent fields or remove from DTO).
2. `ICostCenterService` + real CRUD (unique code per tenant, block delete if journal entries reference it).
3. `CostCentersController` REST + FluentValidation + `[PermissionResource]` + tenant filter.
4. SQL Server tests: create → list → update → soft-deactivate; reject delete-in-use.

**Frontend**

1. List + form under `/finance/cost-centers`.
2. Wire journal (and bill line if present) cost-center dropdown.

**Exit:** Cost centers usable end-to-end; journals can pick a center.

---

### Phase A2 — Default accounts (2–3 days)

**Backend**

1. Schema + domain + repository for `AccountDefaults`.
2. Service: get-all / upsert-by-purpose; validate target account is active + `IsPostingAllowed`.
3. REST: `GET/PUT /api/finance/account-defaults` (bulk upsert OK).
4. Hook consumers (minimal v1): document which flows **should** read defaults; wire at least one path (e.g. SI post → AR control account, or PI → AP) if currently hard-coded/missing.
5. Tests: upsert, reject non-posting account, tenant isolation.

**Frontend**

1. **Default Account** screen: purpose → account picker (posting accounts only).
2. Optional: **Default Fourth Level Account** = same screen filtered/grouped (one UI, two menu labels if desired).

**Exit:** Tenant can configure system accounts; at least one posting path uses them.

---

### Phase A3 — Leveled CoA UX (3–5 days)

**Backend**

1. Return `Level` (1-based depth) and `HasChildren` on CoA DTO.
2. Query filters: `?level=2`, `?parentAccountId=`, `?postingOnly=true`.
3. Optional validation when `MaxDepth` setting set: reject parent that would exceed depth.
4. Optional: `POST /api/finance/chart-of-accounts/seed-defaults` — creates inactive or template L1 groups if CoA empty.
5. Accounts Information: tree endpoint or client-assembled tree from flat list with `parentAccountId` + `level`.

**Frontend**

1. Menu under Finance / Accounts:
   - Account Setup (seed + L1 list)
   - Setup Configuration (settings form)
   - Second / Third / Fourth Level (filtered lists + create with constrained parent)
   - Accounts Information (tree/search)
2. Enforce parent picker = previous level only.

**Exit:** Reference menu parity for items 1–6 without rigid DB levels.

---

### Phase A4 — Setup Configuration (1–2 days)

1. Persist settings via `Core.Settings` (or finance config endpoint).
2. UI: max depth, posting-leaf-only, require cost center on expense.
3. Enforce in CoA create/update and journal line validation.

**Exit:** Configuration changes behavior, not just labels.

---

### Phase V0 — Voucher workflow spec (0.5 day, with A0)

1. Lock transition map (ERP-FIX-5):

```text
Draft ──approve──► Approved ──post──► Posted
  ▲                  │                  │
  │                  reject             ├──unpost──► Draft   (only if period open & no dependent apps)
  │                  ▼                  └──reverse─► Reversed (+ reversing JV)
  └────────────── Draft
```

2. Prefer **Unpost** = reopen to `Draft` when safe; else force **Reverse**. Document in constitution/memory if new `ERP-FIX-n` needed.
3. Strip free `Status` on journal update DTO; only transition endpoints change status.
4. Approver ≠ creator (ERP-FIX-6).

**Exit:** Transition map + unpost policy written in DATA_MODEL / this plan.

---

### Phase V1 — Typed voucher entry (3–4 days)

**Depends on:** A2 defaults for Cash / BankClearing (at least); A1 for cost-center control.

**Backend**

1. Gate header columns in `DATA_MODEL.md`, then add: `ReferenceDate`, `ChequeNumber`, `PaidBy`, `ReceivedBy`, `CashOrBankAccountId` (FK CoA), optional `IsDefaultVoucher` / last-used setting.
2. Numbering: allocate `CPV`/`CRV`/`BPV`/`BRV`/`JV` sequences into `JournalNumber`.
3. Extend allowed `JournalType` values; validate on create.
4. Typed create/update matching **CPV form spec**:
   - Cash Payment: require cash account + amount; on save build Cr cash + Dr line(s); cost center on Dr lines.
   - Cash Receipt / Bank Payment / Bank Receipt: same helpers with side/account rules.
   - Journal: unconstrained balanced entries (existing).
5. Optional: `InvoiceNo` / entity reference to AR invoice or AP bill.
6. Optional link: posted cash/bank voucher → `Payment` + bank transaction (GL first).
7. Tests: CPV field persistence, auto balancing lines, wrong side rejected, numbering prefix.

**Frontend**

1. **Cash Payment** form layout per reference: voucher no, cash account, dates, reference, credit amount, cheque, invoice no, paid/received by, cost center (+), default voucher, remarks; then offset line grid.
2. Clone layout for CRV / BPV / BRV (labels + Dr/Cr swap + bank account where needed).
3. Menu **Vouchers** + shared list (type + status filters).

**Exit:** CPV (and siblings) match reference field set; post still Draft→Posted until V2.

---

### Phase V2 — Approve / Post / Unpost (2–3 days)

**Backend**

1. `POST …/journals/{id}/approve` — Draft→Approved; permission + not self if policy on.
2. `POST …/journals/{id}/reject` — Approved→Draft (optional reason).
3. Change `post` to accept **Approved** (and optionally Draft if setting `Finance.Voucher.AllowPostWithoutApprove=true` for SMB).
4. `POST …/journals/{id}/unpost` — Posted→Draft when safe; else 409 with message to reverse.
5. List filters: `status=Approved` for “Approved Voucher” queue; actions Post / Unpost from UI.
6. SQL tests for full lifecycle + illegal transitions.

**Frontend**

1. **Approved Voucher** — queue of Approved only + Post action.
2. **Post Voucher** / **UnPost Voucher** — actions on detail + optional bulk post of approved.
3. Hide illegal buttons per status (UI = transition map).

**Exit:** Reference items 6–8 work; reverse remains for closed-period corrections.

---

### Phase R1 — Core GL reports (3–5 days)

**Depends on:** posted journals (done); A3 levels help #2/#4 but TB/ledger can start earlier.

**Backend**

1. Point `/api/finance/reports/*` financial methods at `AccountingService` (or shared query) — delete placeholder messages.
2. Enrich TB: include `AccountCode`, `AccountName`, `AccountType`, `Level`.
3. **Running Trial Balance:** opening (before fromDate) + period debit/credit + closing.
4. **Accounts Ledger:** movement lines with running balance, voucher no, narration.
5. **Fourth Level Ledger:** same as ledger with `level=4` or `postingOnly=true` account picker.
6. **CoA reports:** tree JSON + optional CSV/PDF export (CSV first).
7. **Voucher Report:** filter journals by date/type/status; include totals; export CSV.
8. **Income Statement:** group Revenue/Expense from posted entries in period; net income.
9. **Financial Statement (BS):** Asset/Liability/Equity as-of; plug net income into equity.
10. SQL tests: TB balances, running TB identity (`opening + period = closing`), P&L nets to equity plug.

**Frontend**

1. **Accounts Reports** menu matching reference (show v1 must; hide/disable R2 until ready).
2. Parameter form (dates, account, type) → results table → export.

**Exit:** Reference #1–7, #9–10 return real rows; no `"report generated"` stubs.

---

### Phase R2 — Cash & partner summaries (2–3 days) — ✅ done

**Depends on:** V1 typed vouchers; A2 cash/bank defaults.

1. ✅ Daily Financial; Cash/Bank Payment/Receipt Summary; Cash/Bank Ledger Summary; Voucher Type Monthly Summary (`AccountingService` + `/api/finance/reports/*`).
2. ✅ Customer / Supplier Summary + aged AR/AP (`InvoiceService` + reports routes).
3. ⬜ Other Receivables Balance — deferred (needs account purpose tagging).
4. ✅ Accounts Reports UI lists R2 reports.

**Exit:** Reference #11–15 (+ #8 if tagged).

---

### Phase A5 — Wave 4 adjacency (parallel / after slices)

| Priority | Work | Notes |
|----------|------|--------|
| P1 | Phase R1–R2 | ✅ done |
| P2 | Bank reconciliation + cash-flow + budget | ✅ recon + cash-flow + budget-vs-actual |
| P3 | Fixed assets | ✅ thin CRUD + dispose + period depreciation |
| P4 | Payment / invoice / bill void | ✅ payment unapply; AR/AP void when unpaid |
| P5 | Stock average rate | ✅ Inventory WAC + ledger + `/api/inventory/stock/average-rate` |
| P5b | Cheque register | ✅ voucher cheques + clear/bounce/cancel; report UI |
| P6 | `frontend-new` Finance bind | After API freeze |

---

## Suggested order (one stream)

```text
A0 + V0 Spec + DATA_MODEL
  → A1 Cost Center (API + UI)
  → A2 Account Defaults (API + UI + Cash/Bank purposes)
  → V1 Typed vouchers (Cash/Bank/JV entry)
  → V2 Approve / Post / Unpost
  → A3 Leveled CoA + Accounts Information UI
  → A4 Setup Configuration
  → R1 Core GL reports (TB, ledger, P&L, BS, voucher report)
  → R2 Cash / partner / monthly summaries
  → A5 bank recon / frontend-new
```

Rough calendar: **~4–5 weeks** for A0–A4 + V0–V2 + R1; R2 ~extra half week after vouchers.

---

## Definition of done

### Accounts Setup v1

1. Reference checklist items 1–9 have a screen or explicit “N/A merged into X”.
2. Cost centers: full CRUD, tenant-scoped, used on journal lines.
3. Every `AccountPurpose` can be mapped to a posting CoA account per tenant.
4. CoA list supports level/parent filters; UI can operate as L1–L4.
5. Settings enforce max depth / leaf-posting when enabled.
6. SQL Server tests for CostCenter + AccountDefaults + CoA level validation.

### Vouchers v1

1. Menu parity: Cash Payment/Receipt, Bank Payment/Receipt, Journal Voucher.
2. CPV form fields from reference are creatable/editable and persist; numbering uses `CPV-…` style prefixes.
3. Status only via approve / reject / post / unpost / reverse — no free Status on update.
4. Approved queue + post/unpost actions match reference intent.
5. SQL tests for typed create + CPV header fields + full status lifecycle + illegal transitions.

### Accounts Reports v1

1. Menu lists reference reports; R1 items return real data (no placeholder messages).
2. TB, running TB, ledger, L4 ledger, CoA (+ detail), voucher report, income statement, financial statement (BS) work with date params + CSV export.
3. R2 summaries available after typed vouchers.
4. Stock average rate owned by Inventory — ✅ WAC API + ledger.
5. Docs: this file + `DATA_MODEL.md` + memory note when shipped.

---

## Out of scope (v1.1+)

- Multi-segment CoA (company / CC / natural as separate segments)
- Intercompany / consolidation charts
- Automatic depreciation (fixed assets depth)
- Full bank statement import / auto-match
- Cheque/print voucher layouts, multi-currency cash books beyond existing FX fields
- Fancy PDF layouts / report designer (CSV + simple print OK for v1)
- Changing tenancy model or Nest/Zallo rewrite
