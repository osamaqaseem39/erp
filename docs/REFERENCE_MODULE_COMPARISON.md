# All-modules functional comparison

> **Lens:** capabilities / workflows / data rules only — **not** UI.  
> **References:** `reference/POS` (complete trade/GL code) · `reference/BlocksERP` (**copy in progress** — see status) · **packaging Sale walkthrough** (Setup / Customer / Sale Query / CSO screens — aligns with BlocksERP `clsInvoice` / `clsAccountDetail` fields).  
> **Ours:** `ERP.Domain` + API modules.  
> Finance depth: [ACCOUNTS_MODULE_PLAN.md](./ACCOUNTS_MODULE_PLAN.md).  
> **Post-v1 note (2026-10-20):** Domain/API coverage matches [ENTITY_TREE.md](./ENTITY_TREE.md) (~180 entities) including packaging Enquiry/PreCost, Dispatch Advice, SI Unpost (setting-gated), cheque books, pending-for-production report, and customer-depth masters. Remaining gaps are mostly UI/`frontend-new` registry wiring, PDF layout polish, schedule email delivery, and staging E2E — not missing Domain entities. Lending remains ❌ / out of scope.

---

## BlocksERP copy status (re-checked)

| On disk now | Not copied yet (menus/`cls*` already hint at them) |
|-------------|-----------------------------------------------------|
| `Areas/Loan`, `Areas/Users` | `Areas` for Accounts, CashPayment, BankPayment, ChequeBook, Inventory, Sale, Purchase, Production, … |
| Root controllers: Account, Home, Documents, MenuRights, Combolist | Voucher / CoA / Item / Invoice controllers |
| `Models/cls*` including CoA, CostCenter, DefaultGl, Cashbank, ChequeBook, Warehouse, Invoice, Brand, Salesman, Unit, … | Matching Area implementations |
| `Views/Home` AsideMenu + MainMenu (full ERP nav) | Controllers those URLs call |
| Loan-centric `tbl*` EDMX entities | Likely more tables when remaining Areas arrive |

**Declared BlocksERP modules (from AsideMenu — functionality checklist):**  
Account Setup · Vouchers (CP/CR/BP/BR/JV + recheck/approve/post) · Cheque lifecycle · Inventory · Purchase (PO/GRN/PI/return/DN) · Production (formulation, PN) · Sale · Loan · Company / Warehouse / Region.

Until those Areas finish copying, **POS remains the implemented code reference** for trade/GL; BlocksERP Loan + menus + **Models** update the checklist.

---

## BlocksERP Models inventory → our modules

Source: `reference/BlocksERP/Models/cls*.cs` + root `tbl*.cs` (EDMX). Controllers for many of these are not copied yet; models still define **intended functionality**.

### A. Finance / GL

| BlocksERP model | Key capabilities (fields) | Our equivalent | Gap |
|-----------------|---------------------------|----------------|-----|
| `clsChartofAccount` / `tblChartofAccount` | `AccountLevel`, parent, display code, section, status; embeds ledger/voucher lists | `ChartOfAccount` | ✅ Computed `Level` + L1–L4 UX; section/report type still thin |
| `clsAccountDetail` / `tblAccountDetail` | Party leaf: credit days/limit, GST/NTN/CNIC, phones, Urdu name, rating, salesman, portal user/pass, terms, city, CoA parent | `Customer` / `Vendor` (+ AR/AP control) | Credit **days**; split tax IDs; salesman; multi-phone; CoA leaf link (see §4a) |
| `clsDefaultGl` / `tblAccountDefault` + `tblAccountDefaultGL` | `Type` → account/detail | `AccountDefault` | ✅ Purpose → posting CoA (API + UI) |
| `clsCostCenter` | Id, name, remarks, status | `CostCenter` | ✅ Service/API/UI CRUD |
| `clsCashbank` | **Voucher hub**: type, no, dates, ref date, Dr/Cr lines, tax, cheque, paid/received by, default voucher, cost center, project, FY, branch, multi-currency, status + approve/check/recheck/post roles | `Journal` + `JournalEntry` + `Payment` | ✅ CPV/CRV/BPV/BRV + approve→closed→post→unpost; FY enforce ✅; recheck role optional |
| `clsChequeBook` | Book + cheque lifecycle roles (deposit/check/approve/clear/bounce/return + voucher) | — | ✅ thin register on vouchers (Outstanding/Cleared/Bounced/Cancelled); no cheque-book stock |
| `clsTaxSetup` | Tax name/rate/type → detail account | `TaxCode` (+ GL) / `TaxRate` | ✅ TaxCode → posting CoA via `/api/finance/tax-codes` |
| `clsBank` / `clsCompanyBankAccount` / `clsUserBankAccount` / `tblBank*` | Bank + company/user bank accounts | `Bank` / `BankAccount` | Company-bank for lending OK later |
| `clsBusiness` / `clsCompany` | Multi-company | `Company` / `Tenant` | — |

### B. Sales / Purchase / Inventory (via `clsInvoice` + masters)

| BlocksERP model | Key capabilities | Our equivalent | Gap |
|-----------------|------------------|----------------|-----|
| `clsInvoice` (mega DTO) | `Type` drives PO/GRN/PI/PRN/DN/SO/SI/…; lines with item/qty/warehouse; discount/freight/**Tax1+Tax2**; credit days; salesman; transporter/bilty; **packaging** (PouchType, PET/MPET, lamination, FOHType, Process); margin/pre-cost fields; production tags; approve/cancel | Split: `PurchaseOrder`, `GoodsReceiptNote`, `PurchaseInvoice`, `SalesOrder`, `SalesInvoice`, … | Returns/DN/CN; bilty; **pre-cost / dual tax / packaging line attrs**; production link |
| `clsBrand` / `clsCategory` / `clsSubCategory` / `clsUnit` | Item classification + UoM | `ItemCategory`, `UnitOfMeasure` | Brand / sub-category |
| `clsWarehouse` | Warehouse master + contacts | `Warehouse` | Near parity |
| `clsSalesman` / `clsBDEmployee` | Sales force | — / CRM thin | **Salesman master** (walkthrough Setup) |
| Pouch Type / Production Process / FOH Type | Sale Setup masters (walkthrough; IDs on `clsInvoice`) | — | **Missing packaging masters** |
| `clsTerms` | Terms of sale | ⚠️ on docs | Optional |

**No dedicated `clsItem` yet** in BlocksERP Models folder — item fields live on `clsInvoice` lines; item Area likely still copying.

### C. Lending (models + `tbl*` — code Areas present)

| BlocksERP model / table | Capability | Ours |
|-------------------------|------------|------|
| `clsLoanApplication` / `tblLoanApplication` (+ Detail) | Amount, days, fees, purpose, category, investor, bank, status, approval | ❌ |
| `tblLoanReturn` / Detail | Repayment allocation | ❌ |
| `clsPersonalInfo` / `tblPersonalInfo` + Work/Education/… | Borrower KYC | ❌ |
| `clsDocument*` / `tblDocument*` | KYC docs + status | ❌ |
| `clsLoanCategory` / `clsLoanPurpose` / referral / income / employment | Masters | ❌ |

### D. Platform / geo / misc models

| BlocksERP | Ours | Notes |
|-----------|------|-------|
| `clsUser`, `clsUserType`, `clsModule` | User, Role, Permission | Parity intent |
| `clsCountry`, `clsCity`, `clsArea`, `clsTown`, `clsZone` | ⚠️ reference data | Richer geo in Blocks |
| `clsShift`, `clsJobType`, `clsTeam`, `clsCheckList`, `clsWork` | HRM / ops fragments | Partial overlap |
| `clsAdminEmail` | ⚠️ email settings | |
| `clsSqlErrorLog` | SystemLog | |

### E. Model-derived Finance field checklist (vs our `Journal`)

From `clsCashbank` — treat as required **functional** fields for typed vouchers:

| Field / behavior | Ours today |
|------------------|------------|
| VoucherType + VoucherNo | ⚠️ JournalType / JournalNumber |
| VoucherDate, ReferenceDate, ReferenceNo | ✅ date + ReferenceDate + ReferenceNo |
| Header Cash/Bank DetailCode | ✅ `CashOrBankAccountId` (typed CPV/CRV/BPV/BRV) |
| Invoice No (voucher↔AR/AP) | ✅ `InvoiceNumber` + header `EntityReferenceTable`/`EntityReferenceId` |
| Line Dr/Cr + net after tax | ✅ Debit/Credit; tax on voucher ⚠️ |
| ChequeNo, PaidBy, RecievedBy | ✅ ChequeNumber + PaidBy + ReceivedBy |
| DefaultVoucher | ✅ last-used via `finance.voucher.lastCashOrBankAccountId` / `lastCostCenterId` + `GET …/journals/voucher-defaults` |
| CostCenter (header + line) | ⚠️ line FK; last-used cost center applied when omitted |
| ProjectId on lines | ⚠️ `JournalEntry.ProjectId` |
| FinancialYearId | ✅ set on post when Active FY covers date |
| Multi-currency line amounts | ⚠️ journal FX fields |
| Status + Checked / Recheck / Approved / Post roles | ✅ approve/reject/post/unpost (no separate recheck role) |

---

## Module index (what maps to what)

| # | Our module | POS (code) | BlocksERP (now) |
|---|------------|------------|-----------------|
| 1 | **Core / Platform** | ✅ User, FY, geo | ✅ Users + menu rights; Company/geo in Loan |
| 2 | **Admin** | ✅ | ⚠️ AdminEmail; modules |
| 3 | **Finance** | ✅ full vouchers/CoA/reports | ⚠️ models+menu; Loan banks/defaults; **Areas not copied** |
| 4 | **CRM** | ⚠️ parties | ⚠️ borrower KYC ≠ CRM |
| 5 | **Sales** | ✅ | 📋 menu + models; **walkthrough:** Query → Pre-Cost → Quotation → CSO |
| 6 | **Procurement** | ✅ | 📋 menu only |
| 7 | **Inventory / SCM** | ✅ | 📋 menu + cls models |
| 8 | **HRM** | ✅ | 📋 partial menu refs |
| 9 | **Reports** | ✅ | ⚠️ loan dashboards |
| 10 | **Manufacturing** | — | 📋 Production/formulation menu |
| 11 | **Projects** | — | — |
| 12 | **Lending** | HR advance only | ✅ **Loan Area on disk** |
| 13 | **Skip** | Cab/SMS/CMS | — |

Legend: ✅ code present · ⚠️ partial · 📋 declared in menu/models only · ❌ missing · — N/A

---

## 1. Core / Platform — User module (video transcript)

Walkthrough: login → main module menu → **User** (User Type, User, User Role, Admin Email) + Company in settings.

| Capability | Reference (video) | Ours | Gap |
|------------|-------------------|------|-----|
| Login (username + password) → module main menu | ✅ | ✅ | — |
| Main menu shows all modules user may access | ✅ | ✅ via permissions | Keep menu = granted modules only |
| **User Type** master (Admin, Accountant, Warehouse, Production, Sale; add e.g. Customer) | ✅ | ⚠️ Role used as type; no separate UserType | Optional UserType entity, or document Role = type |
| **User** create: first/last name, picture, email, user type, phones, address | ✅ | ✅ name/email/phone/picture; address thin | Richer address; 2nd phone |
| Optional link user ↔ **Employee** | ✅ | ✅ | — |
| Activate / deactivate user (+ end sessions) | ✅ | ✅ | — |
| Username + password on create | ✅ | ✅ | — |
| **Assign modules / rights** (main User Role screen) | Per feature: View / Edit / Save / Delete | ✅ Role → permissions (`module.action`) + AssignPermissions API/UI | Ensure catalog has view/create/update/delete per screen; hide single report (e.g. stock avg) |
| Example: Accounts report item unchecked → not visible | ✅ | ⚠️ needs permission per report | Seed fine-grained report permissions |
| Example: Supplier View-only (no edit/save/delete) | ✅ | ✅ if write perms omitted | Enforce on API |
| Operators without Delete; Admin deletes | ✅ | ✅ omit delete permission | — |
| **Admin Email** (email + type) | ✅ | ✅ `/api/admin/emails` | — |
| **Company**: name, contact person, logo, phone, cell, email, website, address | ✅ | ✅ Company + LogoUrl; contact person ⚠️ | Contact person if missing |

**User-module DoD:** types (or roles-as-types) · user profile+creds · per-feature CRUD rights · company logo/profile · admin emails (if needed) · optional employee link.  
**Implementation plan:** [USER_MODULE_PLAN.md](./USER_MODULE_PLAN.md).

### Platform extras

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Sessions / refresh tokens | — | — | ✅ |
| Tenant multi-company | ✅ | ✅ | ✅ |
| Fiscal year enforce on post | ✅ | 📋 | ✅ Active FY covers journal date when configured |
| Document sequences / settings / audit / setup | ⚠️–✅ | 📋 | ✅ |
| Country / city | ✅ | ✅ | ⚠️ |

---

## 2. Admin

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| System settings CRUD | ✅ | ⚠️ | ✅ |
| Email admin / Admin Email directory | ✅ SetupEmail | ✅ AdminEmail (video) | ✅ |
| Document type masters | ✅ | ✅ | ⚠️ |
| CMS / blogs / FAQ | ✅ | — | — skip |
| SMS admin | ✅ | — | — skip |

---

## 3. Finance

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Chart of accounts CRUD + hierarchy | ✅ L1–L4 + detail | ✅ leveled CoA + detail | ✅ parent tree |
| Account defaults (purpose → account) | ✅ DefaultGL | ✅ AccountDefault + DefaultGL | ✅ |
| Cost centers | ✅ (on vouchers) | — | ✅ CRUD + journal lines |
| Opening balances | ✅ | — | ⚠️ type exists; wizard thin |
| Journals / JV balanced post | ✅ | — | ✅ post + reverse + unpost |
| Cash payment / receipt vouchers | ✅ | — | ✅ typed CPV/CRV |
| Bank payment / receipt vouchers | ✅ | — | ✅ typed BPV/BRV |
| Approve / unpost vouchers | ✅ | — | ✅ approve/reject/post/unpost |
| Cheque register | ✅ | — | ✅ journal-based + clear/bounce |
| Banks + bank accounts | ✅ | ✅ (+ company bank) | ✅ |
| Bank reconciliation | ⚠️ | — | ✅ open/tick/close |
| AR invoices | ✅ via sale/party | — | ✅ |
| AP bills | ✅ | — | ✅ |
| Payments + apply to AR/AP | ✅ party style | repayments ≠ AR | ✅ |
| Tax codes / rates | ✅ | — | ✅ |
| Currency / FX | ⚠️ | — | ✅ |
| Fixed assets / depreciation | — | — | ✅ thin CRUD + period dep |
| Budgets | — | — | ✅ API + FE list; enforceBudget on journal post |
| TB / ledger / running TB | ✅ | — | ✅ |
| P&L / balance sheet (classified) | ✅ | — | ✅ |
| AR/AP aging | ✅ | — | ✅ buckets + partner balances |
| Cash-flow (cash/bank vouchers) | ⚠️ | — | ✅ posted CPV/CRV/BPV/BRV |

**Primary ref for Finance:** POS. BlocksERP only reinforces **defaults** pattern.

---

## 4. CRM

| Capability | POS | BlocksERP / walkthrough | Ours |
|------------|-----|-------------------------|------|
| Customer master | ✅ as CoA detail | ✅ party = CoA **detail** leaf + rich fields | ✅ `Customer` (not CoA-detail) |
| Contacts | ⚠️ | contact person on party | ✅ |
| Leads → opportunities | — | — | ✅ |
| Campaigns / activities | — | — | ✅ |
| Quotes (+ lines) | ✅ Sale Estimate | ✅ Sale Quotation (+ approve) — see §5 | ✅ `Quote` approve/reject + convert gate |
| Sales orders (+ lines) | ⚠️ via DC/invoice | ✅ CSO / Sale Order — see §5 | ✅ `SalesOrder` |
| Price lists / territories / groups | ⚠️ manual price | — | ✅ domain |
| Salesman | ✅ | ✅ Setup → Salesman master | ✅ Salesman master |
| Return order entity | ✅ Sale Return | — | ✅ ReturnOrder.Complete → SalesCreditNote (posted SI required) |

### 4a. Customer master (walkthrough “Add Customer” vs our `Customer`)

Reference party is a **Fourth Level / detail account** (`Detail Code`) with trade + tax + portal login. Source fields also in BlocksERP `clsAccountDetail`.

| Capability / field | Reference | Ours (`Customer`) | Gap |
|--------------------|-----------|-------------------|-----|
| Detail / customer code (auto) | ✅ Detail Code | ✅ `CustomerCode` | — |
| Name + optional Urdu / local name | ✅ | ⚠️ name only | Local/Urdu name |
| Link to CoA fourth-level (AR control) | ✅ required | ⚠️ separate AR account elsewhere | Explicit party→control account |
| City (FK) + quick-add | ✅ required + `+` | ⚠️ free-text `City` | City master FK |
| Company name, SECP / registry no. | ✅ | ⚠️ `CompanyName` only | Registry / SECP |
| Rating stars | ✅ | ❌ | Optional rating |
| Phone×2, fax, cell×2, contact person | ✅ | ⚠️ single `Phone` + contact | Multi-phone / fax |
| Email, website | ✅ | ✅ | — |
| Credit days + credit limit | ✅ | ✅ `CreditDays` + `CreditLimit` | — |
| Tax type + NTN / STRN / CNIC (tax IDs) | ✅ | ✅ `TaxType` / `Ntn` / `Strn` / `Cnic` (+ legacy `TaxId`) | — |
| Salesman (required) + quick-add | ✅ | ✅ `SalesmanId` FK + Salesman CRUD | required-on-create optional |
| Portal username / password on party | ✅ | ❌ (User is separate) | Optional B2B login link |
| Address, remarks, terms & conditions | ✅ | ⚠️ address + notes; terms thin | Terms on customer |

**We are ahead** on pipeline CRM (leads/opportunities/campaigns). Reference is stronger on **trade-party fields** (credit days, tax IDs, salesman, CoA leaf).

---

## 5. Sales (incl. POS + packaging Sale walkthrough)

### 5a. Core trade docs (POS / our modules)

| Capability | POS | BlocksERP / walkthrough | Ours |
|------------|-----|-------------------------|------|
| Estimate / quote | ✅ | ✅ Sale Quotation (§5b) | ✅ (CRM Quote) |
| Sale order + approve | ⚠️ | ✅ Sale Order + Approval/Rejection (§5e) | ✅ Draft→Approved→Shipped→Invoiced / Cancelled; reject; FIX-6 |
| Delivery / ship → stock | ✅ Delivery Challan | ✅ Dispatch Advice | ⚠️ SO ship (no separate Dispatch doc) |
| Sales invoice + lines | ✅ | ✅ Sale Invoice + General Sale Invoice | ✅ |
| Post → AR / GL | ✅ | ✅ Sale Posting (+ Un-Posting) | ✅ post→AR; void cancels unpaid AR (no unpost reopen) |
| Sale return | ✅ | — | ⚠️ incomplete |
| Credit note | ✅ | ✅ Credit Note | ✅ AR + GL + optional warehouse restock on post / issue on void |
| Invoice hold | ✅ | — | — |
| POS session + lines | ⚠️ | — | ✅ |
| Customer payment apply | ✅ | — | ✅ |
| Sale summary reports | ✅ many | — | ⚠️ |

### 5b. Sale module pipeline (walkthrough menu **Query**)

Packaging pre-sales chain — each step has a **create** screen and a sibling **Approval / Rejection** screen (role-split workflow). Fields corroborated by BlocksERP `clsInvoice` (PouchType, FOHType, PET/MPET, Tax1/Tax2, freight/overhead, margin).

| Stage | Capability | Ours | Gap |
|-------|------------|------|-----|
| **Sale Query** | Capture enquiry (customer + item/spec intent) | ⚠️ Lead / Opportunity (generic CRM) | Dedicated **enquiry / RFQ** doc with packaging specs |
| **Sale Query (Approval / Rejection)** | Gate enquiry before costing | ❌ | Approve/reject workflow on enquiry |
| **Sale Query Pre-Costing** | Cost build: materials, process, FOH, margin → unit rate | ❌ | **Pre-costing** engine / doc (TotalCost, UnitRate, Margin%) |
| **Sale Query Pre-Costing (Approval / Rejection)** | Gate costed rate before quote | ❌ | Costing approve/reject |
| **Sale Quotation** | Commercial quote from approved costing | ✅ `Quote` + lines (price only) | Link quote ← costing; dual tax; packaging line attrs |
| **Sale Quotation (Approval / Rejection)** | Gate quote before customer / before CSO | ✅ approve/reject + convert from Accepted | — |
| **CSO / Sale Order** | Convert approved quote → order (see §5c) | ✅ `SalesOrder` | Spec lines, header trade fields, footer freight/overhead |
| **Setup masters (Sale)** | Country, City, Salesman, Tax, Customers, **Pouch Type**, **Production Process**, **FOH Type** | ⚠️ geo/tax/customer thin; no pouch/process/FOH | Packaging masters + Salesman |

**Pipeline DoD (if packaging Sale is in product scope):**  
Query → approve → Pre-Cost → approve → Quotation → approve → Sale Order → approve → Dispatch → Invoice → Post — with shared line specs and Tax1/Tax2.

### 5c. Customer Sales Order — CSO (walkthrough vs our `SalesOrder`)

Document id pattern e.g. `CSO-…`. Header + line builder + wide grid + footer totals.

| Area | Reference behavior | Ours | Gap |
|------|-------------------|------|-----|
| **Header** | Customer\*, record date, ref # / ref date, credit days, payment mode, ship-to, req. date, remarks | Customer, order/required dates, ship address, notes | Ref #/date, **credit days**, **payment mode** |
| **Line entry** | Item\*, UoM, Qty\*, PCS, Rate\*, gross value; Discount / Tax1 / Tax2 toggles; item remarks | Item, qty, unit price, discount %, Tax1/Tax2 rates | PCS; packaging attrs |
| **Packaging specs (line)** | Size, material type, PET, MPET, lamination micron, cut-off length, folder size, # of UPS | ❌ | **Line specification attrs** (or item attributes) |
| **Line grid** | Sr#, manual item, discount type/value, net rate, gross, Tax1/2 name/type/rate/value, net total, ref. comp | Simple lines (qty × price − discount) | Dual tax columns; manual item; ref. comp |
| **Footer** | Total; disc type %/amount + net disc; freight; overhead → net amount | SubTotal / TaxAmount / TotalAmount | Header **freight**, **overhead**, order-level disc |
| **Actions** | Save; Save & Print; Close | Save via API | Print / save-and-print (app concern) |

### 5d. Setup masters used by Sale (walkthrough)

| Master | Role in Sale | Ours |
|--------|--------------|------|
| Country / City | Party + filters | ⚠️ free-text / thin geo |
| Salesman | Required on customer; used on docs | ❌ / ⚠️ |
| Tax | Tax type on party; Tax1/Tax2 on lines | ✅ party tax IDs + SO/SI Tax1/Tax2 |
| Customers | Party master (§4a) | ✅ incomplete fields |
| Pouch Type | Packaging classification on jobs/lines | ❌ (`clsInvoice.PouchTypeID`) |
| Production Process | Process on costing / production tag | ⚠️ MFG process thin; not Sale-linked |
| FOH Type | Factory overhead type in pre-costing | ❌ (`clsInvoice.FOHTypeID`) |

### 5e. Operational Sale menu (walkthrough **Sale**)

Fulfilment / billing chain after (or alongside) Query. Sibling **Approval / Rejection** only on Sale Order in this menu.

| Screen | Capability | Ours | Gap |
|--------|------------|------|-----|
| **Sale Order** | Confirmed order (CSO / SO) — stock commitment / production feed | ✅ `SalesOrder` | Packaging/header fields (§5c) |
| **Sale Order (Approval / Rejection)** | Role queue to approve or reject SO before dispatch | ✅ approve/reject + `crm.salesOrders` | — |
| **Dispatch Advice** | Ship / DA — stock out, qty vs SO balance | ⚠️ ship on SO (no DA entity) | **Dispatch Advice** doc (POS: Delivery Challan) |
| **Sale Invoice** | Customer SI from SO/DA; AR | ✅ `SalesInvoice` + post → AR | Dual tax / freight/overhead parity |
| **Credit Note** | Credit against SI (returns / adjustments) | ✅ AR + GL + optional `WarehouseId` restock | — |
| **General Sale Invoice** | SI without full SO/DA chain (cash / misc sale) | ⚠️ POS / ad-hoc SI | Explicit “general” SI path if needed |
| **Sale Posting** | Post SI → ledger / AR (batch or single) | ✅ `PostInvoiceAsync` | Batch post queue optional |
| **Sale Un-Posting** | Reverse post → editable / unposted | ❌ Posted → Voided only | **Unpost** (reopen) vs void — same policy as Finance vouchers |

**Order-to-cash DoD (generic, not packaging-only):**  
SO → approve → Dispatch → SI → Post; Credit Note; Unpost policy aligned with Finance.

### 5f. Sale Reports (walkthrough catalog)

Document / operational / analytical reports under **Sale Reports**. Ours: Reports module has metadata + many **stubs**; Finance has aging stubs.

| Report | Capability | Ours | Gap |
|--------|------------|------|-----|
| **Sale Order** | SO list / print by filters | ✅ built-in `SALE_ORDER` + list APIs | Print/PDF later |
| **Sale Invoice** | SI list / print | ✅ `SALE_INVOICE` + `/sales/invoice-summary` | Print/PDF later |
| **Credit Note** | CN list / print | ✅ `/sales/credit-notes` + `CREDIT_NOTE` | Print/PDF later |
| **Customer Information Report** | Party master dump (contact, credit, tax IDs) | ✅ `/sales/customers` | — |
| **Sale Order Schedule** | SO lines by delivery / req. date schedule | ❌ | Schedule by date |
| **Pending Sale Order** | Open SO qty / value not fully dispatched or invoiced | ✅ `/sales/pending-orders` | — |
| **Pending Sale Order for Production** | SO lines awaiting production / not released to MFG | ❌ | SO↔production pending (packaging-relevant) |
| **Sale Schedule** | Broader sale delivery / invoice schedule | ❌ | Schedule view |
| **Sale Invoice Summary** | Aggregated SI totals (period) | ✅ `/sales/invoice-summary` | — |
| **Sale Summary Customer-Item** | Matrix: customer × item | ✅ `/sales/summary-customer-item` | — |
| **Sale Summary Item-Customer** | Matrix: item × customer | ✅ `/sales/summary-item-customer` | — |
| **Sale Summary Item** | Totals by item | ✅ `/sales/summary-by-item` | — |
| **Sale Item Rate Detail** | Historical / last rates by item (and customer) | ✅ `/sales/item-rate-detail` | — |
| **Sale Aging** | AR aging from sales / invoices | ✅ `/sales/aging` → Finance AR aging | — |

**Reports DoD (Sale):** document prints (SO/SI/CN) + pending SO (+ production pending if MFG linked) + summary pivots (customer/item) + item rate detail + aging — all real queries, not placeholders. See also §9.

**Gaps vs POS:** unpost. ReturnOrder.Complete → SalesCreditNote (AR/GL/stock) ✅.  
**Gaps vs packaging Sale walkthrough:** Query → Pre-Cost → Quotation approve; CSO packaging specs; Setup (Pouch/FOH/Process/Salesman); customer trade fields; SO approve queue; Dispatch Advice as own doc; Sale Un-Posting; **Sale Reports catalog (§5f)** (many operational routes already exist — sync docs). Credit Note AR+GL+stock ✅.

---

## 6. Procurement

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Vendor master | ✅ party detail | — | ✅ |
| Purchase requisition | — | — | ✅ |
| Purchase order | — / limited | — | ✅ |
| RFQ | — | — | ✅ Draft→Open→Awarded/Cancelled + vendor responses |
| Goods receipt → stock | ✅ GRN | — | ✅ |
| Purchase invoice → AP | ✅ | — | ✅ PI approve→bill; cancel voids unpaid AP |
| Purchase return | ✅ | — | ✅ PurchaseReturn.Complete → PurchaseDebitNote (stock via DN WarehouseId) |
| Debit note | ✅ | — | ✅ PurchaseDebitNote Draft→Posted→Voided (AP+GL+stock) |
| Vendor payment apply | ✅ | — | ✅ |
| Purchase summary reports | ✅ | — | ✅ `/purchase/invoice-summary` |

**We are ahead** on PR/PO/RFQ. PurchaseDebitNote AP+GL+stock ✅. PurchaseReturn.Complete → DN (stock via DN) ✅.

---

## 7. Inventory / SCM

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Item master | ✅ | — | ✅ |
| Brand / category / subcat / UoM | ✅ | — | ⚠️ cat+UoM; brand thin |
| Item combos (retail kits) | ✅ | — | ❌ |
| Warehouse | ✅ | — | ✅ |
| Stock in | ✅ Store In | — | ✅ adjust / GRN |
| Stock out | ✅ Store Out | — | ✅ adjust / ship |
| Transfer warehouse↔warehouse | ✅ | — | ✅ StockTransfer (Draft→InTransit→Completed + stock; FIX-5/6) |
| Stock balance / ledger | ✅ | — | ✅ |
| Batch / serial / barcode | ⚠️ | — | ✅ domain |
| Reorder rules / valuation method | ⚠️ avg rate report | — | ✅ domain |
| Average cost calculation utility | ✅ report | — | ⚠️ |

**Roughly parity** on core stock; POS has more retail helpers (combo/brand).

---

## 8. HRM

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Employee | ✅ | — | ✅ |
| Department | ✅ | — | ✅ soft-delete CRUD |
| Attendance / time | ✅ | — | ✅ check-in/out/summary |
| Shifts | — | — | ⚠️ |
| Leave types / requests / balance | — | — | ✅ request approve/reject |
| Payroll run + lines | ✅ Salary | — | ✅ Draft→Processed→Posted |
| Salary payment | ✅ | — | ⚠️ post path only |
| Employee advance / loan | ✅ AdvanceLoan | — | ❌ |
| Recruitment (jobs, candidates, interviews) | — | — | ✅ workflows + tenant |
| Performance / training | — | — | ✅ workflows + tenant |
| Employment contracts / documents | ⚠️ | — | ⚠️ |

**POS:** operational payroll. **Ours:** broader HR domain, weaker implementation. BlocksERP KYC ≠ HRM.

---

## 9. Reports

| Capability | POS | BlocksERP / walkthrough | Ours |
|------------|-----|-------------------------|------|
| Report definitions / schedules | — | — | ✅ metadata |
| Financial GL reports | ✅ FinancialController | — | ⚠️ partial real / stubs |
| Sales operational reports | ✅ many | ✅ **Sale Reports** catalog (§5f) | ✅ `/api/reports/operational/sales/*` |
| Purchase operational reports | ✅ | — | ✅ invoice summary |
| Stock / average rate | ✅ | — | ✅ balances + WAC |
| Loan portfolio dashboards | — | ✅ | ❌ |
| Honest execute (no placeholder) | ✅ SP/EF | ⚠️ | ✅ operational JSON queries |

**Sale catalog wired:** Pending SO · SI Summary · Customer×Item / Item×Customer / Item · Item Rate Detail · Sale Aging · Customer info · Credit notes (+ purchase invoice summary, stock balances).

---

## 10. Manufacturing

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Work centers | — | — | ✅ REST + CRUD |
| Production orders + lines | — | — | ✅ start creates run |
| Production runs | — | — | ✅ create / complete / cancel |
| BOM / routing | — | — | ✅ BOM + stock on complete; routing optional |
| Work order (legacy file) | ⚠️ clsWorkOrder | — | — |

**We have the module; refs largely don’t.** Light MFG + BOM issue/receive on complete done; routing optional.

---

## 11. Projects

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Project + tasks / milestones / resources / docs | — | — | ✅ children REST + complete |

**Ours only** — tasks / milestones / resources wired; docs service already present.

---

## 12. Lending (BlocksERP product — we have no module)

| Capability | POS | BlocksERP | Ours |
|------------|-----|-----------|------|
| Borrower profile + KYC masters | — | ✅ | ❌ |
| Document upload + status | — | ✅ | ❌ |
| Loan category / purpose | — | ✅ | ❌ |
| Loan application + fees + tenure | — | ✅ | ❌ |
| Status: waiting → confirmed → active | — | ✅ | ❌ |
| Disbursement via bank | — | ✅ | ❌ |
| Repayment / loan return + allocation | — | ✅ | ❌ |
| Investor on loan | — | ✅ | ❌ |

**Product decision:** include Lending as a new module, or ignore BlocksERP except AccountDefaults pattern.

---

## 13. Skip (reference-only verticals)

| Area | POS | Action |
|------|-----|--------|
| Cab / taxi (zones, booking, pricing, offers, drivers) | ✅ many controllers | Out of scope |
| SMS blast / ledger SMS | ✅ | Out of scope |
| Website CMS (blogs, FAQ, pages, footer) | ✅ | Out of scope |

---

## Scoreboard (where to invest)

| Module | Parity vs POS | Parity vs BlocksERP | Priority for *our* product |
|--------|---------------|---------------------|----------------------------|
| Core | Strong | Strong | FY ✅; permissions ongoing |
| Finance | **Strong core + reports + recon + cheque register + tax→GL** | Defaults ✅ | Optional cheque-book stock |
| CRM | Ahead (pipeline) | N/A | Harden |
| Sales | Strong core; CN AR+GL+stock; RO→CN ✅ | **Weak** vs Query/Pre-Cost/CSO specs | Packaging Query/Pre-Cost/CSO only if in scope |
| Procurement | Ahead on PR/PO; DN AP+GL+stock; PR→DN ✅ | N/A | Packaging/RFQ only if in scope |
| Inventory | Near parity | N/A | Polish |
| HRM | Behind on payroll reality | N/A | P2 after Finance |
| Reports | Operational sales/purchase/stock ✅ | N/A | Schedules / PDF later |
| Manufacturing | Ahead of refs | N/A | Per product plan |
| Projects | Ahead of refs | N/A | Per product plan |
| Lending | N/A | **Missing entire module** | Only if product wants it |

---

## Recommended build order (all modules, functionality)

```text
1. Finance: defaults, cost centers, typed cash/bank journals, approve/unpost, honest reports
2. ~~Sales/Procurement: returns + credit/debit notes (ReturnOrder→SalesCreditNote)~~ ✅
3. ~~Party harden: credit days, tax IDs, salesman master, dual tax on SO/SI~~ ✅
4. ~~Reports: wire stubs to real queries (sales/purchase/stock catalogs)~~ ✅
5. ~~HRM: employee + leave + attendance + simple payroll~~ ✅ (advance-loan out of scope)
6. ~~MFG light + Projects children~~ ✅ (+ BOM/stock on complete)
6b. ~~Platform forgot-password + session list/revoke~~ ✅
6c. ~~Core + Admin dashboard summaries~~ ✅
6d. ~~Inventory stubs: item categories + stock adjustments (Draft→Posted)~~ ✅
6e. ~~TaxCode → posting GL account (CRUD `/api/finance/tax-codes`)~~ ✅
6f. ~~SalesOrder workflow (approve/reject/ship/invoice + FIX-5/6)~~ ✅
6g. ~~Quote workflow (send/approve/reject/convert + FIX-5/6)~~ ✅
6h. ~~OpenAPI method polish (Reports + Inventory/HRM/MFG/Projects)~~ ✅
6i. ~~StockTransfer workflow + tenant + stock on complete~~ ✅
6j. ~~Opportunity workflow (win/lose/cancel + convert from Won; Open-only edit; FIX-5)~~ ✅
6k. ~~Lead workflow (qualify/lose + convert from Qualified; New/Qualified edit; FIX-5)~~ ✅
6l. ~~Stock tenant fail-closed (balance/transfer/ledger + StockService ITenantProvider reads)~~ ✅
6m. ~~Campaign workflow (launch/pause/complete/cancel; Draft-only edit; FIX-5)~~ ✅
6n. ~~Activity workflow (complete/cancel/reschedule; Open-only edit; FIX-5)~~ ✅
6o. ~~Leave request tenant fail-closed + Pending→Approved/Rejected/Cancelled (FIX-5/6)~~ ✅
6p. ~~TimeEntry/Attendance tenant fail-closed + Attendance GraphQL~~ ✅
6q. ~~Project task/milestone/resource tenant fail-closed + Complete DocumentWorkflow (FIX-5)~~ ✅
6r. ~~Customer tenant fail-closed + Activate/Deactivate (Vendor parity; FIX-5)~~ ✅
6s. ~~ReturnOrder GraphQL/REST/service tenant fail-closed~~ ✅
6t. ~~AccountDefaults + Payment GraphQL tenant honesty~~ ✅
6u. ~~FixedAsset service tenant fail-closed (was soft-filter)~~ ✅
6v. ~~BillService + BankService (accounts/tx/recon) tenant fail-closed~~ ✅
6w. ~~AccountingService cheque/budget/voucher-defaults tenant fail-closed~~ ✅
6x. ~~ReportService built-in operational reports tenant fail-closed~~ ✅
6y. ~~OperationalReportService + Quote/SO scoped + CN/DN account defaults fail-closed~~ ✅
6z. ~~Leave AddLeaveRequest GraphQL/REST tenant args + Payroll fail-closed~~ ✅
6aa. ~~ApplyTenantFilter / MatchesTenant fail-closed foundation~~ ✅
6ab. ~~Contact tenant fail-closed + Activate/Deactivate~~ ✅
6ac. ~~CRM pipeline (Lead/Opp/Campaign/Activity/Quote/SO) explicit tenant~~ ✅
6ad. ~~POS session Open→Closed DocumentWorkflow + FIX-5~~ ✅
6ae. ~~FIX-5 trade docs (SI/AR/Bill/Payment/POSTxn) + PR DocumentWorkflow/FIX-6~~ ✅
6af. ~~Project Update Status FIX-5; P6 continuous closed~~ ✅
7. Packaging Sale pipeline (Query → Pre-Cost → Quotation approve → CSO specs) — ONLY if product wants flexible-packaging Sale
8. Lending module — ONLY if explicitly in scope (use BlocksERP)
```

---

## Explicit non-goals

- UI/menu clone of either reference  
- Cab, SMS, CMS  
- Lending unless product asks  
- Packaging Query/Pre-Cost/FOH/Pouch masters unless product asks  
- CoA-detail parties instead of Customer/Vendor  
- Schema copy from PosMB / BlocksERP EDMX
