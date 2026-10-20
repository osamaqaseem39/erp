# ERP operating rules (one screen)

Load this before coding. Full law: [PROJECT_CONSTITUTION.md](./PROJECT_CONSTITUTION.md). Memory: [memory.md](./memory.md).

1. **Precedence:** `rules.md` → constitution + `ERP-FIX-*` → data model/schema → product/backend plans → comments.
2. **One app, one SQL Server DB**, modules by folder; shared DB + `TenantId`. No Nest/Zallo cutover in v1 work.
3. **New tables** go in [DATA_MODEL.md](./DATA_MODEL.md) + schema **before** services/controllers.
4. **UUID PK**; business numbers secondary. Money = `decimal`. Times = UTC.
5. **Posted / audit rows are immutable.** Fix by void or reverse. No hard delete after leave-draft (ERP-FIX-1).
6. **Status only via transition map** + dedicated endpoints. Do not honour free `Status` on update (ERP-FIX-5). UI buttons = legal next states.
7. **Concurrency in SQL**, not UI-only. Single stock writer.
8. **Config over magic numbers** for new thresholds (`Core.Settings` / dated config).
9. **Permission + tenant scope** on every changed mutating API (ERP-FIX-3). Approver ≠ creator (ERP-FIX-6).
10. **No external outbox in v1** (ERP-FIX-4). Idempotent design when you add integrations later.
11. **Tests on real SQL Server** for persistence/workflows (ERP-FIX-7). Keep one O2C + one P2P E2E green.
12. **Same PR:** code + docs when behaviour changes. New decisions get the next `ERP-FIX-n`.
