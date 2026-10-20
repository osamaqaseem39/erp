# ERP Product Completion Plan

**Goal:** Ship a **complete SMB multi-tenant ERP SaaS** — not a scaffold, not a SAP clone.  
**Stack to finish:** .NET modular monolith + `frontend-new` against live API.  
**Explicitly deferred:** NestJS/Zallo rewrite, DB-per-tenant, GraphQL parity (REST wins).

Live API: [Swagger All modules](https://erp-api.tekvers.com/swagger/index.html?urls.primaryName=All%20modules) (765 ops local export).  
Backend detail slices: [BACKEND_COMPLETION_PLAN.md](./BACKEND_COMPLETION_PLAN.md).  
**Operating rules** (how work must be done; wins on conflict with this schedule): [operating/rules.md](./operating/rules.md) · [operating/PROJECT_CONSTITUTION.md](./operating/PROJECT_CONSTITUTION.md).

---

## What “complete” means (product DoD)

The product is **complete** when all of the following are true:

| # | Criterion |
|---|-----------|
| 1 | Every public REST operation either **works on real data** or is **removed** from Swagger |
| 2 | Core business cycles work end-to-end: **O2C**, **P2P**, **stock**, **basic GL**, **HR leave/attendance**, **project delivery**, **light MFG** |
| 3 | Platform is a usable SaaS shell: setup, auth, tenants, users/roles/permissions, password reset, sessions, audit |
| 4 | `frontend-new` covers every complete backend module with list/detail/create/edit + lifecycle actions (no dead buttons) |
| 5 | Multi-tenant isolation holds on every mutating path; permissions enforced |
| 6 | One smoke E2E path per cycle (API + UI) passes on a staging tenant |

**Not required for v1 complete:** advanced MRP, multi-company consolidations, EDI, full BI warehouse, marketplace billing, mobile apps, 2FA (nice-to-have), Nest migration.

---

## Current honesty snapshot

| Area | Reality |
|------|---------|
| CRM | Most mature — keep and harden |
| Admin / auth | Wired; contract hygiene in progress |
| Sales / POS | 3 real APIs; **lines not persisted** |
| Inventory | Large surface; stock paths exist — verify depth |
| Finance / HRM | Many routes; **stub services** in places |
| Procurement | Vendor real; **PO+rest stubs** |
| Projects / MFG / Reports | Thin headers; children / BOM / real execute missing |
| `frontend` (old) | Template + partial ERP bindings |
| `frontend-new` | New Vite app — bind to finished APIs as backend lands |

---

## Brainstorm — complete capability map

### A. Platform (must)

- Super-admin setup, tenant CRUD (no client DB secrets)
- Login / refresh / logout / me / change-password / forgot+reset / verify-email
- Sessions list + revoke (+ existing sign-out-everywhere)
- Users, roles, permissions (enforced on API)
- System settings, audit logs, system logs
- Company context + dashboard summary (real aggregates)
- Email sender abstraction (SMTP/provider) for reset / verify

### B. Order-to-cash (must)

- CRM: lead → opportunity → quote (+lines) → sales order (+lines) → approve/ship/invoice
- Sales invoice (+**lines**) post/void; link to Finance AR
- POS: session open/close; transaction (+**lines**); cash drawer totals
- Inventory: ship deducts stock; reservations optional v1.1
- Payments applied to invoices; customer balance

### C. Procure-to-pay (must)

- Vendor (done) → PR → PO (+lines) approve/reject → GRN (+lines) → stock in → purchase invoice → AP bill → payment
- RFQ optional v1.1 if timeboxed after PO/GRN/PI

### D. Inventory (must)

- Items, UoM, warehouses, stock balance/ledger
- Adjust / transfer; barcode optional
- Batch/serial where manufacturing needs them
- **Single stock writer** rule (MFG/GRN/ship only via Inventory services)

### E. Finance (must)

- CoA, journals (balanced post), AR invoices, AP bills, payments, taxes, currencies/FX
- Banks (accounts + simple reconcile later)
- Trial balance, P&L, balance sheet, aged AR/AP from real data
- Fixed assets thin OK for v1

### F. HRM (must for “complete HR lite”)

- Employees, departments
- Leave types + requests approve/reject
- Attendance check-in/out
- Simple payroll run (gross → net one formula) — enough to close cycle
- Recruitment / performance / training / benefits → v1.1 backlog

### G. Projects (must)

- Project + tasks + milestones + resources + documents
- Start/pause/complete/cancel; task complete; milestone reach

### H. Manufacturing (must = light MFG)

- Work centers, production orders (+lines), production runs
- BOM header/lines; complete order issues components / receives FG via Inventory
- Full routing/capacity calendars → v1.1

### I. Reports (must)

- Report definitions CRUD
- Runner for **catalog** reports (TB, P&L, BS, stock, sales, PO) — not arbitrary SQL from clients
- Schedule metadata + optional Hangfire/worker for email delivery
- Export CSV/PDF for catalog reports

### J. Frontend-new (must)

- Auth + setup + tenant switcher
- Module shell driven by permissions
- Screens for every “must” entity above (list/form/actions)
- Dashboard from `/api/core/dashboard/summary`
- OpenAPI/types sync (`gen:schemas` or hand-maintained clients)
- Retire old `frontend` from “supported” once parity reached

### K. Quality & ops (must)

- Tenant filter regression tests
- Integration tests: O2C, P2P, MFG complete, leave approve
- Staging deploy + smoke checklist
- Swagger responses typed; no secret fields on DTOs

---

## Delivery program (end-to-end)

Work in **vertical slices**: backend honest → API test → `frontend-new` screen → smoke.

```text
Wave 0   Hygiene + Platform contracts          [backend] then auth UI
Wave 1   Sales lines + POS lines               [backend] then Sales/POS UI
Wave 2   Procurement PO→GRN→PI                 [backend] then Procurement UI
Wave 3   O2C + P2P glue (stock + AR/AP)        [backend ✅ stock + PI→bill + SI→AR + payments] then CRM ship/invoice UX
Wave 4   Finance journals/AP/banks + reports   [backend ✅ journals/banks/COA/recon/reports] then Finance + Reports UI
Wave 5   Projects children + MFG + BOM         [backend ✅] then Projects/MFG UI
Wave 6   HRM employee/leave/attendance/payroll [backend ✅] then HRM UI
Wave 7   Platform forgot-password + sessions   [backend ✅] then account UI
Wave 8   Dashboard, permissions polish, E2E    [backend ✅; frontend-new primary; staging E2E in post-v1]
Wave 9   Hardening, remove dead routes, docs   [both — post-v1 polish]
```

Backend phase mapping (detail in [BACKEND_COMPLETION_PLAN.md](./BACKEND_COMPLETION_PLAN.md)):

| Wave | Backend phases |
|------|----------------|
| 0 | P0 |
| 1 | P1A |
| 2 | P1B |
| 3 | P2 |
| 4 | P3 Finance + P1C Reports |
| 5 | P1D + P4 |
| 6 | P3 HRM |
| 7 | P5 |
| 8–9 | P6 + frontend polish |

---

## Frontend-new track (parallel after each wave’s API freezes)

1. Point env at `https://erp-api.tekvers.com` (or local).
2. Generate or maintain typed API client from Swagger module JSON.
3. Per module: routes → list → form → lifecycle buttons wired to real endpoints.
4. Empty/error/permission states — no fake demo numbers.
5. When a backend route is stub/`501`, hide or disable the UI control (don’t fake success).

---

## Backlog after “complete” (v1.1+)

**Customer depth (whole product):** [CUSTOMER_DEPTH_PLAN.md](./CUSTOMER_DEPTH_PLAN.md) — geo/branch/terms, dispatch, reservations, bins/kits/cycle count, landed cost, 3-way match, packaging Sale, routing/scrap, timesheets, advances, opening balances, period close, cheque book, expense claims, SMTP/PDF/report permissions.

Also deferred / adjacent:
- Billing/subscriptions (SaaS marketplace)
- GraphQL parity, Nest/Zallo migration, DB-per-tenant
- Mobile / customer portal

---

## Rough effort (one focused backend + one frontend stream)

| Wave | Backend | Frontend-new | Calendar (indicative) |
|------|---------|--------------|------------------------|
| 0 | 2–3 d | 3–5 d | week 1 |
| 1 | 3–5 d | 3–5 d | week 2 |
| 2 | 1–2 w | 1 w | weeks 3–4 |
| 3 | 1–2 w | 1 w | weeks 5–6 |
| 4 | 2–3 w | 1–2 w | weeks 7–9 |
| 5 | 2–3 w | 1–2 w | weeks 10–12 |
| 6 | 2 w | 1–2 w | weeks 13–14 |
| 7 | 1–2 w | 3–5 d | weeks 15–16 |
| 8–9 | 1–2 w | 1 w | weeks 17–18 |

~**4–5 months** calendar for a complete SMB ERP v1 with two streams; less if stubs are ruthlessly deleted instead of implemented.

---

## Decision rules (keep the product shippable)

1. **Implement or delete** — never leave a public stub that returns empty success.
2. **Catalog reports only** — no arbitrary client SQL.
3. **Inventory is the only stock writer.**
4. **REST-first**; GraphQL follows finished services.
5. **Shared DB + TenantId** until after v1 complete.
6. Prefer **deleting** HRM/MFG/Finance edge endpoints over years of half-stubs.

---

## Immediate next action

Start **Wave 0 + Wave 1 backend**: finish Platform contract ship, then **Sales invoice + POS transaction line persistence** (highest ROI, APIs already live). Mirror in `frontend-new` as soon as OpenAPI updates.
