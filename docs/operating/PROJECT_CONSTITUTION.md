# ERP Project Constitution

Governing rules for this repository. Completion plans decide **when**; this file decides **how**.

**How to use (new projects):** copy `docs/operating/` into the target repo, fill the parameter table, then start from [rules.md](./rules.md).

---

## Parameter table (this repo)

| Parameter | Value |
|-----------|--------|
| Product | ERP SaaS (SMB multi-tenant) |
| Application | One .NET 9 modular monolith (`ERP.SaaS.sln`) |
| Database | One SQL Server database (`erpnew`), shared schema + `TenantId` |
| Module layout | Folders under Domain / Application / Infrastructure / Api; Core tables in schema `Core`, others mostly `dbo` |
| Table prefixes | Prefer schema or clear entity names; new tables document here before code. Do not invent cross-SKU prefixes (`plat_`, etc.) while on SQL Server monolith |
| Pilot dataset | Staging tenant used for one O2C and one P2P E2E journey (see tests under `ERP.Tests`) |
| External ledger | **None in v1.** Outbox / Manager.io-style queue is specified under Integrations but not built |
| Portals | `frontend-new` (primary admin UI); `frontend` (legacy, maintenance only) |
| API surface | REST primary; GraphQL secondary; live Swagger at erp-api.tekvers.com |
| Brand / UI tokens | `frontend-new` CSS variables: `--primary`, `--muted`, `--dur`, `--ease` (see `frontend-new/web/src/index.css`). Do not introduce a parallel palette |
| Test engine | Same SQL Server engine as runtime (no in-memory EF as the system of record for integration tests) |

---

## Documentation precedence

When documents disagree, higher wins:

1. [rules.md](./rules.md) — hard constraints (one screen)
2. This constitution + decision IDs (`ERP-FIX-*`)
3. [DATA_MODEL.md](./DATA_MODEL.md) and `ERP.Infrastructure/Database/Schemas/*`
4. [PRODUCT_COMPLETION_PLAN.md](../PRODUCT_COMPLETION_PLAN.md) / [BACKEND_COMPLETION_PLAN.md](../BACKEND_COMPLETION_PLAN.md)
5. Module status docs and code comments

New tables are described in `DATA_MODEL.md` (and schema SQL) **before** application code.  
When code and docs diverge after a merge, fix the docs in the same PR or open an `ERP-FIX-n`.

Related agent files: [AGENT_MASTER_PROMPT.md](./AGENT_MASTER_PROMPT.md), [AUTONOMOUS_GROWTH_PROMPT.md](./AUTONOMOUS_GROWTH_PROMPT.md), [GROWTH_PLAN.md](./GROWTH_PLAN.md), [PROGRESS.md](./PROGRESS.md), [memory.md](./memory.md).

---

## 1. Documentation

| Document | Purpose |
|----------|---------|
| `rules.md` | Non-negotiables agents load first |
| `PROJECT_CONSTITUTION.md` | Full operating law |
| `DATA_MODEL.md` | Table index / ER notes before code |
| `memory.md` | Append-only running session memory |
| `AGENT_MASTER_PROMPT.md` | Paste-once agent context |
| `AUTONOMOUS_GROWTH_PROMPT.md` | Self-driving growth loop (paste or `/loop`) |
| `GROWTH_PLAN.md` | Growth work packages + phase gates |
| `PROGRESS.md` | Growth ledger between sessions |
| Product / backend plans | Schedule and DoD |
| `ERP-FIX-n` decisions | Numbered resolutions when rules conflict with legacy |

Decisions get stable IDs (see [Decision log](#decision-log-erp-fix-)). Do not silently reverse a decision; supersede with a new ID.

---

## 2. Architecture

- **One application, one database.** Modules are folder + DI boundaries, not separate deployables for v1.
- **Commands and queries** use repositories per entity; do not bypass tenant filters with ad-hoc queries on mutating paths.
- **Same database engine everywhere**, including integration tests (SQL Server).
- Host composition: `ERP.Api` → Application services → Infrastructure repositories → `ERPDbContext` / Dapper SPs.
- Tenancy: shared DB + `TenantId` from JWT (`TenantMiddleware` / `ITenantProvider`). Database-per-tenant is deferred.

---

## 3. Data model

- Primary keys: **UUID** (`Guid`). Business numbers (invoice no., PO no.) are **secondary** unique keys per tenant.
- Money: **`decimal`**. Never float/double for amounts.
- Timestamps: **UTC** (`DateTime.UtcNow` / store UTC).
- Derived values (balances, rolled-up statuses): either a **view** or written by a **single** service/SP — never multiple writers.
- Management / org assignments (when HRM scope lands): carry **start and end** dates; “current” means `EndDate IS NULL` or `EndDate >= today`.

---

## 4. History

- Prefer **no hard delete** of business documents once they have left draft (use void / cancel / reverse).
- **Posted and audit records** are not updated in place; mistakes are fixed by **reversal** (or void) that preserves the original.
- Each financial/rate-sensitive record keeps the **rate or policy** it used at posting time.
- Soft flags (`IsActive`) may remain for master data until replaced by dated assignments ([ERP-FIX-1](#erp-fix-1--soft-delete-via-isactive)).

---

## 5. Workflows

- Every status change goes through an **allowed-transition map** in the application service (SQL guards follow when tables are hardened).
- UI action buttons are driven by **legal next states** (`when:` / `becomes:` in `frontend-new`), not free-form status edits.
- Terminal states (Posted, Voided, Cancelled, Completed, Paid, etc.) are guarded explicitly.
- Each transition should record **who, when, why** (and from-state) where columns exist; extend entities rather than silent status flips.

---

## 6. Concurrency

- Double-booking and “only one active” rules are enforced in **SQL** (filtered unique indexes, app locks, indexed views) — never only in the UI.
- Stock movements have a **single writer** path (`IStockService` / stock ledger).

---

## 7. Configuration

- Business numbers (days, rates, thresholds) come from **configuration** (`Core.Settings` or a dated config table), not magic literals in services for new Wave 2+ work.
- Config rows that vary over time include effective dates and scope (tenant / company) when introduced.

---

## 8. Security

- Access requires: **permission** + **organisational/tenant scope** + (where modelled) a **current assignment**.
- Mutating endpoints use `[PermissionResource]` / `[RequirePermission]` consistently.
- Approvals: whoever **starts** a request must not be the **final approver** on that same request (enforce on approve paths).

---

## 9. Integrations

Pattern retained from Manager.io-style rollouts (build when an external ledger exists):

- External calls go through a **queue** with a **duplicate-safe idempotency key**.
- Each external API is tested and documented before product code depends on it.
- Unclear send results are **looked up** before retry.
- Missing account mapping **stops** the job from being queued.
- Dry run + totals reconciliation before go-live.

**v1 status:** not implemented ([ERP-FIX-4](#erp-fix-4--no-external-ledger-outbox-in-v1)). Client may send `Idempotency-Key` on POST; server-side store is still incomplete.

---

## 10. Admin UI

List / form checklist:

- Shared page components from `frontend-new` engine; no one-off CRUD shells for standard entities.
- Blocked actions explain **why**.
- Status is never conveyed by **colour alone** (label + tone).
- Lifecycle buttons match server-legal transitions.

---

## 11. Testing

- Integration tests run on the **real SQL Server** engine.
- For each hardened document type include: conflict / concurrency, insert-only or posted-immutable, illegal transition, duplicate-replay where idempotent.
- Keep **one O2C** and **one P2P** end-to-end journey green (`ERP.Tests`).

---

## 12. Delivery

- Build bottom-up: Domain → Repository → Service → Controller → validation → tenant filter → tests (see backend plan).
- Planning answers use a fixed format: goal, constraints, touched files, test plan, risks.
- Go-live gates: signed recovery targets (RTO/RPO), runbook, UAT checklist, operating rules satisfied on in-scope modules.

---

## Decision log (`ERP-FIX-*`)

### ERP-FIX-1 — Soft-delete via `IsActive`

**Context:** Constitution prefers insert-only / no delete; codebase uses `BaseEntity.IsActive` and hard `DeleteAsync` on drafts.  
**Decision:** Master data may keep `IsActive`. Posted transactional documents must not be hard-deleted; void/cancel/reverse only. Draft hard-delete allowed until a dedicated archive table exists.  
**Supersedes:** nothing.

### ERP-FIX-2 — Dual EF + SQL schema deploy

**Context:** EF migrations and `Database/Schemas/*Module_Schema.sql` both exist.  
**Decision:** For tables you touch, keep EF model and schema SQL aligned in the same change. Do not add a third source of truth. Full unification is deferred.  
**Supersedes:** nothing.

### ERP-FIX-3 — Uneven permission attributes

**Context:** CRM/Admin patterns use permissions; some Sales/Procurement/MFG routes historically relied on `[Authorize]` only.  
**Decision:** Every **new or changed** mutating endpoint must declare permissions. Backfill Sales invoice + Procurement PO/GRN/PI/PR in the Wave 1–2 enforcement slice.  
**Supersedes:** nothing.

### ERP-FIX-4 — No external ledger outbox in v1

**Context:** Integrations section describes queue + mapping gates.  
**Decision:** Document the pattern; do not build Manager.io/outbox until an external ledger is in scope. Finance AR/AP stay in-process.  
**Supersedes:** nothing.

### ERP-FIX-5 — Status updates via DTO

**Context:** Update DTOs often expose `Status`.  
**Decision:** Clients must not free-set status on update for transition-managed documents. Status changes go through dedicated actions (post, void, approve, reject). Services ignore or reject raw status writes when a transition map exists.  
**Supersedes:** nothing.

### ERP-FIX-6 — Requester cannot final-approve

**Context:** Approve endpoints historically allowed any permitted user, including creator.  
**Decision:** On approve, reject if `ApprovedBy`/actor equals document creator (`CreatedBy`) when both are present.  
**Supersedes:** nothing.

### ERP-FIX-7 — In-memory test DB

**Context:** Some unit tests may use substitutes.  
**Decision:** Persistence and workflow tests that claim integration coverage must use SQL Server. Pure mapper/unit tests may stay in-memory.  
**Supersedes:** nothing.

---

## Go-live checklist (excerpt)

- [ ] RTO/RPO agreed and written in runbook
- [ ] Restore drill documented
- [ ] UAT checklist signed for pilot tenant
- [ ] O2C + P2P E2E green on staging
- [ ] Permissions + tenant filters on all in-scope mutating APIs
- [ ] No unresolved `ERP-FIX` that blocks pilot scope
