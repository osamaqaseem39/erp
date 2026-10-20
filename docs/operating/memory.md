# Operating memory

Append-only session notes for agents and humans. Do not rewrite history; add a new dated entry.

---

## 2026-10-21 — WP 1.2 tenant fail-closed (in progress)

- FinanceGap Post* methods call `RequireTenant`.
- WMS putaway/pick create/start/complete + phase2 create paths return empty/false on `Guid.Empty` tenant.
- Test: `Putaway_Create_FailsClosed_OnEmptyTenant`. Full suite still green.

## 2026-10-21 — WP 1.1 / 1.6 kill stubs + FinanceGap GL

- Wired 23 former stub report codes in `ModuleReportQueryService` to existing query methods.
- FinanceGap fund/credit/debit memo post creates balanced journals via `IAccountingService` and sets `PostedJournalId` (requires AR/AP defaults for memos; distinct From/To for transfers).
- Added WP 1.7 for TaxTransactions writers. Tests: 553/553.

## 2026-10-21 — WP 0.2 docs honesty

- Flipped BUSINESS_MODULES DI column to Yes for Finance/HRM/Inventory/Procurement/MFG/Projects.
- Unchecked CUSTOMER_DEPTH tracker Finance + Reports; fixed §11 Already have; expense-claim GL DoD marked not yet.
- REPORTS / BACKEND / FEATURES / PRODUCT honesty snapshots refreshed for stub report codes + FinanceGap/JournalService gaps.
- `frontend-new/docs/FRONTEND_NEW_COVERAGE.md` notes expense/gap post without GL + report catalog honesty gap.

## 2026-10-21 — WP 0.1 / 0.3 / 0.4 done (autonomous growth)

- Created integration branch `growth` and WP branch `wp/0.1-inventory-wip`.
- Inventory: Finance/MFG/WMS/Projects schemas are committed + DI-wired; honesty gaps are semantic (finance gap post without GL; ~20 report codes were empty-success; dead `JournalService` Post/Reverse returned true).
- Fixed journal line totals/post: `GetEntriesByJournalIdAsync` no longer `Include`s required Account (INNER JOIN dropped lines without CoA). `MapJournal` prefers entry sums.
- Report stub pack + unknown codes now `NotSupportedException` (no empty success).
- `JournalService.PostAsync`/`ReverseAsync` return false.
- Tests: tenant-aware stock level asserts; stock-transfer approve seeds stock / insufficient fails on approve; serial GraphQL update respects FIX-5 Status ignore.
- Baseline: build 0 errors; **553/553** tests; `frontend-new` build OK.
- Legacy `frontend/` MFG/Reports/CRM WIP **not committed** (WP 0.4) — primary UI remains `frontend-new`.
- Added WP **1.6** (finance gap → GL) and expanded **3.1** for stub report implementation/hide; Phase 0 still needs **0.2** docs honesty.

## 2026-10-21 — Autonomous growth prompt adopted

- Added `docs/operating/AUTONOMOUS_GROWTH_PROMPT.md` (self-driving loop).
- Seeded `GROWTH_PLAN.md` (WPs 0.1–6.2 + phase gates) and `PROGRESS.md` ledger (all TODO).
- Linked from constitution docs table and `AGENT_MASTER_PROMPT.md`.

## 2026-10-20 — Operating playbook adopted

- Created `docs/operating/` (constitution, rules, agent prompt, data model stub, this file).
- Seeded `ERP-FIX-1`…`ERP-FIX-7` for known gaps (soft-delete, dual schema, permissions, no outbox, status DTO, self-approve, in-memory tests).
- Linked product/backend/architecture docs to operating precedence.

## 2026-10-20 — Wave 1–2 workflow enforcement

- Added `ERP.Application/Common/DocumentWorkflow.cs` (transition + same-actor helpers).
- Sales invoice: transition map Draft→Posted|Voided, Posted→Voided; create forces Draft; update ignores Status; delete only Draft; post/void gated.
- Purchase order: transitions + create Draft; update ignores Status; approve requires map + **approver ≠ creator**; reject/cancel gated; GRN receipt uses `ReceiptTransitions`.
- GRN: Draft|Verified→Posted|Cancelled; create Draft; update ignores Status; **self-post allowed** (receiver); post immutability.
- Purchase invoice: Pending|Draft→Approved|Cancelled; create Pending; self-approve blocked; approved immutable for update/delete.
- Controllers: `[PermissionResource]` on sales invoices, PO, GRN, PI, PR.
- Tests: `SalesInvoiceWorkflowTests`, `ProcurementWorkflowTests`; fixed persistence tests for separate Approver. Filter run: 15 passed including Wave2 O2C/P2P glue.
- Follow-ups: SQL-level transition guards; server idempotency store; PR self-approve (ERP-FIX-6) when PR approve is hardened; dated config beyond Settings; external ledger outbox when needed.
