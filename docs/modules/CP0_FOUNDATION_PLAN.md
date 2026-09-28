# Checkpoint 0 — Baseline and Foundation: Plan (Gate A)

> Status: **approved 2026-09-28 (recommended decisions D1–D6) and implemented**. See [CP0_FOUNDATION_STATUS.md](CP0_FOUNDATION_STATUS.md).
> Parent plan: [../erp-ui-program/PLAN.md](../erp-ui-program/PLAN.md) · Rules: [../erp-ui-program/MASTER_PROMPT.md](../erp-ui-program/MASTER_PROMPT.md)
> Written: 2026-09-28

Checkpoint 0 builds no module screens. It produces a green build, one error format, one paging pattern, and the shared `components/erp` + `lib/erp` kit that every later module uses. It ends by migrating **CRM Customers** end to end as proof.

---

## 1. Baseline measured today

| Check | Result | Cause |
|-------|--------|-------|
| `npx tsc --noEmit` (the first half of `npm run build`) | ❌ **1,018 errors** | See breakdown below. The CI frontend job (`npm run build`) fails for the same reason. |
| `npm run lint` | ❌ does not run | ESLint 9 is installed but there is no `eslint.config.*` (flat config) file |
| `npm test` (vitest) | ✅ 1 file, 1 test | Only `config.test.ts` |
| `dotnet build ERP.SaaS.sln` | ✅ compiles (0 `CS` errors, 370 warnings) | The 8 errors reported are file locks from the running `ERP.Api` process (PID 14064), not code errors |
| `dotnet test` | ✅ 9 passed | CRM line calculator, lead conversion, password hashing |

TypeScript errors by root cause (about 95% come from 5 causes):

| Count | Code | Root cause | Fix |
|-------|------|------------|-----|
| 706 | TS18046 `'x' is of type 'unknown'` | `useTableSort<T>()` is called without a type argument, so `sortRows` returns `unknown[]` | Make `sortRows` generic per call (`<R>(rows: R[]) => R[]`). **One file.** |
| 128 | TS2305 no exported member `XxxDto` from `../services` | Pages import DTO types from `services/index.ts`, which doesn't re-export `types` | Add `export type * from "../types"` to each services barrel |
| 86 | TS2322 `{ title; subName }` not assignable to `PageTitleProps` | Pages pass `subName`, but the component takes `name` | Accept `subName` as an alias in `PageBreadcrumb`, then replace it with `PageHeader` during module migrations |
| 40 | TS2345 `LayoutActionType`/`AuthActionType` not assignable to `UnknownAction` | Redux Toolkit 2 action typing | Type the action creators with `type` literals, or widen the dispatch helpers |
| 20 | TS2306 `is not a module` | Empty `index.ts` barrels in `modules/core/entities/*/{types,hooks,components,utils}` | Add `export {};` |
| ~38 | assorted | Real type bugs (tenant form resolver, gridjs `DataTableRecord`, customer form reset) | Fix individually |

Other findings that affect the foundation:

- **`.gitignore` is deleted** in the working tree. Running `git add -A` now would commit `frontend/node_modules/`, `ERP.Api/bin/` and `.vs/`.
- The 403 interceptor in `apiCore.ts` sets `window.location.href = "/access-denied"`, but **no `/access-denied` route exists**, so any 403 sends the user to a blank page.
- The app uses `<BrowserRouter>`. React Router's `useBlocker`, which the unsaved-changes guard needs, only works with a data router (`createBrowserRouter`).
- `PrivateRoute.tsx` is React Router v5 code (`render=` prop) and is effectively unused. Protection actually happens in `Routes.tsx › renderProtectedElement`.
- The browser tab title still says "Konrix - Responsive Tailwind Admin Dashboard".
- Backend validation errors are returned as raw FluentValidation `ValidationFailure[]` at **55 call sites in 21 controllers**. Nothing in the frontend reads `errors`, so the format can change safely.
- `ExceptionMiddleware` and `ControllerBase.HandleAsync` produce two different shapes (`{message, errorCode, statusCode}` vs `{message, errors}`). Neither maps 409 Conflict.
- `BaseQuery` and `PagedResult<T>` exist in `ERP.Shared`, but **no controller returns `PagedResult`**. Every list endpoint returns a full array.
- Scale of later migration: 44 list pages, 44 form pages, 72 `Swal.fire` calls in 20 files, 53 `confirm(` calls, inline status-badge colors in 52 files.

---

## 2. Decisions needed before I start

| # | Decision | Options | My recommendation |
|---|----------|---------|-------------------|
| D1 | **How to baseline the 280 uncommitted changes** | (a) I restore `.gitignore`, extend it to ignore `build-out*/`, `build-crm-check/`, `temp_batch_*.sql` and `.kilo/worktrees/`, commit everything else on `master` as "chore: baseline before ERP UI program", then create branch `feature/erp-ui-foundation`. (b) You commit it yourself first. | **(a)**, but only if you confirm those deletions (the `*ApiService.cs` files, CRM repository interfaces, completion-summary `.md` files) are intentional |
| D2 | Router | (a) Migrate `BrowserRouter` to `createBrowserRouter` + `RouterProvider` while splitting routes, which enables `useBlocker`. (b) Keep `BrowserRouter` and guard only on browser unload. | **(a)**. Route splitting touches every route anyway, and (b) can't catch in-app navigation. |
| D3 | Toasts and dialogs | (a) Toasts use a thin wrapper over the sweetalert2 `toast` mixin (already installed). Confirm/Delete dialogs use Headless UI `Dialog` (accessible, supports custom content). (b) Add a toast library. | **(a)**: no new dependencies |
| D4 | Template demo pages (`/apps/*`, `/pages/*`, `/ui/*`, `/forms/*`, `/tables/*`, `/charts`, the Konrix `/dashboard`) | (a) Load them only when `VITE_SHOW_TEMPLATE_PAGES=true`; delete in Checkpoint 12. (b) Delete now. | **(a)**: they're useful as a visual reference while building the kit |
| D5 | Lint strictness | (a) Add a flat config. New `components/erp`, `lib/erp` and migrated files lint at error level. Legacy files are downgraded to warnings with a tracked count, and `--max-warnings` is dropped until Checkpoint 12. (b) Fix every legacy lint issue now. | **(a)**: (b) could be hundreds of edits to code we'll rewrite anyway |
| D6 | Paged list endpoint convention | (a) New sibling route `GET /api/<module>/<entities>/paged` returning `PagedResult<T>`, leaving existing array endpoints unchanged. (b) Change the existing `GET` to return `PagedResult`. | **(a)**: nothing breaks, and old endpoints are removed per module once no caller uses them |

---

## 3. Work packages (each is one or more reviewable commits)

### WP0 — Green baseline (blocks everything else)
1. Handle D1 (restore `.gitignore`, then baseline commit and branch).
2. Fix the TypeScript errors by root cause (table above) until `npx tsc --noEmit` passes.
3. Add `frontend/eslint.config.js` (typescript-eslint, react-hooks, react-refresh) per D5.
4. Update CI: add `npm run lint` and `npm test` to the frontend job.
5. Stop tracking or clean nothing else. Record warning counts in this doc.

**Done when:** `npm run build`, `npm run lint`, `npm test`, `dotnet build`, `dotnet test` all pass locally, with the API stopped for the .NET build.

### WP1 — Backend error contract
- `ERP.Shared/DTOs/ApiError.cs`: `{ status, message, errorCode, errors?: Dictionary<string, string[]>, traceId }`. Field keys are camelCase to match the frontend DTOs.
- Add `ConflictException` (409) to `ERP.Shared/Exceptions`.
- In `ControllerBase`, add `ValidationFailed(ValidationResult)`, and make `HandleAsync` and `ExceptionMiddleware` both emit `ApiError`. Map 400/401/403/404/409/422/500.
- Replace all 55 `BadRequest(new { message = "Validation failed", errors = ... })` sites with `ValidationFailed(result)` (mechanical).
- ERP.Tests: middleware and `HandleAsync` mapping for each exception type; FluentValidation-to-field-dictionary conversion.

### WP2 — Backend paging pattern
- `ERP.Infrastructure/Extensions/QueryableExtensions.cs`: `ToPagedResultAsync(BaseQuery, IReadOnlyDictionary<string, Expression> sortMap)`. `sortBy` is **whitelisted** through `sortMap`, `PageSize` is capped at 200, and the default sort is stable.
- Pilot: `GET /api/crm/customers/paged?pageNumber&pageSize&searchTerm&sortBy&sortDescending&status&industry` → `PagedResult<CustomerDto>`, tenant-filtered through the existing `CrmRepositoryBase`.
- ERP.Tests: paging math, unknown `sortBy` rejected with 400, tenant isolation (tenant A never sees tenant B's rows).

### WP3 — `frontend/src/lib/erp/` (logic, no UI)
| File | Purpose |
|------|---------|
| `apiError.ts` | `parseApiError(err): { status, message, fieldErrors, code }`. Handles the new `ApiError`, the legacy `{Error}`/`{message}` shapes, raw FluentValidation arrays, and network errors |
| `http.ts` | `usePagedQuery(fetcher, query)`, `useResource(fetcher, id)`, `useMutation(fn)`: loading/error/data, abort on unmount, stale-response protection, `refetch` |
| `paging.ts` | `PagedQuery<F>` and `PagedResult<T>` TypeScript types mirroring `BaseQuery`/`PagedResult`; URL ↔ query sync so filters survive refresh and back navigation |
| `statusRegistry.ts` | One map from status to `{ label, tone, icon }` (tones as in the master prompt §5), with a module-override hook for domain labels. Unknown statuses render neutral and log a dev warning |
| `format.ts` | `formatMoney(amount, currency)`, `formatDate`, `formatDateTime`, `formatNumber`, `formatPercent` using `Intl` and the tenant settings, falling back to browser locale |
| `permissions.ts` | `usePermission(code)`, `useAnyPermission`, `<PermissionGuard>`. **Until Checkpoint 2 it reads `permissions` from the auth session if present and otherwise allows (logged in = allowed), which matches the backend's current behavior.** The API surface is final, so modules can adopt it now |
| `useUnsavedChangesGuard.ts` | `useBlocker` (after D2) plus `beforeunload` |

Also: remove the `/access-denied` hard redirect from `apiCore.ts`. A 403 now rejects with `status: 403`, and the page renders `PermissionDenied`. Add a real `/access-denied` route for route-level denials.

### WP4 — Routing restructure
- Split `routes/index.tsx` (1,964 lines) into `routes/modules/{core,erp-legacy,hrm,crm,pos,finance,inventory,procurement,manufacturing,projects}.routes.tsx` and `routes/template.routes.tsx` (behind the D4 flag).
- Move to `createBrowserRouter` (D2). Keep `SetupGuard`, `SessionGuard`, `RootRedirect` and the layout choice (`/modules` → Default, everything else → Vertical).
- Add a `RoutesProps.permission?: string` field, enforced once Checkpoint 2 lands.
- Delete the dead `PrivateRoute.tsx`.
- Every existing URL keeps working; checked with a route-table snapshot test.

### WP5 — `frontend/src/components/erp/` (UI kit)
| Group | Components |
|-------|------------|
| Page | `PageHeader` (title, description, breadcrumbs, actions; sets `<title>` to "Page · ERP" instead of Konrix), `PageActions` |
| Feedback | `StatusBadge`, `EmptyState` (variants: `empty`, `no-results`, `unavailable`, `error`, `forbidden`), `ErrorState` (+Retry), `Skeleton` (text/row/card/table/chart), `PermissionDenied`, `toast` (D3) |
| Dialogs | `ConfirmDialog`, `DeleteDialog` (names the record, states consequences, optional type-to-confirm, shows 409 dependency messages inline, offers Archive when `onArchive` is provided) on Headless UI `Dialog` |
| Data | `DataTable` (column config, server paging/sort, row selection, bulk-action bar, row actions menu, row click, sticky header, horizontal scroll, all state slots), `DataTableToolbar`, `SearchInput` (300 ms debounce), `FilterPanel` (Headless UI drawer, applied-filter chips), `Pagination` (page size 10/25/50/100, "1–25 of 312"), `ColumnSelector` (visibility saved per user in `localStorage`, guarded) |
| Dashboard | `KpiCard` (value, previous-period delta, trend arrow + text, optional ApexCharts sparkline, drill-down link, `unavailable` state), `ChartCard`, `WidgetBoundary` (React error boundary + per-widget retry), `AttentionList` |
| Forms | `FormCard`, `FormSection`, `FormField` (react-hook-form `Controller`-aware: label, required marker, help text, error, `aria-describedby`), `EntitySelect` (react-select async over any service `list()`, shows name/code, never GUID), `CurrencyInput`, `DateInput`, `DateRangePicker` (react-datepicker), `LineItemsTable` (qty × price − discount% + tax%, subtotal/discount/tax/total footer; math in a pure `lineTotals()` function), `FormFooter` (Cancel / Save, "Saving…" disabled state) |
| Detail | `DetailHeader` (title, code, `StatusBadge`, primary actions, more-menu), `DetailTabs` (tab state in `?tab=`), `FieldGrid`, `RelatedRecordsPanel`, `ActivityTimeline` |

Consolidation: `components/entity/*` and `modules/crm/components/*` become thin re-exports of the kit (marked `@deprecated`), so the 88 existing pages keep compiling until their module checkpoint migrates them. CRM's Dynamics look is kept as a **theme variant** (`data-erp-theme="crm"` + CSS variables from `modules/crm/theme`), not as `moduleName === "CRM"` branches inside components.

### WP6 — Component gallery
`/dev/components` (only when `import.meta.env.DEV`): every kit component in every state (loading, empty, no-results, unavailable, error, forbidden, disabled, filled, validation error), in light and dark themes.

### WP7 — Pilot: CRM Customers end to end
Proves the kit and the contracts against a real, wired API:
- `customerService.list(query)` → `/crm/customers/paged`; `get`, `create`, `update`, `remove`, with typed DTOs.
- `CustomerList` on `DataTable`: server search, status/industry filters, sort, paging, column selector, row click → detail, row menu (View, Edit, Delete), `DeleteDialog`, `StatusBadge`, `formatMoney` for credit limit.
- `CustomerForm` on `FormCard`/`FormSection`/`FormField`: server field errors shown against fields, unsaved-changes guard, toast, then navigate to detail.
- **New** `CustomerDetail` at `/crm/customers/:id`: `DetailHeader` + tabs Overview / Contacts / Opportunities / Quotes / Sales Orders. Related tabs reuse existing `by customer` endpoints where they exist; where they don't, the tab shows the `unavailable` state and is logged as a Checkpoint 3 gap.

### WP8 — Tests and docs
- Vitest + Testing Library (`@testing-library/react`, `@testing-library/user-event`, `jsdom` as **dev dependencies**, required to test components; justified under master prompt rule 7): `parseApiError` (every status plus legacy shapes), `statusRegistry`, `format`, `lineTotals`, `usePagedQuery` (race and abort), `DataTable` (sort/page/select/empty/error), `DeleteDialog` (keyboard, focus trap, 409 message), `FormField` a11y wiring.
- ERP.Tests for WP1 and WP2.
- `docs/modules/CP0_FOUNDATION_STATUS.md`: what shipped, lint warning count, known gaps. Update `docs/FRONTEND.md` for the kit and route layout.

---

## 4. Order and dependencies

```
WP0 ──► WP1 ──► WP2 ─────────────┐
   └──► WP3 ──► WP4 ──► WP5 ──► WP6 ──► WP7 ──► WP8
```
WP1/WP2 (backend) and WP3–WP6 (frontend) can proceed in parallel after WP0. WP7 needs both tracks.

## 5. Exit criteria (Gate B)

- [ ] All five build and test commands pass. CI frontend job runs build + lint + test.
- [ ] `/dev/components` shows every component in every state. Keyboard-only use works for DataTable (sort, select, row menu), FilterPanel, DeleteDialog and forms. Focus returns to the trigger when a dialog closes.
- [ ] Customers: list → search/filter/sort/page (server-side, verified in the Network tab) → create with a server validation error shown on the field → save → detail → edit → navigate away with unsaved changes (blocked) → delete with confirm → list refreshes. No GUID visible anywhere.
- [ ] Forcing a 403 (temporary test endpoint or devtools override) renders `PermissionDenied` in place, and the app stays usable.
- [ ] Every pre-existing URL still resolves (route snapshot test). Template pages are hidden unless the flag is set.
- [ ] Unit and backend tests from WP8 pass.

## 6. Out of scope for Checkpoint 0

Permission catalog and enforcement (Checkpoint 2), dashboards with real KPIs (per module), migrating the other 43 list pages, removing `pages/erp/*` duplicates (Checkpoint 1), and anything in `backend-nestjs/`.

## 7. Risks

| Risk | Mitigation |
|------|------------|
| Router migration breaks guards or layouts | Route snapshot test plus a manual walk of `/setup`, login, `/modules`, and one page per module before merging WP4 |
| Deprecated re-export shims hide visual regressions on legacy pages | Shims only map props; legacy pages keep their current markup until their module checkpoint |
| Paged endpoint sort accepts arbitrary columns | Whitelist map, with unknown keys returning 400 (tested) |
| A running API locks .NET build output | Stop the API before `dotnet build`, or build with `-o` to a temp folder when verifying |
