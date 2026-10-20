# ERP Growth Progress

Last updated: 2026-10-21  
Integration branch: `growth`  
Current WP: **1.2** (IN_PROGRESS)  

## Summary (written at the end)

_In progress. Phase 0 exit PASS. WP 1.1 + 1.6 DONE. WP 1.2 tenant fail-closed in progress on `wp/1.2-tenant-fail-closed`._

## Work packages

| WP | Title | Phase | Status | Depends | Commits | Evidence | Notes / Blocker + human action |
|----|-------|-------|--------|---------|---------|----------|--------------------------------|
| 0.1 | Inventory and finish WIP gap slices | 0 | DONE | — | `7e45bfa` | Inventory: schemas DI-wired; journal Include fix; report stubs throw; JournalService post/reverse return false; Wave2 P2P/O2C green | Follow-ups: 1.6 GL for finance gap posts; 3.1 implement/hide stub report codes |
| 0.2 | Documentation honesty pass | 0 | DONE | 0.1 | *(this WP)* | Aligned BUSINESS_MODULES DI, FEATURES, PRODUCT/BACKEND/REPORTS/CUSTOMER_DEPTH trackers, FRONTEND_NEW_COVERAGE | No “complete” claims for FinanceGap GL or all report codes |
| 0.3 | Baseline green | 0 | DONE | — | `7e45bfa` | `dotnet build` 0 err; `dotnet test` 553 passed; `frontend-new` `npm run build` ok (tsc -b + vite; no separate lint/typecheck scripts) | Initial baseline had 10 fails — fixed in 0.1 |
| 0.4 | Legacy frontend WIP disposition | 0 | DONE | 0.1 | — | Uncommitted `frontend/` MFG/Reports/CRM left out of commits | Discard for product path; `frontend-new` already covers MFG/reports |
| 1.1 | Kill stubs on public REST | 1 | DONE | 0.3 | *(wp/1.1)* | Wired 23 report query methods; FinanceGap post→GL | Overlaps 1.6 |
| 1.2 | Tenant fail-closed audit | 1 | IN_PROGRESS | 1.1 | | | Branch `wp/1.2-tenant-fail-closed` |
| 1.3 | Transition maps + FIX-5 / FIX-6 | 1 | TODO | 1.1 | | | |
| 1.4 | Single stock writer | 1 | TODO | 1.1 | | | |
| 1.5 | Settings toggles | 1 | TODO | 1.3 | | | |
| 1.6 | Finance gap post → real GL | 1 | DONE | 1.1 | *(with 1.1)* | Fund/credit/debit memo set `PostedJournalId` via AccountingService | |
| 1.7 | TaxTransactions write path | 1 | TODO | 1.6 | | | Schema exists; no Application writers yet |
| 2.1 | frontend-new resource coverage | 2 | TODO | 1.1 | | | |
| 2.2 | Permission-filtered shell | 2 | TODO | 2.1 | | | |
| 2.3 | Real dashboard KPIs | 2 | TODO | 2.1 | | | |
| 3.1 | Reports catalog honesty | 3 | TODO | 1.1 | | | Includes implementing/hiding NotSupported stub codes |
| 3.2 | Exports & schedule worker | 3 | TODO | 3.1 | | | |
| 3.3 | Document print pack | 3 | TODO | 3.1 | | | |
| 4.1 | O2C depth automated | 4 | TODO | 1.4, 2.1 | | | |
| 4.2 | P2P depth automated | 4 | TODO | 1.4, 2.1 | | | |
| 4.3 | MFG / Projects / HR smoke | 4 | TODO | 1.4 | | | |
| 4.4 | GL depth | 4 | TODO | 1.5 | | | |
| 5.1 | Local scratch full smoke | 5 | TODO | 4.1–4.4 | | | |
| 5.2 | Go-live evidence pack | 5 | TODO | 5.1 | | | |
| 5.3 | Staging apply + human sign-off | 5 | TODO | 5.2 | | | Likely BLOCKED for human |
| 6.1 | SaaS billing (optional) | 6 | TODO | 5.1 | | | Mark N/A if deferred |
| 6.2 | GraphQL parity (finished REST) | 6 | TODO | 5.1 | | | |

Status values: `TODO` · `IN_PROGRESS` · `DONE` · `BLOCKED` · `N/A`

## Phase gates

| Gate | Result | Date | Evidence |
|------|--------|------|----------|
| Phase 0 exit | PASS | 2026-10-21 | WIP inventoried + honesty fixes (`7e45bfa`); docs aligned (`dedb9b9`); baseline 553/553 + frontend-new build; follow-ups tracked as 1.6 / 3.1 |
| Phase 1 exit | — | | |
| Phase 2 exit | — | | |
| Phase 3 exit | — | | |
| Phase 4 exit | — | | |
| Phase 5 exit | — | | |
| Phase 6 exit | — | | |

## Regression probes (re-run on every verify)

- Cross-tenant read/write on a mutating API → must be 404/403 (fail-closed)
- Low-privilege user hitting admin/finance post → must be 403
- Posted SI/PI/Journal update/delete → must fail; void/reverse only
- Free `Status` on update DTO for transition-managed docs → ignored or 400
- Document creator as final approver → must be 403 (ERP-FIX-6)
- Stock movement bypassing Inventory service → must not exist / must fail review
- O2C happy path test → green (`Wave2O2C*`)
- P2P happy path test → green (`Wave2P2PGlueTests`)
- Journal entry totals with missing CoA masters → must still balance/post (`Wave3JournalTests`)
- Unimplemented report catalog codes → must not empty-succeed (`ModuleReportQueryService` throws)

_Add a probe for every security or integrity fix as WPs land._

