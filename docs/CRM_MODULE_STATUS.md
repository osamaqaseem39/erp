# CRM Module Status

> Living document for CRM and its dependent modules. Update this file as work completes.
>
> **Last reviewed:** 2026-05-29 (completion pass: lead→opportunity, line validators, frontend wiring)

---

## How to update this document

1. Change **Last reviewed** date when you edit.
2. Update layer **%** and entity table **Status** cells (`✅` / `🟡` / `❌`).
3. Move items in **Backlog** to **Changelog** when done (with date).
4. Re-run a quick grep/build if you claim DI or routes are fixed.

**Status legend**

| Symbol | Meaning |
|--------|---------|
| ✅ | Complete for that layer |
| 🟡 | Partial — works with known gaps |
| ❌ | Missing or not wired |

---

## Executive summary

The CRM module’s **8 core sales entities** have an end-to-end stack: domain → EF repositories → application services → REST API → React list/form pages. **Conversions** (lead → customer, lead → opportunity, opportunity → quote, quote → sales order) and **quote/sales-order line items** are implemented on the backend.

**Extended entities** (customer groups, price lists, territories, return orders) are **full stack** (API + validators + frontend CRUD).

**Dependent modules** (Core tenant/auth, company, users) are integrated on the backend. **Finance** DI is registered; CRM SO → Finance invoice creates an invoice header. Inventory catalog is wired on quote/SO lines; SO approve/ship reserves and issues stock. POS deep links remain partial.

| Layer | Estimate | Notes |
|-------|----------|-------|
| Domain | ~75% | 14 C# entities; 7 SQL-only tables not modeled |
| Infrastructure | ~92% | Line repos tenant-filtered; extended tables use dbo (aligned with migrations) |
| Application | ~95% | CRUD + all core conversions |
| REST API | ~95% | All CRM controllers `[Authorize]` |
| Validators | ~95% | Core + extended + quote/SO line rules |
| GraphQL | ~60% | Contact + campaign mutations; lead→opportunity mutation |
| Frontend | ~90% | Dashboard + extended UIs; activity complete wired |
| DB scripts | ~90% | Broader than runtime EF schema |
| Tests | ~15% | Line calculator + lead conversion unit tests |

---

## Core entities (8)

Primary sales pipeline: **Customer → Contact → Lead → Opportunity → Quote → Sales Order**, plus **Activity** and **Campaign**.

| Entity | Domain | Repo | Service | API | Validators | GraphQL | Frontend | EF DbSet |
|--------|--------|------|---------|-----|------------|---------|----------|----------|
| Customer | ✅ | ✅ | ✅ | ✅ | ✅ | Q + create | ✅ | ✅ |
| Contact | ✅ | ✅ | ✅ | ✅ | ✅ | Q + CRUD | ✅ | ✅ |
| Lead | ✅ | ✅ | ✅ | ✅ | ✅ | Q + create + convert ×2 | ✅ | ✅ |
| Opportunity | ✅ | ✅ | ✅ | ✅ | ✅ | Q + convert | ✅ | ✅ |
| Quote | ✅ | ✅ | ✅ | ✅ | ✅ | Q + convert | ✅ | ✅ |
| QuoteLine | ✅ | ✅ | via Quote | nested | ✅ | ❌ | in forms | ✅ |
| SalesOrder | ✅ | ✅ | ✅ | ✅ | ✅ | Q | ✅ | ✅ |
| SalesOrderLine | ✅ | ✅ | via SO | nested | ✅ | ❌ | in forms | ✅ |
| Activity | ✅ | ✅ | ✅ | ✅ | ✅ | Q + complete | ✅ | ✅ |
| Campaign | ✅ | ✅ | ✅ | ✅ | ✅ | Q + create/update | ✅ | ✅ |

### REST routes (core)

| Entity | Base route | Workflow |
|--------|------------|----------|
| Customer | `/api/crm/customers` | CRUD + search |
| Contact | `/api/crm/contacts` | CRUD + by customer |
| Lead | `/api/crm/leads` | CRUD + qualify/lose/convert (New/Qualified edit; convert from Qualified; FIX-5) |
| Opportunity | `/api/crm/opportunities` | CRUD + win/lose/cancel/convert (Open-only edit; convert from Won; FIX-5) |
| Quote | `/api/crm/quotes` | CRUD + send/approve/reject/convert (Draft-only edit; FIX-6; convert from Accepted) |
| Sales order | `/api/crm/sales-orders` | CRUD + approve/reject/ship/invoice (Draft-only edit; FIX-6) |
| Activity | `/api/crm/activities` | CRUD + complete/cancel/reschedule (Open-only edit; FIX-5) |
| Campaign | `/api/crm/campaigns` | CRUD + launch/pause/complete/cancel (Draft-only edit; FIX-5) |

### Frontend routes (core)

| Entity | List | New | Edit | Notes |
|--------|------|-----|------|-------|
| Dashboard | `/crm/dashboard` | — | — | KPIs + charts |
| Customer | `/crm/customers` | ✓ | ✓ | |
| Contact | `/crm/contacts` | ✓ | ✓ | |
| Lead | `/crm/leads` | ✓ | ✓ | Hub + list (`?view=list`) |
| Opportunity | `/crm/opportunities` | ✓ | ✓ | Convert to quote in list |
| Quote | `/crm/quotes` | ✓ | ✓ | Lines + send/convert |
| Sales order | `/crm/sales-orders` | ✓ | ✓ | Lines + workflow actions |
| Activity | `/crm/activities` | ✓ | ✓ | Complete action in list |
| Campaign | `/crm/campaigns` | ✓ | ✓ | |

### GraphQL (HotChocolate)

Registered in `ERP.Api/Program.cs`: `CRMQueries`, `CRMMutations`.

| Area | Status |
|------|--------|
| Queries | Customer, Contact, Lead, Opportunity, Quote, SalesOrder, Activity, Campaign |
| Mutations | Create customer/lead/contact/campaign; lead qualify/lose/convert; opp win/lose/cancel/convert; campaign launch/pause/complete/cancel; activity complete/cancel/reschedule; quote send/approve/reject/convert; update contact/campaign |
| Missing | Update/delete for most entities; SO workflow mutations |

### Key backend files

| Layer | Path |
|-------|------|
| Domain | `ERP.Domain/CRM/` |
| Services | `ERP.Application/CRM/` |
| Interfaces | `ERP.Application/Interfaces/CRM/` |
| Repositories | `ERP.Infrastructure/Repositories/CRM/` |
| Tenant filter base | `ERP.Infrastructure/Repositories/CRM/CrmRepositoryBase.cs` |
| Controllers | `ERP.Api/Controllers/CRM/` |
| Validators | `ERP.Api/Validators/CRM/` (incl. `CrmLineValidators.cs`) |
| DTOs | `ERP.Shared/DTOs/CRM/` |
| GraphQL | `ERP.Api/GraphQL/CRM/` |
| DI | `ERP.Api/Program.cs` |
| Line helpers | `ERP.Application/CRM/CrmLineCalculator.cs`, `CrmLineMapper.cs` |

### Key frontend files

| Area | Path |
|------|------|
| Entities | `frontend/src/modules/crm/entities/` |
| Routes | `frontend/src/routes/index.tsx` (`crmRoutes`) |
| Dashboard | `frontend/src/pages/crm/CrmDashboard.tsx` |

---

## Extended entities

| Entity | Domain | Repo | Service | API | Validators | GraphQL | Frontend |
|--------|--------|------|---------|-----|------------|---------|----------|
| CustomerGroup | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| PriceList | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| SalesTerritory | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| ReturnOrder | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |

| Route | Controller |
|-------|------------|
| `/api/crm/customer-groups` | `CustomerGroupController` |
| `/api/crm/price-lists` | `PriceListController` |
| `/api/crm/sales-territories` | `SalesTerritoryController` |
| `/api/crm/return-orders` | `ReturnOrderController` |

**Files:** `ExtendedCrmControllers.cs`, `ExtendedCrmRepositories.cs`, `ExtendedCrmServices.cs`, `ERP.Shared/DTOs/CRM/ExtendedCrmDtos.cs`

---

## SQL schema only (not in C# / EF)

Defined in `ERP.Infrastructure/Database/Schemas/CRMModule_Schema.sql`. **Repositories use EF only** — SPs are not called from app code.

| Table | App layer |
|-------|-----------|
| `CustomerAddresses` | ❌ |
| `CampaignMembers` | ❌ |
| `ContactPersons` | ❌ |
| `DiscountPolicies` | ❌ |
| `ReturnOrderItems` | ❌ |
| `PriceListItems` | ❌ |
| `ServiceContracts` | ❌ |

**Schema drift:** SQL scripts use `[CRM].*` naming; EF migrations use **dbo** (`QuoteLines`, etc.). Treat SQL scripts as reference.

---

## Dependent modules

| Module | CRM usage | Status | Notes |
|--------|-----------|--------|-------|
| **Tenant** | `CrmRepositoryBase` + `ITenantProvider` | ✅ | Main + line repos filtered |
| **Auth / JWT** | `[Authorize]` on CRM controllers | ✅ | |
| **Company** | `CompanyId` on entities | 🟡 | `getCompanyId()` on key forms |
| **Users** | Assigned-to fields | 🟡 | User picker not on all forms |
| **Permissions** | Fine-grained CRM | ❌ | |
| **Finance** | SO invoice | 🟢 | `SalesOrderService.InvoiceAsync` creates Finance invoice via `IInvoiceService` |
| **Inventory** | Line `ItemId` | ✅ | Quote/SO `ItemLineSelect` + `itemService.searchOptions`; SO approve reserves stock |
| **Sales / POS** | Separate invoice model | ❌ | |

---

## Known gaps (prioritized backlog)

### P0 — Correctness

- [x] Lead → Opportunity: service + REST + GraphQL + frontend
- [x] Extended EF schema aligned with migrations (dbo, not `[CRM]`)
- [x] CRM sales order → Finance invoice (Finance DI registered; invoice header created on SO invoice action)

### P1 — Product completeness

- [x] Remove dead CRM `*ApiService` wrappers
- [x] Quote/SO line validators (`CrmLineValidators.cs`)
- [x] Activity `complete` wired in Lead hub + Activity list
- [ ] GraphQL: update/delete for most entities; SO workflow mutations

### P2 — Extended CRM

- [ ] Domain + API for SQL-only tables (addresses, price list items, return lines, campaign members)

### P3 — Cross-module

- [x] Quote/SO lines ↔ Inventory products (`ItemLineSelect`)
- [ ] CRM permission keys
- [x] Register Finance DI + link SO invoice

### P4 — Quality

- [x] Unit tests: lead conversions (`LeadConversionTests.cs`)
- [ ] Integration/API tests for controllers
- [ ] Reconcile `CRMModule_Schema.sql` with EF (document strategy)

---

## Tests

| File | Coverage |
|------|----------|
| `ERP.Tests/Crm/CrmLineCalculatorTests.cs` | Line math + number prefix |
| `ERP.Tests/Crm/LeadConversionTests.cs` | Lead → customer, lead → opportunity |

---

## Changelog

| Date | Change |
|------|--------|
| 2026-05-17 | Initial status doc + first completion pass |
| 2026-05-29 | Lead→opportunity end-to-end; line validators; activity complete UI; remove dead ApiServices; EF extended schema fix; conversion tests; doc refresh |

---

## Quick reference — DI registration

Confirmed in `ERP.Api/Program.cs`:

- **Repos:** 8 core + 2 line + 4 extended
- **Services:** 8 core + 4 extended
- **Validators:** 24 (create/update × 12 entities) + line rules via `RuleForEach`
- **GraphQL:** `CRMQueries`, `CRMMutations`
- **Tenant:** `ITenantProvider` → `HttpContextTenantProvider`
