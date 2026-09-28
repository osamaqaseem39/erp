# Checkpoint 0 — Baseline and Foundation: Status

> Branch `feature/erp-ui-foundation` · baseline commit `7ddb049` · last updated 2026-09-29
> Plan: [CP0_FOUNDATION_PLAN.md](CP0_FOUNDATION_PLAN.md) · Program: [../erp-ui-program/PLAN.md](../erp-ui-program/PLAN.md)

**Gate B status: code complete. The live walk-through (§4) still has to be done by you.**

## 1. Results against the baseline

| Check | Before | Now |
|-------|--------|-----|
| `tsc --noEmit` / `npm run build` | ❌ 1,018 errors, build fails (CI red) | ✅ 0 errors, build passes |
| `npm run lint` | ❌ could not run (no ESLint 9 config) | ✅ 0 errors · 681 warnings, all in legacy code |
| `npm test` (vitest) | 1 test | ✅ 105 tests in 15 files |
| `dotnet build` | ✅ (0 CS errors) | ✅ 0 errors · 330 warnings (was 370) |
| `dotnet test` | 9 tests | ✅ 42 tests |
| CI frontend job | build only | lint + test + build |

## 2. What shipped (by work package)

| WP | Commit | Delivered |
|----|--------|-----------|
| WP0 | `dacdca8`, `3148941` | Green build. Root causes fixed (generic sort hook, type re-exports, Redux 5 typing, Apollo v4) plus real bugs (two legacy admin pages that crashed on render, a PO form sending a field the API drops, swiper 11 imports). ESLint 9 flat config. CI runs lint and tests. |
| WP1 | `e816366` | One `ApiError` body for every endpoint, from one `ApiErrorMapper`. Removed **230** `catch (Exception)` blocks that returned every failure as 400 **with the raw exception text**. 55 validation responses now have field-keyed errors. Model-binding 400s use the same shape. |
| WP2 | `69af75b` | `PagedRequest` + `SortMap<T>` whitelist + `ToPagedResultAsync`. `GET /api/crm/customers/paged`. |
| WP3 | `ad624ff` | `lib/erp`: `parseApiError`, `useResource`/`usePagedQuery`/`useMutation`, paging + URL sync, status registry, formatters, permissions. The 403 interceptor no longer redirects to a missing route. |
| WP4 | `fd659c9` | `createBrowserRouter`. Routes split into 13 files. `ProtectedRoute` with per-route permission. `RouteErrorBoundary`. Template pages behind `VITE_SHOW_TEMPLATE_PAGES`. `/access-denied`. **Login redirect fix + open-redirect protection.** Unsaved-changes guard. |
| WP5 | `0b3b833` | `components/erp` kit: 24 files (tables, dialogs, forms, KPI/chart cards, detail pieces). |
| WP6 | `2ab3601` | `/dev/components` gallery (dev builds only), checked in headless Chrome in light mode. |
| WP7 | `a986f4d`, `fab2c6d` | CRM Customers pilot: list, form and the **new detail page**. Backend fixes found along the way (below). |

## 3. Defects found and fixed along the way

| Severity | Defect | Fix |
|----------|--------|-----|
| High (data) | Deleting a customer set `CustomerId = NULL` on its opportunities, quotes and sales orders (tracked Includes + EF `ClientSetNull`). | Delete is refused with 409 and a summary ("It has 1 opportunity, 2 quotes…"). The UI offers "mark inactive". Tested. |
| High (security) | 230 API actions returned raw exception messages (SQL and internals) to clients. | Central mapper; 5xx and framework messages are never sent. |
| High (security) | Login `?next=` accepted any URL, so a crafted link could redirect to another site. | Same-site paths only (`safeRedirectPath`), with tests for `//`, `/\`, and encoded variants. |
| Medium | After a session expiry, login redirected to a garbage path (`slice(6)` assumed `next` came first). | Proper query parsing. |
| Medium | `POST /crm/customers` returned an empty 200, so the UI couldn't learn the new id. Missing customers returned 204/200. | 201 `{id}`, 404 for missing. |
| Medium | Legacy `AuditLogs` and `SystemSettings` pages crashed on render (use before declaration). | Fixed. |
| Medium | Customer outstanding balance was editable in the form. | Read-only; it comes from invoices and payments. |
| Low | 403 sent users to `/access-denied`, which didn't exist. | Rendered in place; the route now exists. |

## 4. Walk-through still needed (Gate B)

I could not run these myself. Your API on :5252 is the pre-change build, and customer pages need a login.

1. Stop the API, then run `dotnet run --project ERP.Api` (the new build) and `npm run dev` in `frontend/`.
2. Sign in, open **CRM › Customers**:
   - [ ] Search, the status filter, More filters, sort by Credit limit, and page 2 all change the request in the Network tab (`/crm/customers/paged?...`). Reloading keeps the state.
   - [ ] **Add customer**: submit empty (field errors); enter a duplicate code (server error on the field or in the summary); save, which lands on the new detail page.
   - [ ] Detail tabs load (contacts, opportunities, quotes, orders, activities).
   - [ ] **Edit**, change a field, click Back: the "Leave without saving?" dialog appears.
   - [ ] **Delete** a customer that has a quote: the dialog explains why it can't be deleted and offers **Archive customer instead**. Delete a customer with no history: it's removed.
   - [ ] No GUIDs visible anywhere.
3. Open `/dev/components` (dev server only) and check keyboard use: Tab through the table, the row menu (Enter/arrows/Esc), the dialogs (focus starts on Cancel and returns to the trigger), and the filter drawer.

## 5. Deviations from the plan

- **Legacy layout components** (`components/entity/*`, `modules/crm/components/*`) are marked `@deprecated` rather than turned into thin re-exports. Their props differ from the kit's, and they're replaced page by page as each module migrates.
- **Dates** use the native `<input type="date">` instead of react-datepicker, because it's more accessible and needs no new code.
- **Route group paths** such as `/hrm` or `/projects` (group nodes with no page) are no longer registered. They used to render an empty layout; now they return 404 until each module gets a dashboard at `/<module>`.
- **Added dev dependencies** (test-only): `@testing-library/{react,dom,user-event,jest-dom}`, `jsdom`, `Microsoft.EntityFrameworkCore.{InMemory,Sqlite}`. **Declared** already-present `redux@5`, `@eslint/js` and `globals`.

## 6. Known gaps carried forward

| Gap | Where it's addressed |
|-----|----------------------|
| `HandleAsync<T>` has a class constraint, so any action returning a struct (`Guid`, `bool`) sends an empty body. Only Customers is fixed. | CP3 (CRM-wide), then each module |
| `ApplyTenantFilter` **fails open**: with no tenant id it returns every tenant's rows. | CP2 (security milestone) |
| Customer changes aren't audited. The History tab says so. | CP3 |
| No permission enforcement on the API. `usePermission` allows any signed-in user until the session carries permissions. | CP2 |
| Nine controllers still return a plain-string 400 ("ID mismatch"). The frontend handles it. | Per module |
| CRM and other forms still load every customer for dropdowns (`customerService.getAll`, deprecated). | Per module, by switching to `EntitySelect` |
| Model snapshot and DbContext disagree on the ReturnOrder→Customer delete rule (Cascade vs Restrict). | CP3, as part of schema reconciliation |
| The Apollo client is unused by any page. | CP12 cleanup |
| Template branding: Konrix splash logo in `index.html` and "Coderthemes" meta tags. | CP12 |
| 681 lint warnings in legacy code (mostly `any`). | Drop as modules migrate; CP12 target 0 |
| `frontend/yarn.lock` sits next to `package-lock.json`, and CI uses npm. | CP12 |
