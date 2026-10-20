# Customer Depth Plan — Complete module map

> **Goal:** The full set of entities, fields, and behaviors customers expect — treated as **one complete product depth**, not phased waves.  
> **Scope:** This ERP only (no taxi, manpower/hostel, CMS, SMS, lending).  
> **Companions:** [PRODUCT_COMPLETION_PLAN.md](./PRODUCT_COMPLETION_PLAN.md) · [BACKEND_COMPLETION_PLAN.md](./BACKEND_COMPLETION_PLAN.md) · [REFERENCE_MODULE_COMPARISON.md](./REFERENCE_MODULE_COMPARISON.md).

**Implementation rule (every item):** Domain → schema SQL → repository → service (real) → FluentValidation → REST → GraphQL (after REST) → `frontend-new` → tenant fail-closed + permissions → integration test.

**Principles**

1. Enrich existing aggregates before adding entities; add an entity only when it needs its own lifecycle.
2. No public stub that returns empty success.
3. Shared DB + `TenantId` stays.
4. Packaging Sale is **in scope** for this complete map (trade customers ask for it). Omit only if you later cut it from the product.

---

## Target picture (whole ERP)

```text
ERP (customer-complete)
├── Core          masters, auth, company, geo, branch, terms, email/PDF infra
├── Admin         settings, logs (no CMS/SMS)
├── CRM           rich parties + pipeline + packaging enquiry/pre-cost + commercial SO
├── Sales         SI/CN/POS + Dispatch Advice + unpost + prints
├── Procurement   P2P + landed cost + 3-way match + vendor contract
├── SCM           stock + bins + kits + cycle count + reservation + brand
├── Finance       GL/AR/AP + opening + period close + cheque book + budget + expenses
├── Manufacturing BOM + routing + scrap + production release from SO
├── Projects      tasks + timesheet + milestone billing + project expense
├── HRM           HR lite + employee advance + team + expense link
└── Reports       catalog + schedules + PDF + per-report permissions
```

**Rough size when done:** existing ~135 entities + ~25 new masters/aggregates (+ their line tables).

---

## 1. Core / Platform

### Already have (keep)
Tenant, Company, Plan, Subscription, User, UserRole, UserSession, RefreshToken, Role, RolePermission, Permission, Setting, FiscalYear, DocumentSequence, AdminEmail, AuditLog

### Add — entities

| Entity | Purpose |
|--------|---------|
| `Country` | Code, name, active |
| `City` | FK Country; quick-add from party forms |
| `Branch` | Company site/branch; optional on warehouses & documents |
| `PaymentTerm` | Name, days, description; used on parties & trade docs |

### Add — platform capabilities (not new modules)

| Capability | Detail |
|------------|--------|
| Real SMTP / email provider | Password reset, invoice send, report schedule (replace log-only) |
| PDF print service | Shared templates for SO, SI, CN, PO, DA, vouchers |
| Fine-grained report permissions | Seed view permission per report code; hide unauthorized |

### APIs / UI
`/api/core/countries`, `/cities`, `/branches`, `/payment-terms` · Setup screens · City lookup on Customer/Vendor

### Complete when
Parties use City FK; PaymentTerm drives credit days; email and PDF work in staging.

---

## 2. Admin

### Already have
SystemSetting, SystemLog

### Add
All new feature toggles as settings keys (examples):

| Key | Purpose |
|-----|---------|
| `crm.requireSalesmanOnCustomer` | Require salesman on customer |
| `inventory.requireBin` | Require bin on stock moves |
| `procurement.threeWayMatch` + tolerance | Hard gate PI approve |
| `sales.allowUnpost` | Allow SI/CN unpost vs void-only |
| `finance.enforceBudget` | Warn/block over budget |
| `finance.requireOpenPeriod` | Block post into closed period |

### Out of scope
CMS, blogs, FAQ, SMS admin

### Complete when
Every depth feature is switchable via settings where noted; no CMS/SMS routes.

---

## 3. CRM

### Already have
Customer, CustomerGroup, Contact, Lead, Opportunity, Quote(+Line), SalesOrder(+Line), ReturnOrder(+Line), Activity, Campaign, PriceList, SalesTerritory, Salesman

### Enrich — Customer (and mirror Vendor in Procurement)

| Field / behavior |
|------------------|
| Local / Urdu name |
| Phone, Phone2, Cell, Cell2, Fax, ContactPerson |
| CompanyName, RegistryNo / SECP |
| Rating (1–5) |
| CityId (FK), keep address text |
| CreditDays, CreditLimit (already) + PaymentTermId |
| TaxType, NTN, STRN, CNIC (already) |
| SalesmanId — required when setting on |
| ReceivableAccountId (AR control CoA) |
| PortalUserId → Core.User (optional; no password on party) |
| Terms / remarks |

### Enrich — Quote / SalesOrder (commercial)

| Area | Fields |
|------|--------|
| Header | ReferenceNo, ReferenceDate, CreditDays, PaymentMode, PaymentTermId, ShipTo, Freight, Overhead, DiscountType/Amount |
| Lines | PackQty/PCS, Tax1/Tax2 (ensure UI + recalc), item remarks, manual description flag |
| Footer | Persist net after freight/overhead/discount; return on GET |

### Add — packaging Sale (full)

| Entity | Role |
|--------|------|
| `PouchType` | Setup master |
| `ProductionProcess` | Setup master |
| `FOHType` | Setup master (rate/basis) |
| `SaleEnquiry` + lines | Query; Draft → Approved / Rejected |
| `SalePreCost` + lines / cost breakdown | From approved enquiry; materials + process + FOH + margin → UnitRate; approve/reject |

**Line specification attrs** (shared on Enquiry, PreCost, Quote, SO, SI lines):  
Size, MaterialType, PET, MPET, LaminationMicron, CutOffLength, FolderSize, Ups, PouchTypeId, ProcessId, FOHTypeId

**Flow (complete):**  
Enquiry → approve → PreCost → approve → Quote → approve → SalesOrder → approve → Dispatch (Sales) → Invoice → Post

### Add — other
| Item | Detail |
|------|--------|
| Link PreCost → Quote | Rate locked from approved costing |
| SO line → production | Release flag or link to ProductionOrder; “pending for production” report |

### Complete when
Party form matches trade walkthrough; commercial SO totals correct; packaging pipeline works end-to-end.

---

## 4. Sales

### Already have
SalesInvoice(+Line), SalesCreditNote(+Line), POSSession, POSTransaction(+Line)

### Enrich

| Item | Detail |
|------|--------|
| SI commercial fields | Same header/footer pattern as SO (freight, overhead, payment mode, refs, Tax1/Tax2) |
| Unpost | Reopen to Draft when no payments / FY open and `sales.allowUnpost`; else void |
| General / cash sale | First-class UI path: ad-hoc SI without SO; optional immediate payment |
| Source links | SI from SO and/or DispatchAdvice; track invoiced qty |

### Add — entities

| Entity | Role |
|--------|------|
| `DispatchAdvice` + `DispatchAdviceLine` | Delivery challan; from Approved SO; qty ≤ open SO qty; Draft → Approved → Dispatched → Cancelled; stock issue on Dispatched via `IStockService` |

### Add — capabilities
PDF print for SO, SI, CN, DA · Batch post queue (optional multi-select)

### Complete when
SO → Dispatch → SI → Pay (partial dispatch OK); unpost policy documented and enforced; cash sale ≤ 3 screens; prints work.

---

## 5. Procurement

### Already have
Vendor, VendorBankAccount, PurchaseRequisition(+Detail), RequestForQuotation(+Detail), RFQVendorResponse(+Detail), PurchaseOrder(+Detail), GoodsReceiptNote(+Detail), PurchaseInvoice(+Detail), PurchaseReturn(+Line), PurchaseDebitNote(+Line)

### Enrich — Vendor
Mirror Customer party depth (phones, local name, CityId, PaymentTermId, PayableAccountId, rating, tax IDs).

### Add — entities

| Entity | Role |
|--------|------|
| `LandedCostVoucher` + lines | Allocate freight/duty/insurance onto GRN/PI lines; update stock unit cost |
| `VendorContract` | Vendor + item/price + validity + payment terms; PO defaults from active contract |

### Add — behaviors

| Behavior | Detail |
|----------|--------|
| Three-way match | PI approve blocked unless qty/price within tolerance vs PO + GRN; 409 with variances |
| Landed cost link | GRN/PI reference landed cost voucher |

### Complete when
Import path PO → GRN → Landed cost → PI → Pay; mismatch cannot approve; contract prices default on PO.

---

## 6. SCM (Inventory)

### Already have
ItemCategory, ItemGroup, UnitOfMeasure, InventoryValuationMethod, Item (+ attributes, variants, pricing, barcode, batch, serial, tracking), Warehouse, StockBalance, StockLedger, ReorderRule, StockTransfer(+Detail), StockAdjustment(+Line), InventoryAdjustment

### Enrich
| Item | Detail |
|------|--------|
| `Item.BrandId` | FK Brand |
| Stock lines | Optional `WarehouseLocationId` on receive/issue/transfer/adjust/GRN/DA |
| WAC | Average cost from ledger; honest report + optional admin recalc |

### Add — entities

| Entity | Role |
|--------|------|
| `Brand` | Item brand master |
| `WarehouseLocation` | Bin under warehouse |
| `StockReservation` (+ lines/rows) | Soft allocate on SO approve; release on cancel/ship/dispatch; Available = OnHand − Reserved |
| `ItemKit` + `ItemKitComponent` | Sellable parent → component qty; explode or deduct components on ship/POS |
| `CycleCount` + `CycleCountLine` | Draft → Counted → Posted; variance creates StockAdjustment |

### Complete when
Bin moves work when required; kits deduct components; reservations prevent oversell; cycle count posts adjustments; WAC matches ledger.

---

## 7. Finance

### Already have
ChartOfAccount, AccountDefault, CostCenter, Currency, ExchangeRate, TaxCode, TaxRate, TaxTransaction, Journal(+Entry), Invoice(+Line), Bill(+Line), Payment(+Application), Bank, BankAccount, BankTransaction, BankReconciliation, Budget, FixedAsset(+DepreciationEntry), IntercompanyTransaction (if present)

### Enrich
| Item | Detail |
|------|--------|
| Budget | Full CRUD + vs-actual; enforce warn/block via setting |
| Cheque on journal | Stay; also issue from cheque book leaves |
| Expense → GL | Approved ExpenseClaim posts expense JV or AP bill |

### Add — entities

| Entity | Role |
|--------|------|
| `OpeningBalance` (+ lines) | Per FY; CoA / party / optional stock; posts opening JV; lock after post |
| `AccountingPeriod` | Month (or period) inside FY; open/closed; block post when closed |
| `ChequeBook` + `ChequeLeaf` | Book ranges; Blank → Issued → Cleared / Bounced / Cancelled; no double-issue |
| `ExpenseClaim` + lines | Employee claim; Draft → Submitted → Approved → Paid; optional ProjectId |

### Complete when
New FY opening balances load; closed period rejects posts; cheque book + register consistent; budgets enforceable; expense claims hit GL.

---

## 8. Manufacturing

### Already have
WorkCenter, BillOfMaterial(+Line), ProductionOrder(+Line), ProductionRun

### Add — entities

| Entity | Role |
|--------|------|
| `Routing` + `RoutingStep` | Sequence, work center, std time; optional on ProductionOrder |
| `ProductionScrap` (or scrap lines on run) | Qty + reason; adjust issue/receive |

### Add — behaviors

| Behavior | Detail |
|----------|--------|
| Capacity warning | WorkCenter weekly hours; overload warn only |
| SO → production | Release approved SO lines → create/link ProductionOrder |
| Pending production report | SO lines not yet released |

### Complete when
Order completes with routing steps logged; scrap recorded; SO can release to production; pending report accurate.

---

## 9. Projects

### Already have
Project, ProjectTask, ProjectMilestone, ProjectResource, ProjectDocument

### Add — entities

| Entity | Role |
|--------|------|
| `ProjectTimesheet` + lines | Employee hours on project/task; submit/approve |

### Add — behaviors

| Behavior | Detail |
|----------|--------|
| Milestone billing | Milestone reached → draft SalesInvoice / Finance AR |
| Project expense | ExpenseClaim.ProjectId; show on project cost |

### Complete when
Time entry → approve; milestone can bill; expenses roll into project cost.

---

## 10. HRM

### Already have
Department, Employee(+Address, Document), EmploymentContract, LeaveType, LeaveBalance, LeaveRequest, Shift, TimeEntry, Payroll(+Line), PayrollComponent, Benefit, EmployeeBenefitEnrollment, Training(+Enrollment), PerformanceReview, JobOpening, Candidate, JobApplication, Interview, HRSettings

### Add — entities

| Entity | Role |
|--------|------|
| `EmployeeAdvance` | Disburse + recover (payroll deduction and/or payment) |
| `Team` | Optional group under manager |

### Link
ExpenseClaim (Finance) ↔ Employee; advances visible on employee balance

### Complete when
Advance balance + payroll recovery work; optional teams; expense claims usable from HR context.

---

## 11. Reports

### Already have
Report, ReportSchedule, ReportExecution + operational/finance catalog routes

### Add — catalog / behavior (mostly queries, not entities)

| Report / capability |
|---------------------|
| Sale order schedule by req. date |
| Pending SO for production |
| Broader sale delivery / invoice schedule |
| PDF for SO, SI, CN, PO, DA, vouchers |
| Per-report permission gating |
| Schedule worker delivers via real SMTP |

### Complete when
All catalog reports return real data; prints work; unauthorized reports hidden; schedules email successfully.

---

## Cross-module flows (must all work)

```text
O2C:  Lead/Enquiry → PreCost? → Quote → SO → Reserve stock → Dispatch → SI → AR → Payment
      ReturnOrder → CreditNote → AR/GL/stock
      Unpost SI when policy allows

P2P:  PR → RFQ → PO (contract price) → GRN → Landed cost → PI (3-way match) → AP → Payment
      PurchaseReturn → DebitNote → AP/GL/stock

Stock: GRN/DA/adjust/transfer/MFG/kit/cycle-count/reservation all via Inventory services only

GL:   Opening → Journals (FY + period) → TB/P&L/BS; ChequeBook on BPV; Budget; ExpenseClaim

MFG:  BOM + Routing → ProductionOrder/Run → scrap → stock; SO release → pending report

Projects: Timesheet + milestone bill + project expenses

HR:   Leave/attendance/payroll + advance recovery + expense
```

---

## Full new-entity list (whole product)

| Module | New entities |
|--------|----------------|
| Core | Country, City, Branch, PaymentTerm |
| CRM | PouchType, ProductionProcess, FOHType, SaleEnquiry(+lines), SalePreCost(+lines) |
| Sales | DispatchAdvice(+lines) |
| Procurement | LandedCostVoucher(+lines), VendorContract |
| SCM | Brand, WarehouseLocation, StockReservation, ItemKit(+components), CycleCount(+lines) |
| Finance | OpeningBalance(+lines), AccountingPeriod, ChequeBook, ChequeLeaf, ExpenseClaim(+lines) |
| Manufacturing | Routing(+steps), ProductionScrap |
| Projects | ProjectTimesheet(+lines) |
| HRM | EmployeeAdvance, Team |
| **Total** | **~25 aggregates/masters** (+ line tables) |

Plus field enrichments on Customer, Vendor, Quote, SalesOrder, SalesInvoice, Item, stock documents — no new module folders required.

---

## Definition of complete (whole)

1. Every section §1–§11 above is implemented (API + UI + tests).
2. Cross-module flows in this file all pass on a staging tenant.
3. No public stub for any listed entity or behavior.
4. Settings toggles exist where listed; defaults sane for SMB trade.
5. Taxi, manpower/hostel, lending, CMS, SMS remain **outside** this ERP.

---

## Tracker (module whole — check off when module depth is done)

- [x] Core (geo, branch, terms, SMTP, PDF)
- [x] Admin (settings keys only; no CMS/SMS)
- [x] CRM (party + commercial SO + packaging enquiry/pre-cost)
- [x] Sales (dispatch, unpost, general sale, prints)
- [x] Procurement (landed cost, 3-way match, vendor contract)
- [x] SCM (brand, bin, reservation, kit, cycle count, WAC)
- [x] Finance (opening, period, cheque book, budget, expense)
- [x] Manufacturing (routing, scrap, SO release)
- [x] Projects (timesheet, milestone bill, expense)
- [x] HRM (advance, team)
- [x] Reports (schedules, production pending, PDF, permissions)
- [ ] Cross-flows E2E on staging (apply SQL scripts; run smoke on staging tenant)
