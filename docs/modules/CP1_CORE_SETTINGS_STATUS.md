# Checkpoint 1 — Core / Settings: Status

> Branch `feature/erp-ui-foundation` · commits `39a47e2`…`534d9b1` · last updated 2026-09-29
> Plan: [CP1_CORE_SETTINGS_PLAN.md](CP1_CORE_SETTINGS_PLAN.md) (decisions D1–D8 approved as recommended)

**Gate B status: code complete. It needs your review of one migration, then your walk-through (§4).**

## 1. Results

| Check | Before CP1 | Now |
|-------|-----------|-----|
| Backend tests | 42 | ✅ 103 |
| Frontend tests | 99 | ✅ 118 |
| `npm run build` / `tsc` | ✅ | ✅ |
| `npm run lint` | 0 errors · 686 warnings | ✅ 0 errors · 632 warnings (legacy only; all new code linted strictly) |
| `dotnet build` | ✅ | ✅ |

## 2. What shipped

| WP | Commit | Delivered |
|----|--------|-----------|
| WP1 | `39a47e2` | **Tenant isolation.** Settings, audit log and system log are scoped to the caller's tenant (fail-closed). **Real caller identity in 41 controllers** (details in §3). Logs are read-only over the API. Tenant administration (REST and GraphQL) is SuperAdmin-only. |
| WP1+ | `59b5299` | **Platform-wide tenant write guard:** no non-SuperAdmin write can touch another tenant's rows, whatever a service copies from the request body. |
| WP2 | `cc4786d` | **Automatic audit trail** for every create/update/delete (changed fields before/after, secrets redacted). **System log** of server errors (with the trace id users see) and security events (failed sign-ins, lockouts). |
| WP3 | `9c8a73e` | **Settings catalog** (preferences, security, email) with validation. **Real encryption** for secrets (write-only). Read-only enforced. |
| WP4 | `a77ac2b` | **Company profile API** and the tenant formatting context (currency, locale, time zone) the UI applies after sign-in. |
| WP5 | `0090fbc` | **Per-tenant document numbering** (`QT-2026-00001`), issued atomically. CRM leads, opportunities, quotes, orders and converted customers use it. **One migration** (§4). |
| WP6–8 | `68fe6a7`, `c90b7c9` | **Module registry** driving tiles, sidebars and landing pages for every module. **Core pages:** System health dashboard, settings hub, company, preferences, security, email, numbering, reference data, audit and system log explorers. Customer **History tab now shows real audit entries**. |
| WP9 | `534d9b1` | Removed the `/erp/*` duplicates and redirected every old URL (ids and query strings kept). |

## 3. Defects found and fixed

| Severity | Defect | Fix |
|----------|--------|-----|
| Critical | Settings, audit logs and system logs ignored the caller's tenant, so any user could read (and for settings, change) every tenant's data. | Tenant-scoped, fail-closed repositories. Tests prove A can't read, change or delete B's rows. |
| Critical | Any signed-in user could create, update or delete tenants (REST and GraphQL), and read any tenant's audit log via GraphQL. | SuperAdmin only. |
| Critical | 37 create paths copy `TenantId` from the request body, so a user could plant rows in another tenant. | `TenantGuardInterceptor` on every save (403). Services still copy the value, but it no longer matters. Each module cleans this up as it's migrated. |
| High | 13 controllers invented identity (`Guid.NewGuid()` tenant/user, `"system"`). Most others read `tenant_id`/`user_id` claims the token never contains, so role management and many Finance/HRM/Inventory endpoints could never succeed with a real sign-in. | One `CurrentUser` helper (tenant from middleware, user from `NameIdentifier`, company from a new `CompanyId` claim). Missing values give 401. A test blocks placeholders from coming back. |
| High | Audit entries could be forged, and system logs edited or deleted, by any user. | Write endpoints removed. Entries are only written by the server. |
| High | "Encrypted" settings were plain text and returned by the API. Read-only wasn't enforced. | Data Protection encryption, values never returned, 409 on read-only. |
| High | 7 of 9 module tiles led to 404, and 7 modules had no sidebar. | Registry-driven navigation, plus a test that every link resolves. |
| Medium | Document numbers were random GUID fragments. | Sequential per-tenant numbering. |
| Medium | Settings created via the old controller got a random tenant and author. | Fixed by the identity helper and scoping. |

## 4. Your steps (Gate B)

### 4a. Review the migration before restarting the API
The API applies EF migrations automatically at startup.
- File: `ERP.Infrastructure/Migrations/20260929132407_AddDocumentSequences.cs`
- It only creates `Core.DocumentSequences` and the unique index `IX_DocumentSequences_TenantId_SequenceKey`. Nothing existing is altered.
- Rollback (after stopping the API): `dotnet ef database update FixTenantNameColumn --project ERP.Infrastructure --startup-project ERP.Api`
- I deliberately kept the known ReturnOrder foreign-key drift out of it (see §6).

### 4a-bis. Found while running (2026-09-29)
EF Core's startup `Migrate()` had been **failing on every start, before Checkpoint 1**: the ReturnOrder foreign-key drift between `ERPDbContext` and the model snapshot tripped EF 9's pending-model-changes check, so no migration could apply. `20260929144919_SyncReturnOrderForeignKeys` is a **snapshot-only** migration that runs no SQL: the database already has those keys on `CRM.ReturnOrders` as `ON DELETE NO ACTION`, which matches the model. With it in place, both migrations applied on your database and `Core.DocumentSequences` exists.

### 4b. First start
- A Data Protection key ring is created at `ERP.Api/App_Data/keys` (override with `DataProtection:KeysPath`). **Back it up.** It's gitignored. Losing it means re-entering the SMTP password, not losing data.
- Sign out and back in once, so your token carries the new `CompanyId` claim (the company profile needs it).

### 4c. Walk-through
- [ ] `/modules`: every tile opens a page (none 404). Each module shows its sidebar.
- [ ] `/core`: System health shows real counts. The setup checklist links work.
- [ ] Company profile: change the currency to PKR and the time zone to Asia/Karachi and save. Money on Customers now shows PKR formatting.
- [ ] Preferences: change the language. Email: set the password, reload (shows "Stored (hidden)"), then clear it.
- [ ] Numbering: change the quote prefix and see the preview. Try lowering the next number (refused). Create a quote in CRM (gets `QT-2026-…`).
- [ ] Edit a customer, then open Customer › History and the Audit log. The change appears with before/after values.
- [ ] Sign in with a wrong password twice; the System log shows two Security entries.
- [ ] Old links such as `/erp/admin/users` land on the new pages.
- [ ] As a non-SuperAdmin: Tenants and Advanced settings show "only platform administrators" (the API returns 403 too).

## 5. Deviations from the plan

- **The Tenants and Advanced settings pages were not rebuilt on the kit.** They're the existing screens, now SuperAdmin-only in both UI and API. They move to the kit in Checkpoint 2 (Admin), which rebuilds the other admin screens anyway.
- **Security settings are stored but not yet enforced.** Checkpoint 2 wires them into sign-in, and the page says so. **Email** settings are stored but nothing sends mail yet; that page says so too.
- **Added beyond the plan:** the platform tenant write guard, and a fix for identity in 41 controllers. Both were security fixes found during WP1.
- The Core dashboard's user figures link to the existing Users page (rebuilt in Checkpoint 2).

## 6. Known gaps carried forward

| Gap | Where |
|-----|-------|
| Permission codes aren't enforced by the API yet (the UI hides SuperAdmin-only items). | CP2 |
| The sign-in page lists every tenant name to anonymous visitors (`/api/tenants/login-options`); decision D8 was deferred. | CP2 |
| Services still copy `TenantId` from request bodies (neutralized by the guard). Remove per module. | Each module |
| `HandleAsync<T>`'s class constraint makes create endpoints return an empty body (fixed for Customers only). | CP3, then each module |
| EF maps ReturnOrders to `dbo.ReturnOrders`, but in this database the table is `CRM.ReturnOrders` (created by the SQL scripts), so return-order screens likely fail against this database. The FK drift itself was resolved by the snapshot-only migration below. | CP3 schema reconciliation |
| Changes with no signed-in user (first-run setup) aren't audited (`AuditLog.UserId` is a required FK). | Documented; revisit if needed |
| Document numbers skip a value if a document fails to save after taking it. Strictly gapless numbering for invoices. | CP6 |
| The second settings entity `Core/Setting` is unused apart from `Tenant.Settings`. | CP12 |
