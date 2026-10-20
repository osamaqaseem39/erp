# ERP Domain — Entity / Module Tree

Source of truth: `ERP.Domain/<Module>/`. Regenerated from live types (includes nested classes in shared files).

**Totals:** 11 modules · ~220+ entity types · root `BaseEntity`

```text
ERP.Domain
├── BaseEntity
│
├── Core/                          (20)  tenancy, auth, masters, geo
│   ├── Tenant
│   ├── Company
│   ├── Plan
│   ├── Subscription
│   ├── User
│   ├── UserRole
│   ├── UserSession
│   ├── RefreshToken
│   ├── Role
│   ├── RolePermission
│   ├── Permission
│   ├── Setting
│   ├── FiscalYear
│   ├── DocumentSequence
│   ├── AdminEmail
│   ├── AuditLog
│   ├── Country
│   ├── City
│   ├── Branch
│   └── PaymentTerm
│
├── Admin/                         (2)   ops / system
│   ├── SystemLog
│   └── SystemSetting
│
├── CRM/                           (23 + PackagingLineFields base)
│   ├── Parties
│   │   ├── Customer
│   │   ├── Contact
│   │   ├── CustomerGroup
│   │   ├── Salesman
│   │   ├── SalesTerritory
│   │   └── PriceList
│   ├── Pipeline
│   │   ├── Lead
│   │   ├── Opportunity
│   │   ├── Campaign
│   │   └── Activity
│   ├── Commercial
│   │   ├── Quote
│   │   ├── QuoteLine
│   │   ├── SalesOrder
│   │   ├── SalesOrderLine
│   │   ├── ReturnOrder
│   │   └── ReturnOrderLine
│   └── Packaging
│       ├── PouchType
│       ├── ProductionProcess
│       ├── FOHType
│       ├── PackagingLineFields          (shared base, not a table)
│       ├── SaleEnquiry
│       ├── SaleEnquiryLine
│       ├── SalePreCost
│       └── SalePreCostLine
│
├── Sales/                         (9)   SI / CN / POS / dispatch
│   ├── SalesInvoice
│   ├── SalesInvoiceLine
│   ├── SalesCreditNote
│   ├── SalesCreditNoteLine
│   ├── DispatchAdvice
│   ├── DispatchAdviceLine
│   ├── POSSession
│   ├── POSTransaction
│   └── POSTransactionLine
│
├── Procurement/                   (21)  P2P + landed cost
│   ├── Vendor
│   ├── VendorBankAccount
│   ├── VendorContract
│   ├── PurchaseRequisition
│   ├── PurchaseRequisitionDetail
│   ├── RequestForQuotation
│   ├── RFQDetail
│   ├── RFQVendorResponse
│   ├── RFQVendorResponseDetail
│   ├── PurchaseOrder
│   ├── PurchaseOrderDetail
│   ├── GoodsReceiptNote
│   ├── GoodsReceiptNoteDetail
│   ├── PurchaseInvoice
│   ├── PurchaseInvoiceDetail
│   ├── PurchaseReturn
│   ├── PurchaseReturnLine
│   ├── PurchaseDebitNote
│   ├── PurchaseDebitNoteLine
│   ├── LandedCostVoucher
│   └── LandedCostVoucherLine
│
├── SCM/                           (~34)  inventory / stock / WMS putaway+pick
│   ├── Catalog
│   │   ├── Item
│   │   ├── ItemCategory
│   │   ├── ItemGroup
│   │   ├── Brand
│   │   ├── UnitOfMeasure
│   │   ├── ItemAttribute
│   │   ├── ItemVariant
│   │   ├── ItemVariantAttribute
│   │   ├── ItemPricing
│   │   ├── Barcode
│   │   ├── ItemKit
│   │   └── ItemKitComponent
│   ├── Warehousing
│   │   ├── Warehouse
│   │   └── WarehouseLocation          (+ LocationType, Zone)
│   ├── Stock
│   │   ├── StockBalance               (+ WarehouseLocationId)
│   │   ├── StockLedger                (+ WarehouseLocationId)
│   │   ├── StockTransfer
│   │   ├── StockTransferDetail        (+ From/ToLocationId; two-step)
│   │   ├── StockAdjustment
│   │   ├── StockAdjustmentLine        (+ WarehouseLocationId)
│   │   ├── InventoryAdjustment
│   │   ├── StockReservation
│   │   ├── StockReservationLine
│   │   ├── CycleCount
│   │   ├── CycleCountLine
│   │   ├── PutawayTask                (+ PutawayTaskLine)  WMS inbound
│   │   ├── PickList                   (+ PickListLine)     WMS outbound
│   │   ├── ReplenishmentTask          (+ lines)           Bulk→Pick
│   │   ├── PackOrder                  (+ PackOrderLine)
│   │   ├── Shipment                   (+ ShipmentLine)
│   │   ├── ReorderRule
│   │   └── InventoryValuationMethod
│   └── Traceability
│       ├── BatchNumber
│       ├── SerialNumber
│       └── BatchSerialTracking
│
├── Finance/                       (~55 types)  GL / AR / AP / banks / tax / FA
│   ├── GL
│   │   ├── ChartOfAccount
│   │   ├── AccountDefault
│   │   ├── CostCenter
│   │   ├── Journal                  (+ ReversedFromJournalId)
│   │   ├── JournalEntry
│   │   ├── Budget
│   │   ├── BudgetLine
│   │   ├── OpeningBalance
│   │   ├── OpeningBalanceLine
│   │   ├── AccountingPeriod         (+ IsLocked / ClosedAt)
│   │   ├── RecurringJournal
│   │   └── RecurringJournalLine
│   ├── Currency
│   │   ├── Currency
│   │   ├── ExchangeRate
│   │   ├── FxRevaluationRun
│   │   └── FxRevaluationLine
│   ├── Banks
│   │   ├── Bank
│   │   ├── BankAccount
│   │   ├── BankTransaction
│   │   ├── BankReconciliation
│   │   ├── BankReconciliationLine
│   │   ├── FundTransfer
│   │   ├── ChequeBook
│   │   └── ChequeLeaf
│   ├── AR                           ← SalesInvoice posts via SourceSalesInvoiceId
│   │   ├── Invoice
│   │   ├── InvoiceLine
│   │   ├── CreditMemo
│   │   ├── CreditMemoLine
│   │   ├── CustomerAdvance
│   │   └── AdvanceApplication       (shared AR/AP apply rows)
│   ├── AP                           ← PurchaseInvoice posts via SourcePurchaseInvoiceId
│   │   ├── Bill
│   │   ├── BillLine
│   │   ├── DebitMemo
│   │   ├── DebitMemoLine
│   │   └── VendorAdvance
│   ├── Payment
│   │   ├── Payment
│   │   ├── PaymentApplication       (InvoiceId XOR BillId)
│   │   └── WithholdingTaxLine
│   ├── Tax
│   │   ├── TaxCode                  (+ TaxGroupId)
│   │   ├── TaxRate
│   │   ├── TaxGroup
│   │   ├── TaxGroupLine
│   │   └── TaxTransaction
│   ├── FixedAssets
│   │   ├── AssetCategory
│   │   ├── FixedAsset               (+ AssetCategoryId)
│   │   ├── DepreciationEntry
│   │   ├── AssetDisposal
│   │   └── AssetTransfer
│   └── Other
│       ├── ExpenseClaim
│       ├── ExpenseClaimLine
│       ├── IntercompanyTransaction  (+ SettlementId)
│       └── IntercompanySettlement
│   # Cross-links: Core.FiscalYear ↔ AccountingPeriod; Core.PaymentTerm → AR/AP due dates
│
├── Manufacturing/                 (16)  BOM / routing / production / QC
│   ├── Masters
│   │   ├── WorkCenter
│   │   ├── BillOfMaterial
│   │   ├── BillOfMaterialLine
│   │   ├── Routing                  (shared file also holds RoutingStep, ProductionScrap)
│   │   ├── RoutingStep
│   │   └── ScrapReason
│   ├── Execution
│   │   ├── ProductionOrder
│   │   ├── ProductionOrderLine
│   │   ├── ProductionOrderOperation
│   │   ├── ProductionRun
│   │   ├── ProductionScrap
│   │   ├── QualityCheck
│   │   └── QualityCheckLine
│   └── Cross-module (doc only)
│       ├── SCM: Item, Warehouse, stock issue/receive
│       ├── CRM/Sales: SalesOrderLine → release
│       └── CRM Packaging: ProductionProcess, FOHType (pre-cost)
│
├── Projects/                      (8)
│   ├── Project                      (+ CostCenterId?, CurrencyId?, BranchId?)
│   ├── ProjectMember                (team membership / role)
│   ├── ProjectTask
│   ├── ProjectMilestone             (+ Amount, IsBillable, invoice FKs)
│   ├── ProjectResource              (allocation hours / rate)
│   ├── ProjectDocument              (+ TaskId?, MilestoneId?)
│   ├── ProjectTimesheet
│   └── ProjectTimesheetLine         (+ IsBillable, HourlyRate)
│
├── HRM/                           (25)
│   ├── Org
│   │   ├── Department
│   │   ├── Team
│   │   ├── Employee
│   │   ├── EmployeeAddress
│   │   ├── EmployeeDocument
│   │   ├── EmploymentContract
│   │   ├── Shift
│   │   └── HRSettings
│   ├── Leave / time
│   │   ├── LeaveType
│   │   ├── LeaveRequest
│   │   ├── LeaveBalance
│   │   └── TimeEntry
│   ├── Payroll / benefits
│   │   ├── Payroll
│   │   ├── PayrollLine
│   │   ├── PayrollComponent
│   │   ├── Benefit
│   │   ├── EmployeeBenefitEnrollment
│   │   └── EmployeeAdvance
│   ├── Talent
│   │   ├── JobOpening
│   │   ├── Candidate
│   │   ├── JobApplication
│   │   └── Interview
│   └── Development
│       ├── Training
│       ├── TrainingEnrollment
│       └── PerformanceReview
│
└── Reports/                       (5)   platform metadata + UX prefs
    ├── Report
    ├── ReportExecution
    ├── ReportSchedule
    ├── ReportFavorite               (domain; FE localStorage until DI/EF wired)
    └── ReportSavedFilter            (domain; FE localStorage until DI/EF wired)
    ※ Catalog: ReportCatalog + /api/reports/generate whitelist.
      Ops: /api/reports/operational/* · Finance: /api/finance/reports/*
      Plan: docs/REPORTS_COMPLETION_PLAN.md
```

## Module ↔ API surface (quick map)

| Domain folder | Typical API / UI | Notes |
|---------------|------------------|--------|
| `Core` | `/api/setup`, `/api/core`, auth, tenants | Platform |
| `Admin` | `/api/admin` | Settings + system logs |
| `CRM` | `/api/crm` | Pipeline + packaging enquiry/pre-cost |
| `Sales` | `/api/sales`, `/api/pos` | SI/CN/POS/dispatch |
| `Procurement` | `/api/procurement` | PR→RFQ→PO→GRN→PI |
| `SCM` | `/api/inventory` | Items + stock |
| `Finance` | `/api/finance` | GL/AR/AP/banks |
| `Manufacturing` | `/api/manufacturing` | BOM + production |
| `Projects` | `/api/projects` | Tasks · milestones · members · resources · docs · timesheets · milestone billing · project-linked expenses |
| `HRM` | `/api/hrm` | Employees → payroll |
| `Reports` | `/api/reports` | Catalog + schedules |

## Finance gaps (planned → implemented)

See [operating/DATA_MODEL.md](./operating/DATA_MODEL.md) § Finance gap set. Live types above include BudgetLine, TaxGroup, AssetCategory/Disposal/Transfer, BankReconciliationLine, FundTransfer, Credit/DebitMemo, advances, WHT, RecurringJournal, FxRevaluation, IntercompanySettlement. Glue: `Invoice.SourceSalesInvoiceId`, `Bill.SourcePurchaseInvoiceId`, `PaymentApplication` Invoice XOR Bill.

## Related docs

- [BUSINESS_MODULES.md](./BUSINESS_MODULES.md) — module inventory vs controllers/UI
- [CUSTOMER_DEPTH_PLAN.md](./CUSTOMER_DEPTH_PLAN.md) — target depth map
- [REPORTS_COMPLETION_PLAN.md](./REPORTS_COMPLETION_PLAN.md) — catalog / UI / export / schedule gap-fill
- [REPORTS_STANDARD_PACK.md](./REPORTS_STANDARD_PACK.md) — default ERP report codes by module
- [operating/DATA_MODEL.md](./operating/DATA_MODEL.md) — gated schema change index
- [ACCOUNTS_MODULE_PLAN.md](./ACCOUNTS_MODULE_PLAN.md) — accounts voucher/report checklist
