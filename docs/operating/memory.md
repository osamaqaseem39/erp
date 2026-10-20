# Operating memory

Append-only session notes for agents and humans. Do not rewrite history; add a new dated entry.

---

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
