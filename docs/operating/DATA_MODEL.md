# Data model index

**Rule:** describe new tables here (and in `ERP.Infrastructure/Database/Schemas/`) **before** Domain/EF/service code.

Authoritative runtime shape still lives in `ERP.Domain` + EF configurations + schema SQL. This file is the **gated index** for changes.

## Existing module coverage (summary)

| Area | Schema / location | Notes |
|------|-------------------|--------|
| Core / auth / tenancy | `Core.*` | Tenants, Users, Roles, Permissions, Settings, AuditLogs, Sessions |
| CRM | EF + CRM schema SQL | Customers, leads, quotes, sales orders, … |
| Sales / POS | EF | Sales invoices + lines, credit notes + lines, POS sessions/transactions |
| Finance | EF + Finance schema SQL | CoA, journals, AR/AP, banks, taxes |
| HRM | EF + HRM schema SQL | Employees, leave, payroll, … |
| Inventory / SCM | EF + Inventory schema SQL | Items, warehouses, stock ledger |
| Procurement | EF + Procurement schema SQL | Vendors, PO, PR, GRN, PI (+ details) |
| Manufacturing | EF + schema SQL | Work centers, BOM, routing, production orders/runs, scrap, QC, operations |
| Projects | EF | Projects, tasks, milestones, resources |
| Reports | EF | Definitions, executions, schedules |

See also [DATABASE.md](../DATABASE.md).

## Pending / proposed tables

| Date | Name | Module | Purpose | Status |
|------|------|--------|---------|--------|
| 2026-10-20 | `Core.AdminEmails` | Core / User module | Notification email directory (email + type); see [USER_MODULE_PLAN.md](../USER_MODULE_PLAN.md) U4 | **Implemented** (apply `20261020_UserModule_CompanyContact_AdminEmails.sql`) |
| 2026-10-20 | `Core.Companies` columns `ContactPerson`, `MobilePhone` | Core | Company profile parity with reference video | **Implemented** |
| 2026-10-20 | `Core.Users` columns `AddressLine1`, `City` | Core | User profile address (U6) | **Implemented** (apply `20261020_UserModule_UserProfile.sql`) |
| 2026-10-20 | `Core.Users.EmployeeId` (nullable) | Core / HRM | Optional link user → employee; no cascade | **Implemented** (apply `20261020_UserModule_UserEmployeeLink.sql`) |
| 2026-10-20 | `Finance.AccountDefaults` | Finance | Purpose → posting CoA (Cash, AR, AP, …) per tenant | **Implemented** (apply `20261020_AccountsModule_CoA_CPV.sql`) |
| 2026-10-20 | `Finance.Journals` header columns | Finance | CPV fields: ReferenceDate, ChequeNumber, PaidBy, ReceivedBy, CashOrBankAccountId, JournalNumber prefix | **Implemented** |
| 2026-10-20 | `Finance.Journals.Status` + `Approved` | Finance | Draft → Approved → Posted (Closed UX = Approved) | **Implemented** |
| 2026-10-20 | Finance settings keys (`finance.*`) | Core / Finance | CoA max depth, decimals 1–8, posting-leaf-only, require cost center | **Implemented** via SettingsCatalog |
| 2026-10-20 | `finance.voucher.lastCashOrBankAccountId` + `lastCostCenterId` | Core / Finance | Default Voucher last-used for typed CPV/CRV/BPV/BRV | **Implemented** via SettingsCatalog (no schema change) |
| 2026-10-20 | `Finance.Journals` Invoice No link | Finance | Header `InvoiceNumber` + optional `EntityReferenceTable`/`EntityReferenceId` (AR invoice / AP bill) | **Implemented** (apply `20261020_Finance_Journal_InvoiceReference.sql`) |
| 2026-10-20 | `Finance.BankTransactions.ReconciliationId` | Finance | Optional FK to open/closed bank statement recon | **Implemented** (apply `20261020_Finance_BankReconciliationId.sql`) |
| 2026-10-20 | `dbo.SalesCreditNotes` + `dbo.SalesCreditNoteLines` | Sales | CN against posted SI; post reduces Finance AR balance | **Implemented** (apply `20261020_Sales_CreditNote.sql`) |
| 2026-10-20 | `dbo.SalesCreditNotes.PostedJournalId` | Sales | GL journal on CN post / reverse on void | **Implemented** (apply `20261020_Sales_CreditNote_PostedJournal.sql`) |
| 2026-10-20 | `dbo.SalesCreditNotes.WarehouseId` + `StockReceived` | Sales | Optional stock restock on CN post | **Implemented** (apply `20261020_Sales_CreditNote_Stock.sql`) |
| 2026-10-20 | `ReturnOrders.CreditNoteId` semantics | CRM → Sales | On Complete: create/post `SalesCreditNotes` (requires `SalesInvoiceId`); id is Sales CN, not Finance invoice | **Implemented** (no schema change) |
| 2026-10-20 | `dbo.PurchaseDebitNotes` + `dbo.PurchaseDebitNoteLines` | Procurement | DN against approved PI; post reduces Finance AP bill balance | **Implemented** (apply `20261020_Procurement_PurchaseDebitNote.sql`) |
| 2026-10-20 | `dbo.PurchaseDebitNotes.PostedJournalId` | Procurement | GL journal on DN post / reverse on void (Dr AP / Cr PurchaseReturns) | **Implemented** (apply `20261020_Procurement_PurchaseDebitNote_PostedJournal.sql`) |
| 2026-10-20 | `dbo.PurchaseDebitNotes.WarehouseId` + `StockIssued` | Procurement | Optional stock issue on DN post / receive on void | **Implemented** (apply `20261020_Procurement_PurchaseDebitNote_Stock.sql`) |
| 2026-10-20 | `TaxCodes.AccountId` service surface | Finance | Tax setup → posting CoA; REST `/api/finance/tax-codes` | **Implemented** (existing table; no schema change) |
| 2026-10-20 | `HRM.Departments` | HRM | Org departments; `Employees.DepartmentId` FK (nullable) | **Implemented** (apply `20261020_HRM_Departments.sql`) |
| 2026-10-20 | `HRM.Payrolls` status workflow | HRM | Draft → Processed (lines generated) → Posted (immutable); soft-delete Draft only | **Implemented** (no schema change; existing Payroll / PayrollLine) |
| 2026-10-20 | Payroll simplified formula | HRM | Gross = BaseSalary (contract or run default) + Allowances; Tax = 10% of Gross; Net = Gross − Deductions − Tax | **Implemented** in `PayrollCrudService` |
| 2026-10-20 | `dbo.BillOfMaterials` + `dbo.BillOfMaterialLines` | Manufacturing | FG item BOM header + component lines (qty per 1 FG) | **Implemented** (apply `20261020_Manufacturing_BOM.sql`) |
| 2026-10-20 | `dbo.ProductionOrders.BomId` | Manufacturing | Optional BOM used on complete (issue components / receive FG) | **Implemented** (same SQL) |
| 2026-10-20 | RFQ workflow statuses | Procurement | Draft → Open → Awarded / Closed / Cancelled (existing `RequestForQuotations` / lines / vendor responses) | **Implemented** (no schema change) |
| 2026-10-20 | `HRM.Benefits` + `HRM.EmployeeBenefitEnrollments` | HRM | Benefit catalog + employee enroll/unenroll | **Implemented** (apply `20261020_HRM_Benefits.sql`) |
| 2026-10-20 | Training enroll/complete workflow | HRM | `Trainings` + `TrainingEnrollments`: Enrolled/Completed; capacity; status Scheduled→In Progress→Completed | **Implemented** (no schema change; existing tables) |
| 2026-10-20 | Performance review workflow | HRM | `PerformanceReviews`: Draft → Submitted → Approved → Completed; soft-delete Draft only | **Implemented** (no schema change; existing table) |
| 2026-10-20 | Recruitment workflows | HRM | JobOpening Open→Closed/Filled; Application Applied→Shortlisted→Interviewed→Hired/Rejected; Interview Scheduled→Completed/Cancelled | **Implemented** (no schema change; existing tables) |
| 2026-10-20 | `Core.Users` TOTP 2FA columns | Platform | TwoFactorEnabled/Secret/PendingSecret + login challenge hash/expiry | **Implemented** (apply `20261020_Auth_TwoFactorTotp.sql`) |
| 2026-10-20 | Customer depth tables | All modules | See [CUSTOMER_DEPTH_PLAN.md](../CUSTOMER_DEPTH_PLAN.md); SQL `20261020_CustomerDepth_*.sql` | **Implemented** (apply scripts) |
| 2026-10-20 | Finance gap set (glue + depth) | Finance | SI/PI FKs, PaymentApplication XOR, TaxTransaction EF, BudgetLine, TaxGroup, assets, recon lines, memos, advances, WHT, recurring, FX, IC settlement | **Implemented** (apply `20261020_Finance_Gap_Full.sql`) |

### Finance gap set — shapes

#### Glue / columns on existing tables

| Table / column | Type | Notes |
|----------------|------|--------|
| `Invoices.SourceSalesInvoiceId` | uniqueidentifier NULL | FK to Sales invoice; filtered unique where not null |
| `Invoices.Notes` | nvarchar(2000) NULL | optional; SI post prefers SourceSalesInvoiceId |
| `Bills.SourcePurchaseInvoiceId` | uniqueidentifier NULL | FK to Purchase invoice; filtered unique |
| `PaymentApplications.InvoiceId` | uniqueidentifier NULL | XOR with BillId |
| `PaymentApplications.BillId` | uniqueidentifier NULL | XOR with InvoiceId |
| `Journals.ReversedFromJournalId` | uniqueidentifier NULL | self-FK to original journal |
| `AccountingPeriods.IsLocked` | bit | soft lock |
| `AccountingPeriods.ClosedAt` / `ClosedBy` | datetime2 / uniqueidentifier NULL | period close audit |
| `TaxCodes.TaxGroupId` | uniqueidentifier NULL | optional default group |
| `FixedAssets.AssetCategoryId` | uniqueidentifier NULL | FK AssetCategories |
| `IntercompanyTransactions.SettlementId` | uniqueidentifier NULL | FK IntercompanySettlements |

#### New tables (`Finance` schema, BaseEntity shape: Id, TenantId, audit, IsActive)

| Table | Purpose |
|-------|---------|
| `BudgetLines` | Period amounts under Budget |
| `TaxGroups` / `TaxGroupLines` | Multi-tax composition |
| `TaxTransactions` | Persist orphan domain type |
| `AssetCategories` | Dep method/life + GL accounts |
| `AssetDisposals` / `AssetTransfers` | FA lifecycle |
| `BankReconciliationLines` | Explicit recon ticks |
| `FundTransfers` | Bank/cash transfer + PostedJournalId |
| `CreditMemos` / `CreditMemoLines` | Pure AR financial memo |
| `DebitMemos` / `DebitMemoLines` | Pure AP financial memo |
| `CustomerAdvances` / `VendorAdvances` | Trade advances |
| `AdvanceApplications` | Advance → Invoice/Bill |
| `WithholdingTaxLines` | WHT on Payment |
| `RecurringJournals` / `RecurringJournalLines` | JV templates |
| `FxRevaluationRuns` / `FxRevaluationLines` | FX reval |
| `IntercompanySettlements` | IC settlement hub |

### Customer depth — Core

| Table | Purpose | Status |
|-------|---------|--------|
| `Core.Countries` | Geo master | **Implemented** |
| `Core.Cities` | City FK Country | **Implemented** |
| `Core.Branches` | Company branch/site | **Implemented** |
| `Core.PaymentTerms` | Payment terms (days) | **Implemented** |

### Customer depth — CRM / Sales / Packaging

| Table | Purpose | Status |
|-------|---------|--------|
| `CRM.PouchTypes` / `ProductionProcesses` / `FOHTypes` | Packaging setup masters | Implemented |
| `CRM.SaleEnquiries` + lines | Packaging enquiry | Implemented |
| `CRM.SalePreCosts` + lines | Pre-costing | Implemented |
| Enrich Customer / Quote / SalesOrder columns | Party + commercial fields | Implemented |
| `dbo.DispatchAdvices` + lines | Delivery challan | Implemented |
| Enrich SalesInvoice columns | Commercial parity | Implemented |

### Customer depth — Procurement / SCM

| Table | Purpose | Status |
|-------|---------|--------|
| `Procurement.LandedCostVouchers` + lines | Landed cost | **Implemented** |
| `Procurement.VendorContracts` | Vendor price contracts | **Implemented** |
| Enrich Vendor columns | Party parity | **Implemented** |
| `Inventory.Brands` | Item brand | **Implemented** |
| `Inventory.WarehouseLocations` | Bins (+ LocationType, Zone) | **Implemented** |
| `Inventory.StockReservations` (+ rows) | Soft allocate | **Implemented** |
| `Inventory.ItemKits` + components | Retail kits | **Implemented** |
| `Inventory.CycleCounts` + lines | Cycle count | **Implemented** |
| `StockBalances` / `StockLedgers`.`WarehouseLocationId` | Bin-level qty | **Implemented** (`20261022_Wms_BinStock.sql`) |
| `Inventory.PutawayTasks` + lines | GRN → bin putaway | **Implemented** (same) |
| `Inventory.PickLists` + lines | Dispatch pick from bins | **Implemented** (same) |
| `StockTransferDetails` From/ToLocationId | Two-step bin transfer | **Implemented** (`20261022_Wms_Phase2.sql`) |
| `StockAdjustmentLines.WarehouseLocationId` | Bin adjustments | **Implemented** (same) |
| `Inventory.ReplenishmentTasks` + lines | Bulk→Pick | **Implemented** (same) |
| `Inventory.PackOrders` + lines | Pack after pick | **Implemented** (same) |
| `Inventory.Shipments` + lines | Carrier / tracking | **Implemented** (same) |

### Customer depth — Finance / MFG / Projects / HRM

| Table | Purpose | Status |
|-------|---------|--------|
| `Finance.OpeningBalances` + lines | FY opening | **Implemented** |
| `Finance.AccountingPeriods` | Period close | **Implemented** |
| `Finance.ChequeBooks` + `ChequeLeaves` | Cheque stock | **Implemented** |
| `Finance.ExpenseClaims` + lines | Expense claims | **Implemented** |
| `Manufacturing.Routings` + steps | Routing | **Implemented** |
| `Manufacturing.ProductionScraps` | Scrap/QC | **Implemented** |
| `ProductionOrders` SO FKs + WarehouseId | SO→MO link + default WH | **Implemented** (`20261021_Manufacturing_Gaps.sql`) |
| `WorkCenters.WeeklyAvailableHours` | Capacity warn | **Implemented** (same) |
| `BillOfMaterials` IsDefault + effective dates | Default BOM pick | **Implemented** (same) |
| `BillOfMaterialLines.AlternateGroup` | Component substitutes | **Implemented** (same) |
| `Routings.IsDefault` | Default routing pick | **Implemented** (same) |
| `Manufacturing.ScrapReasons` | Scrap reason master | **Implemented** (same) |
| `ProductionScraps.ScrapReasonId` + WarehouseId | Typed scrap + stock | **Implemented** (same) |
| `Manufacturing.ProductionOrderOperations` | Routing step actuals | **Implemented** (same) |
| `ProductionRuns` WorkCenterId + OperationId | Run ↔ op/WC | **Implemented** (same) |
| `Manufacturing.QualityChecks` + lines | QC gate before FG receive | **Implemented** (same) |
| `Projects.ProjectTimesheets` + lines | Timesheets | **Implemented** |
| `Projects.ProjectMembers` | Project team membership | **Implemented** |
| `Projects` entity depth (milestone billing, billable lines, finance dims, invoice ProjectId) | PSA depth | **Implemented** |
| `HRM.EmployeeAdvances` | Employee advance | **Implemented** |
| `HRM.Teams` | Team master | **Implemented** |

### `Projects.ProjectMembers` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| ProjectId | uniqueidentifier | FK project |
| EmployeeId | uniqueidentifier | FK employee |
| Role | nvarchar(100) | e.g. Member, Lead, Contributor |
| StartDate | date | nullable |
| EndDate | date | nullable |
| Status | nvarchar(30) | Active / Inactive |
| Notes | nvarchar(1000) | nullable |
| audit | | standard |

### Projects entity depth — column alters (implemented)

| Table | Columns | Notes |
|-------|---------|--------|
| `dbo.Projects` | CostCenterId?, CurrencyId?, BranchId? | Finance dimensions |
| `dbo.ProjectMilestones` | Amount, IsBillable, SalesInvoiceId?, FinanceInvoiceId?, BilledAt? | Durable milestone billing |
| `Projects.ProjectTimesheetLines` | IsBillable (default 1), HourlyRate | Labor cost / billing |
| `dbo.ProjectDocuments` | ProjectTaskId?, ProjectMilestoneId? | Optional scoping |
| `dbo.SalesInvoices` | ProjectId?, ProjectMilestoneId? | Reverse project P&L |
| `dbo.Invoices` | ProjectId?, ProjectMilestoneId? | Finance AR link |
| `dbo.TimeEntries` | ProjectTaskId? | Explicit (was shadow FK) |

**ActualCost rollup rule:** approved timesheet lines (`IsBillable` × `Hours` × `HourlyRate`) + approved `ExpenseClaims` for the project + posted `JournalEntry` debits with `ProjectId`.

### `HRM.Benefits` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| Name | nvarchar(200) | |
| Description | nvarchar(1000) | nullable |
| Type | nvarchar(50) | e.g. Health, Dental, Retirement |
| Cost | decimal(18,2) | nullable employer cost |
| IsActive | bit | soft-delete |
| audit | | standard |

### `HRM.EmployeeBenefitEnrollments` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | |
| BenefitId | uniqueidentifier | FK Benefits |
| EmployeeId | uniqueidentifier | FK Employees |
| EnrollmentDate | datetime2 | |
| UnenrollmentDate | datetime2 | nullable |
| Status | nvarchar(30) | Active / Ended |
| Reason | nvarchar(500) | nullable (unenroll) |
| audit | | standard |
| UX | unique (TenantId, BenefitId, EmployeeId) where Status=Active | one active enrollment |

### `dbo.BillOfMaterials` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| BomNumber | nvarchar(50) | business number |
| Name | nvarchar(200) | |
| FinishedGoodItemId | uniqueidentifier | FG Item Id |
| Version | nvarchar(20) | default `1` |
| Status | nvarchar(30) | Draft → Active → Obsolete |
| Notes | nvarchar(1000) | nullable |
| audit | | standard BaseEntity |

### `dbo.BillOfMaterialLines` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | |
| BillOfMaterialId | uniqueidentifier | FK cascade |
| LineNumber | int | |
| ComponentItemId | uniqueidentifier | component Item Id |
| QuantityPer | decimal(18,4) | qty per 1 FG |
| ScrapPercent | decimal(9,4) | default 0 |
| Notes | nvarchar(500) | nullable |
| audit | | standard |

### `Sales.CreditNotes` / `dbo.SalesCreditNotes` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| CreditNoteNumber | nvarchar(50) | business number |
| CreditNoteDate | datetime2 | UTC date |
| CustomerId | uniqueidentifier | required |
| OriginalSalesInvoiceId | uniqueidentifier | required; SI must be Posted at post time |
| ReturnOrderId | uniqueidentifier | nullable |
| Status | nvarchar(50) | Draft → Posted → Voided |
| Reason | nvarchar(500) | nullable |
| SubTotal / TaxAmount / TotalAmount | decimal(18,2) | from lines |
| Notes | nvarchar(max) | nullable |
| PostedAt / PostedBy | datetime2 / uniqueidentifier | nullable |
| AppliedFinanceInvoiceId | uniqueidentifier | AR invoice credit applied to; idempotency |
| PostedJournalId | uniqueidentifier | nullable; GL JV on post (Dr SalesReturns / Cr AR); apply `20261020_Sales_CreditNote_PostedJournal.sql` |
| WarehouseId | uniqueidentifier | nullable; when set, post receives stock / void issues |
| StockReceived | bit | true after successful receive on post |
| CreatedAt / UpdatedAt / CreatedBy / UpdatedBy / IsActive | audit | standard |

### `dbo.SalesCreditNoteLines` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| CreditNoteId | uniqueidentifier | FK cascade |
| LineNumber | int | |
| OriginalSalesInvoiceLineId | uniqueidentifier | nullable |
| ItemId | uniqueidentifier | nullable |
| Description | nvarchar(500) | |
| Quantity / UnitPrice / TaxRate / LineTotal | decimal | money 18,2; qty 18,4 |
| TenantId + audit | | standard |

### `dbo.PurchaseDebitNotes` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| DebitNoteNumber | nvarchar(50) | business number |
| DebitNoteDate | datetime2 | UTC date |
| VendorId | uniqueidentifier | required |
| OriginalPurchaseInvoiceId | uniqueidentifier | required; PI must be Approved/Paid at post |
| PurchaseReturnId | uniqueidentifier | nullable |
| Status | nvarchar(50) | Draft → Posted → Voided |
| SubTotal / TaxAmount / TotalAmount | decimal(18,2) | from lines |
| AppliedFinanceBillId | uniqueidentifier | AP bill credit applied to |
| PostedJournalId | uniqueidentifier | nullable; GL JV on post (Dr AP / Cr PurchaseReturns); apply `20261020_Procurement_PurchaseDebitNote_PostedJournal.sql` |
| WarehouseId | uniqueidentifier | nullable; when set, post issues stock / void receives |
| StockIssued | bit | true after successful issue on post |
| audit | | standard |

### `dbo.PurchaseDebitNoteLines` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| DebitNoteId | uniqueidentifier | FK cascade |
| LineNumber | int | |
| OriginalPurchaseInvoiceLineId / ItemId | uniqueidentifier | nullable |
| Description / Quantity / UnitPrice / TaxRate / LineTotal | | same as sales CN lines |
| TenantId + audit | | standard |

### `Finance.AccountDefaults` (shape)

| Column | Type | Notes |
|--------|------|--------|
| Id / AccountDefaultId | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| Purpose | nvarchar(50) | e.g. Cash, BankClearing, AccountsReceivable, AccountsPayable, SalesRevenue, CostOfGoodsSold, Inventory, InputTax, OutputTax, RetainedEarnings |
| AccountId | uniqueidentifier | FK ChartOfAccounts; must be posting-allowed |
| IsActive | bit | |
| CreatedAt / UpdatedAt | datetime2 UTC | |
| CreatedBy / UpdatedBy | as project standard | |
| Unique | (TenantId, Purpose) | one mapping per purpose per tenant |

### Journal voucher transition map (Accounts)

```text
Draft ──approve──► Approved (= Closed list UX) ──post──► Posted
  ▲                  │                              │
  │                  reject                         ├──unpost──► Draft
  └──────────────────┘                              └──reverse─► Reversed
```

Approver ≠ creator when policy on. Status only via transition endpoints (not free-form update).

### Journal header extensions (CPV / typed vouchers)

| Column | Type | Notes |
|--------|------|--------|
| JournalNumber | nvarchar(50) | already exists; prefix CPV/CRV/BPV/BRV/JV |
| ReferenceDate | datetime2 | nullable |
| ChequeNumber | nvarchar(50) | nullable |
| ChequeStatus | nvarchar(30) | nullable; Outstanding / Cleared / Bounced / Cancelled |
| ChequeStatusAt | datetime2 | nullable |
| ChequeStatusNotes | nvarchar(500) | nullable |
| PaidBy | nvarchar(200) | nullable |
| ReceivedBy | nvarchar(200) | nullable |
| CashOrBankAccountId | uniqueidentifier | nullable FK CoA; header cash/bank for typed vouchers |
| InvoiceNumber | nvarchar(50) | nullable; free-text invoice/bill no on voucher form |
| EntityReferenceTable | nvarchar(100) | nullable; e.g. `Invoices` / `Bills` when linked |
| EntityReferenceId | uniqueidentifier | nullable; linked AR invoice or AP bill id |
| ApprovedAt | datetime2 | nullable |
| ApprovedBy | uniqueidentifier | nullable |

### Finance setup settings (`SettingsCatalog` category `finance`)

| Key | Type | Default | Notes |
|-----|------|---------|-------|
| `finance.coa.maxDepth` | Number 1–8 | 4 | Max CoA hierarchy depth |
| `finance.coa.valueDecimalPlaces` | Number 1–8 | 4 | Amount display/validation decimals |
| `finance.coa.postingLeafOnly` | Boolean | true | Only leaf accounts may post |
| `finance.coa.requireCostCenterOnExpense` | Boolean | false | Require cost center on expense Dr lines |
| `finance.voucher.allowPostWithoutApprove` | Boolean | true | When true, Post accepts Draft or Approved; when false, Approved only |

### `Core.AdminEmails` (proposed shape)

| Column | Type | Notes |
|--------|------|--------|
| Id | uniqueidentifier | PK |
| TenantId | uniqueidentifier | required |
| Email | nvarchar(200) | required; unique per (TenantId, Email) filtered active |
| Type | nvarchar(50) | e.g. Notification, Finance, Support |
| IsActive | bit | |
| CreatedAt / UpdatedAt | datetime2 UTC | |
| CreatedBy / UpdatedBy | nvarchar / guid as project standard | |

### Permission naming (User module U0)

`{area}.{resource}.{action}` — actions: `view` | `create` | `update` | `delete` | domain verbs (`approve`, …).  
Reference UI: View→`view`, Save→`create`, Edit→`update`, Delete→`delete`.

### Built-in roles (User Type seed — Role-as-type)

System: `Administrator`, `Read-only`.  
Templates (editable): `Sales`, `Finance`, `Accountant`, `HR`, `Operations`, `Warehouse`, `Production`.  
(`Accountant` ≈ finance module set; `Warehouse` ≈ inventory; `Production` ≈ manufacturing.)

## Change template

When adding a row above, include:

- PK (`uniqueidentifier`), `TenantId`, audit columns
- Business number / secondary keys and uniqueness scope
- Money columns as `decimal(18,4)` (or project standard precision)
- UTC datetime columns
- Insert-only / immutability triggers or service rules if posted
- Filtered unique indexes for “one active” rules
