# Autonomous Execution Prompt — ERP Development & Growth

Paste everything below the line into Claude Code, or run it as `/loop <contents>`. It is self-driving: each run reads the progress ledger, does the next work packages, verifies them, records the result and continues. If a session ends, run the same prompt again and it resumes where it stopped. You never need to work out what is missing.

Related files: [GROWTH_PLAN.md](./GROWTH_PLAN.md) · [PROGRESS.md](./PROGRESS.md) · [rules.md](./rules.md) · [PROJECT_CONSTITUTION.md](./PROJECT_CONSTITUTION.md)

---

You are the sole engineer taking this ERP from “depth implemented in code” to **pilot-ready SaaS**, then growing it through the post-v1 backlog. Work autonomously until every work package in `docs/operating/GROWTH_PLAN.md` is DONE or BLOCKED. Do not ask me questions, do not stop to report partial progress, and do not ask me to re-check anything. Use your own judgement and the defaults below.

## Sources of truth (read at the start of every session)
1. `docs/operating/rules.md` — non-negotiable one-screen rules. Never violate them.
2. `docs/operating/PROJECT_CONSTITUTION.md` — architecture, ERP-FIX-* decisions, go-live gates. Never reverse an ERP-FIX without a new ID.
3. `docs/operating/GROWTH_PLAN.md` — the work packages (WPs), acceptance criteria and phase gates.
4. `docs/PRODUCT_COMPLETION_PLAN.md` — product DoD (O2C/P2P/stock/GL/HR/MFG/projects + `frontend-new`).
5. `docs/BACKEND_COMPLETION_PLAN.md` — backend honesty, tenant fail-closed, open GraphQL/REST gaps.
6. `docs/CUSTOMER_DEPTH_PLAN.md` — entity/behaviour depth map; tracker + cross-module flows.
7. `docs/ENTITY_TREE.md` — live domain inventory (regenerate from `ERP.Domain` when entities change).
8. `docs/REPORTS_COMPLETION_PLAN.md` and `frontend-new/docs/FRONTEND_NEW_COVERAGE.md` — reports and UI gaps.
9. `docs/operating/STAGING_E2E_CHECKLIST.md` — staging closeout vehicle.
10. `docs/operating/PROGRESS.md` — the ledger. Trust it over your recollection.
11. `docs/operating/memory.md` — append a short dated note after every DONE WP (never erase prior entries).
12. `docs/operating/DATA_MODEL.md` + `ERP.Infrastructure/Database/Schemas/*` — new tables **before** application code.

Prefer **live code** over stale narrative outside `docs/operating/` when they conflict; then fix the stale doc in the same change.

## Loop (repeat until nothing is TODO)
1. Read `PROGRESS.md`. Pick the next WP that is TODO and whose dependencies are DONE, in this priority:
   **Phase 0** (stabilize WIP / honesty) → **Phase 1** (backend gaps & tenant fail-closed) → **Phase 2** (frontend-new parity) → **Phase 3** (reports & prints) → **Phase 4** (cycle integrity: O2C‖P2P‖stock‖GL‖MFG‖HR) → **Phase 5** (staging E2E + go-live pack) → **Phase 6** (growth / v1.1).
   Never start Phase 5 exit gate until Phases 0–4 are DONE or BLOCKED. Never start Phase 6 product features until Phase 5’s “local smoke” gate passes (staging apply may stay BLOCKED).
2. Mark it IN_PROGRESS in the ledger.
3. Create a branch `wp/<id>-<slug>` from the latest integration branch `growth` (create `growth` from the current branch on first run).
4. Implement it fully, vertical slice where applicable:
   Domain → `DATA_MODEL.md` + numbered schema SQL (`ERP.Infrastructure/Database/Schemas/YYYYMMDD_*.sql`) and/or EF migration (never edit an applied migration) → repository → real service (no empty-success stubs) → FluentValidation → REST (`[PermissionResource]` / `[RequirePermission]`) → GraphQL only after REST is honest → `frontend-new` registry/page/actions if the WP has UI → tests for every acceptance criterion including negative/abuse/tenant isolation → docs honesty → append `memory.md`.
   For large WPs, split into module-sized commits, but finish the whole WP.
5. Verify — all must pass before marking DONE:
   - `dotnet build ERP.SaaS.sln -c Release` — 0 errors; no new warnings you introduced
   - `dotnet test ERP.Tests` — at minimum touched suites; always keep Wave2 O2C + P2P (and MFG stock / leave where relevant) green
   - Start API against a **scratch** SQL database (not staging/prod `erpnew`); apply pending `Database/Schemas/20*_*.sql` via the normal deploy path; re-run integration tests that need SQL Server
   - `cd frontend-new && npm run typecheck && npm run lint && npm run build` (use the package scripts that exist; do not invent a second UI app — `frontend` is maintenance-only)
   - Re-run every probe in the ledger’s **Regression probes** section
   - Check each WP acceptance criterion explicitly; record evidence (test name or command output)
6. If verification fails, fix and re-verify. After 3 failed attempts on the same cause, mark the WP BLOCKED with the exact error and move on to the next unblocked WP.
7. Commit (conventional message). Merge the WP branch into `growth` with `--no-ff`. Do not push and do not deploy.
8. Update `PROGRESS.md`: status DONE, date, commits, evidence, follow-ups (add follow-ups as new WPs with the next free id in the right phase — never leave them as prose only).
9. When a phase’s WPs are all DONE, run its exit gate from `GROWTH_PLAN.md`. Record PASS/FAIL with evidence. If FAIL, add WPs for the failures and continue.
10. Go back to step 1. Do not end your turn while any WP is TODO and unblocked.

When everything is DONE or BLOCKED: refresh the honesty snapshot in `PRODUCT_COMPLETION_PLAN.md` / `CUSTOMER_DEPTH_PLAN.md` tracker, regenerate or update `ENTITY_TREE.md` if entities changed, and write a final summary at the top of `PROGRESS.md`: what was done, what is BLOCKED, and the exact human action that unblocks each item.

## First-run bootstrap
If `GROWTH_PLAN.md` or `PROGRESS.md` are incomplete relative to the working tree, inventory uncommitted Finance/MFG/WMS/Projects/Reports gap work and add WPs under Phase 0/1 before coding. Do not invent greenfield scope outside the product/depth/reports plans.

## Decisions already made (do not ask)
- **Stack:** .NET 9 modular monolith `ERP.SaaS.sln` — Api / Application / Domain / Infrastructure / Shared / Tests. SQL Server shared schema + `TenantId`. Primary UI: `frontend-new`. Legacy `frontend` = maintenance only.
- **API:** REST primary; GraphQL secondary and follows finished REST. Live reference: erp-api.tekvers.com Swagger.
- **Tenancy:** shared DB + JWT tenant; **no** DB-per-tenant in this program.
- **No NestJS/Zallo/Postgres rewrite** and **no external ledger outbox** (ERP-FIX-4) until explicitly reopened with a new ERP-FIX.
- **Money:** `decimal`. **PKs:** UUID. **Times:** UTC. Posted/audit rows immutable — void/reverse only.
- **Status:** only via transition maps + dedicated actions (ERP-FIX-5). Approver ≠ creator (ERP-FIX-6).
- **Stock:** single writer through Inventory/`IStockService` (and documented successors). No cross-module direct stock table writes.
- **Reports:** catalog whitelist only — never arbitrary client SQL.
- **Email:** SMTP via tenant settings / `ITenantEmailSender`; tests mock the sender.
- **Schema:** EF model + `Database/Schemas/*` stay aligned for tables you touch (ERP-FIX-2). New numbered SQL patches; never rewrite applied ones.
- **Permissions:** every new/changed mutating endpoint gets permission attributes (ERP-FIX-3).
- Prefer existing patterns: repositories, application services, FluentValidation, `DocumentWorkflow`, tenant fail-closed helpers, `frontend-new` resource registry/engine.

## Hard rules
- Never weaken security, tenant isolation, or immutability to make a test pass. Never disable, skip, or delete an existing green test.
- No secrets in the repo. No client-writable connection strings on tenants. Production config via environment/settings with start-up checks where they already exist.
- Scratch databases only for agent verification; never point destructive tests at staging/production. Drop scratch DBs when done. Check free disk before large DB files.
- Do not push, open PRs, deploy to staging/production, or call paid external services. Do not run destructive git commands (`reset --hard`, force-push, etc.).
- Keep the code style of the surrounding module. Smallest change that satisfies the WP and the rules.
- Same change: code + docs when behaviour changes. Append `memory.md`; update `PROGRESS.md`.

## What you cannot finish alone — mark BLOCKED with the exact human step, then keep going
- Staging/production schema apply and E2E sign-off on a real staging tenant: prepare scripts + checklist evidence; mark deploy/sign-off BLOCKED.
- Real SMTP provider credentials / DNS: implement + mock tests; mark live mailbox verification BLOCKED.
- RTO/RPO agreement, restore drill, UAT signature: produce the pack; mark signatures BLOCKED.
- Penetration test / SOC-style audit: produce scope + evidence pack; mark third-party step BLOCKED.
- SaaS marketplace billing contracts / payment processor accounts: build against interfaces + mocks; mark live processor BLOCKED.
- Entra ID / Okta production tenants (if SSO is later added): local/dev IdP only; mark real tenant BLOCKED.

## PROGRESS.md format
See [PROGRESS.md](./PROGRESS.md). Keep the Work packages / Phase gates / Regression probes sections current.

Start now: read the sources of truth, refresh `PROGRESS.md` if needed, then begin the loop.
