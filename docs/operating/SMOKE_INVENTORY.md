# Smoke / regression inventory (post-v1)

Living index of automated checks. Staging manual checklist remains in [CUSTOMER_DEPTH_PLAN.md](../CUSTOMER_DEPTH_PLAN.md) tracker.

## In-memory service smokes (ERP.Tests)

| Test | Covers |
|------|--------|
| `Crm/Wave2O2CShipStockTests.cs` | SO ship → stock |
| `Sales/Wave2SalesInvoiceArGlueTests.cs` | SI → AR |
| `Procurement/Wave2P2PGlueTests.cs` | GRN → stock + PO qty |
| `CustomerDepth/CustomerDepthSmokeTests.cs` | Depth settings + PDF plumbing |
| `Sales/SalesInvoiceWorkflowTests.cs` | SI post/void/unpost paths |
| `Crm/SalesOrderWorkflowTests.cs` | SO lifecycle |
| `Crm/PackagingEnquiryWorkflowTests.cs` | Enquiry approve/reject + FIX-5/6 |
| `Crm/PackagingPreCostWorkflowTests.cs` | PreCost approve/reject + FIX-5/6 |
| `Finance/ChequeBookLeafTests.cs` | Issue / double-issue / cancel leaf |
| `Finance/BudgetLifecycleTests.cs` | Budget activate + enforceBudget block |
| `Finance/OpeningBalancePostTests.cs` | Create → Post → GL |
| `Reports/ReportScheduleWorkerTests.cs` | Schedule execute/advance |
| `Reports/ReportScheduleEmailTests.cs` | Recipients → `ITenantEmailSender` |

These are **not** HTTP `WebApplicationFactory` suites. Cross-tenant + FIX-5 coverage lives in module workflow/isolation tests under `ERP.Tests/**`.

## Frontend-new

| Test | Covers |
|------|--------|
| `frontend-new/src/__tests__/smoke.test.tsx` | JWT, session, registry keys (depth + budgets), typed print paths, Login/ForgotPassword mount |

## Staging (manual / W5)

O2C, P2P, packaging Enquiry→PreCost, SCM depth, leave+MFG, report schedule SMTP — see [STAGING_E2E_CHECKLIST.md](STAGING_E2E_CHECKLIST.md) and CUSTOMER_DEPTH tracker.
