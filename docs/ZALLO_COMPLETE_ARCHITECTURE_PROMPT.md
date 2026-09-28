# Zallo × ERP — Complete Architecture Prompt

One copy-paste prompt. Paste the **Master prompt** into a new chat (or keep it as system/context). Then optionally paste a **SKU follow-up** to deepen one product.

Source of truth for *current* code: `D:\Projects\erp` (.NET modular monolith + React SPA + `backend-nestjs/` stubs).  
Target of truth: **Zallo multi-product suite** — NestJS API + Next.js portal + TypeScript contracts + **Postgres/Supabase**. No shared tables across SKUs.

Existing domain models (field catalogs) are embedded in the master prompt under **EXISTING DOMAIN MODELS**. Live C# remains in `ERP.Domain/` and DTOs in `ERP.Shared/DTOs/`.

---

## Master prompt (paste first)

```text
You are a principal software architect with 15+ years designing multi-tenant SaaS ERP, CRM, POS, finance, and voice platforms. Design like you are shipping to regulated SMBs (clinics, retail, services): clear bounded contexts, boring reliability, database-first integrity.

You are converting and redesigning the existing product at D:\Projects\erp into the Zallo multi-product suite. Do not invent a greenfield ERP that ignores this codebase. Read live code over stale docs.

==================================================
A. CURRENT SYSTEM (SOURCE — DO NOT COPY GAPS)
==================================================

Repo: D:\Projects\erp
Solution: ERP.SaaS.sln — .NET 9 Clean Architecture modular monolith + React 18/Vite SPA.

Projects:
- ERP.Api — REST (/api/…), HotChocolate GraphQL (/graphql), JWT, FluentValidation, TenantMiddleware, ExceptionMiddleware, Swagger
- ERP.Application — use-case *Service classes (NOT MediatR CQRS)
- ERP.Domain — entities by module folder; BaseEntity = Id, TenantId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsActive
- ERP.Infrastructure — ERPDbContext (EF Core SQL Server) + Dapper IStoredProcedureExecutor + schema/SP SQL deploy
- ERP.Shared — DTOs (~151), JwtHelper, PasswordHelper (PBKDF2), exceptions, ITenantProvider
- ERP.Tests — CRM-focused only
- frontend/ — React + RTK + Saga; modules: core, admin, crm, hrm, finance, inventory, procurement, manufacturing, projects, reports, pos
- backend-nestjs/ — Nest monorepo stubs already generated:
  apps: gateway, identity, admin, audit, crm, sales, finance, hrm, inventory, procurement, manufacturing, projects, reports, backend-nestjs (legacy root app)
  libs (empty scaffolds): @erp/common, @erp/database, @erp/security, @erp/contracts, @erp/audit-client

Live modules / bounded contexts in .NET:
1. Core / Auth / Tenancy — setup super-admin, login/refresh/logout/change-password/reset-password/verify-email, tenants, companies, plans, subscriptions, sessions
2. Admin — users, roles, permissions, system settings
3. Audit — AuditLog + SystemLog (NOT automatic on writes today)
4. CRM — customers, contacts, leads, opportunities, quotes+lines, sales orders+lines, activities, campaigns, customer groups, price lists, territories, return orders
5. Sales / POS — sales invoices+lines, POS sessions, POS transactions+lines
6. Finance — CoA, journals/entries, currencies, FX, banks, taxes, AR invoices, AP bills, payments, fixed assets, budgets, cost centers
7. HRM — employees, leave, payroll, recruitment, performance, training, time tracking, benefits (Department entity likely missing/stub)
8. Inventory / SCM — items, categories, groups, attributes, variants, pricing, UoM, warehouses, stock balances/ledgers, transfers, adjustments, barcodes, batch/serial, reorder, valuation
9. Procurement — vendors, POs, GRNs, RFQs, requisitions, purchase invoices
10. Manufacturing — work centers, production orders/lines, production runs (thin; BOM/routing mostly absent)
11. Projects — projects, tasks, resources, documents, milestones (nav to CRM Customer + HRM Employee — cross-BC coupling)
12. Reports — definitions, executions, schedules (thin)

REST prefixes to preserve as compatibility aliases during cutover (gateway may also expose /v1):
/api/setup /api/auth /api/tenants /api/admin/* /api/crm/* /api/sales/* /api/finance/* /api/hrm/* /api/inventory/* /api/procurement/* /api/manufacturing/* /api/projects/* /api/reports/*

Auth today:
- Custom JWT (not ASP.NET Identity). Claims: NameIdentifier, Email, TenantId, Role(s), jti, iat
- Access ~60 min; refresh 7 days stored on User (plaintext-ish — hash in target)
- Lockout after 5 failures / 30 min; PBKDF2 hash+salt; rehash-on-login pattern exists
- Roles: SuperAdmin, Admin, Manager, User
- DefaultPermissions = Core admin only (core.users.*, core.roles.*, core.tenants.*, core.settings.*, core.audit.view, core.logs.view)
- Controllers use [Authorize] only — permissions are NOT enforced on endpoints. Fix this.

Tenancy today:
- Shared SQL Server DB (erpnew) + TenantId column
- Tenant.DatabaseName / ConnectionString modeled but unused
- TenantMiddleware: JWT TenantId wins; SuperAdmin may override X-Tenant-Id
- Only CRM consistently filters via CrmRepositoryBase.ApplyTenant

Data access today:
- EF for most CRM/Admin/Sales/Finance/Stock
- Dapper SPs for UserRepository, WarehouseRepository, VendorRepository (partial), AuditService
- SP files: ERP.Infrastructure/Database/StoredProcedures/*Module_StoredProcedures.sql
- Schema files: ERP.Infrastructure/Database/Schemas/*Module_Schema.sql
- Naming mixed: [Core].sp_GetAllUsers vs addAuditLog
- EF maps Core.* to schema "Core"; most other entities dbo

EXISTING DOMAIN MODELS — PORT THESE (normalize duplicate columns / dual IDs, do not drop business fields without written rationale).
Every entity inherits BaseEntity unless noted: Id uuid, TenantId uuid, CreatedAt, UpdatedAt?, CreatedBy?, UpdatedBy?, IsActive bool.
Map IsActive=false → deleted_at and/or status. Source: ERP.Domain/**. DTOs: ERP.Shared/DTOs/** (Create/Update/Query/Response).
DTO-only (no domain class today — still design tables): HRM DepartmentDto, BenefitDto. Attendance is service/controller-level without a dedicated entity.
Some Finance types (Currency, BankReconciliation, BankTransaction, BillLine, InvoiceLine, JournalEntry, DepreciationEntry, ExchangeRate, TaxTransaction, IntercompanyTransaction) do NOT inherit BaseEntity — normalize them onto the kernel pattern in Postgres.
Duplicate/overlapping columns are common (e.g. Lead.LeadSource+Source, Quote.ExpiryDate+ValidUntil, SalesOrder.OrderNumber+SalesOrderNumber, Payroll period fields twice, ItemCategory.CategoryName+Name). Collapse only with a mapping note.

----- Kernel -----
BaseEntity: Id, TenantId, CreatedAt, UpdatedAt?, CreatedBy?, UpdatedBy?, IsActive

----- Core → plat_ -----
AuditLog: UserId, Action, EntityName, EntityId, OldValues(json), NewValues(json), IpAddress, UserAgent, Timestamp
Company: CompanyName, LegalName, RegistrationNumber, TaxNumber, AddressLine1, AddressLine2, City, State, Country, ZipCode, Phone, Email, Website, LogoUrl, CompanyCode, TaxId, Address, PostalCode, Currency, TimeZone
Permission: Name, Description?, Module, ModuleName, Action, ActionName, Notes?
Plan: PlanName, Description, MonthlyPrice, YearlyPrice, Currency, MaxUsers, MaxStorage, FeaturesJSON, IsActive
Role: Name, RoleName, Description?, IsSystemRole, Notes?
RolePermission: RoleId, PermissionId, AssignedAt, AssignedBy?
Setting: SettingKey, SettingValue, Description?, SettingType, Category, IsEncrypted, IsReadOnly, ValidationRule?, Notes?
Subscription: PlanId, StartDate, EndDate, Status, PaymentStatus, Amount, Currency, PaymentMethod, PaymentReference?, IsAutoRenew, LastBillingDate?, NextBillingDate?, Notes?
Tenant: TenantName, Domain, Industry, Country, Status, DatabaseName, ConnectionString (do NOT port plaintext connection strings into plat_ — replace with service_links)
User: Username, Email, PasswordHash, PasswordSalt?, FirstName?, LastName?, PhoneNumber?, IsEmailVerified, IsPhoneVerified, LastLoginAt?, EmailVerifiedAt?, PhoneVerifiedAt?, ProfilePicture?, Notes?, CompanyId?, Salutation?, FullName?, Gender?, DateOfBirth?, JobTitle?, Department?, Nationality?, PrimaryEmail?, SecondaryEmail?, PrimaryPhone?, SecondaryPhone?, MobilePhone?, Fax?, LinkedIn?, Twitter?, Facebook?, Instagram?, PreferredCommunicationMethod?, LanguagePreference?, TimeZone?, Status?, IsTenantOwner, LastLoginDate?, IsLocked, FailedLoginAttempts, LockedUntil?, EmailVerificationToken?, PasswordResetToken?, PasswordResetExpires?, RefreshToken?, RefreshTokenExpires?
UserRole: UserId, RoleId, AssignedAt, AssignedBy?, ExpiresAt?
UserSession: UserId, SessionToken, RefreshToken?, LoginTime, LogoutTime?, ExpiresAt, IPAddress, UserAgent, Status

----- Admin → plat_ -----
SystemLog: LogLevel, Message, Exception?, Source?, Method?, UserId?, IPAddress?, Timestamp, Properties?(json)
SystemSetting: SettingKey, SettingValue, Description?, SettingType, Category, IsEncrypted, IsReadOnly, ValidationRule?, Notes?

----- CRM → crm_ -----
Activity: ActivityType, Subject, Description?, StartDate, EndDate?, CustomerId?, LeadId?, OpportunityId?, AssignedToUserId, Status, Priority, Location?, Notes?, RelatedEntityId?, RelatedEntityType?
Campaign: CompanyId, CampaignName, CampaignType, Status, StartDate, EndDate?, Budget?, ActualCost?, ExpectedResponse?, ActualResponse?, Description?, Currency?, TargetAudience?, Goals?, LaunchDate?, PausedDate?, CompletedDate?
Contact: FirstName, LastName, Email, Phone?, JobTitle?, Department?, CustomerId?, IsPrimary, Notes?
Customer: CompanyId, CustomerCode, CustomerName, CompanyName, ContactPerson?, Email?, Phone?, Address?, City?, State?, Country?, PostalCode?, CustomerType, Status, Industry?, CreditLimit, OutstandingBalance, PaymentTerms?, TaxId?, Website?, Notes?, CurrencyCode
CustomerGroup: GroupName, Description?, DiscountPercentage
Lead: LeadCode, FirstName, LastName, CompanyName?, Email?, Phone?, Address?, City?, State?, Country?, LeadSource, Source, Status, Priority, EstimatedValue?, LastContactDate?, AssignedToUserId?, AssignedToId?, CompanyId?, ContactPerson?, PostalCode?, Industry?, AssignedTo?, LeadScore?, Notes?, ConvertedDate?
Opportunity: OpportunityCode, OpportunityName, Title, CustomerId?, LeadId?, Stage, Status, EstimatedValue, ActualValue?, Amount, Currency, Probability, ExpectedCloseDate?, ActualCloseDate?, ClosedDate?, ConvertedDate?, CloseReason?, Description?, Notes?, Source?, AssignedTo?, AssignedToUserId?
PriceList: PriceListName, Description?, ValidFrom, ValidTo?, IsDefault
Quote: QuoteNumber, CustomerId?, QuoteDate, ExpiryDate, ValidUntil, OpportunityId?, Status, SubTotal, TaxAmount, TotalAmount, Currency, Notes?, Terms?, SentDate?, ConvertedDate?, AssignedToUserId?, CompanyId?
QuoteLine: QuoteId, ItemId?, ItemName, Description?, Quantity, UnitPrice, DiscountPercent, LineTotal, Notes?
ReturnOrder: ReturnNumber, ReturnDate, CustomerId, SalesOrderId?, Reason, Status, TotalAmount, Notes?
SalesOrder: CompanyId, OrderNumber, SalesOrderNumber, CustomerId?, QuoteId?, OrderDate, RequiredDate?, ShippedDate?, ApprovedDate?, InvoicedDate?, Status, SubTotal, TaxAmount, TotalAmount, Currency?, Terms?, ShippingAddress?, BillingAddress?, Notes?, AssignedToUserId?
SalesOrderLine: SalesOrderId, ItemId?, ItemName, Description?, Quantity, UnitPrice, DiscountPercent, LineTotal, Notes?
SalesTerritory: TerritoryName, Description?, ManagerId?

----- Sales/POS → pos_ -----
POSSession: SessionNumber, CashierId, TerminalId, StartTime, EndTime?, OpeningBalance, ClosingBalance, ExpectedBalance, Difference, Status, Notes?
POSTransaction: TransactionNumber, POSSessionId, SessionId, CustomerId, TransactionDate, PaymentMethod, SubTotal, TaxAmount, DiscountAmount, TotalAmount, AmountPaid, ChangeAmount, Status, Notes?
POSTransactionLine: POSTransactionId, ItemId, Description, Quantity, UnitPrice, DiscountRate, DiscountAmount, TaxRate, TaxAmount, LineTotal, LineNumber
SalesInvoice: InvoiceNumber, CustomerId, InvoiceDate, DueDate, Status, SubTotal, TaxAmount, TotalAmount, PaidAmount, BalanceAmount, Notes?, Terms?, SalesOrderId?, PostedAt?, PostedBy?
SalesInvoiceLine: SalesInvoiceId, ItemId, Description, Quantity, UnitPrice, TaxRate, TaxAmount, LineTotal, LineNumber

----- Finance → fin_ -----
Bank: CompanyId, BankName, BankCode, SwiftCode?, Address?, City?, State?, Country?, PostalCode?, Phone?, Email?, Website?
BankAccount: CompanyId, BankId, AccountName, AccountNumber, AccountType, Currency, Balance, IBAN?, SWIFT?, GLAccountId
BankReconciliation (no BaseEntity): ReconciliationId, BankAccountId, TenantId, ReconciliationDate, OpeningBalance, ClosingBalance, Difference, Status, CreatedAt, CreatedBy?, ClosedAt?, ClosedBy?
BankTransaction (no BaseEntity): BankTxId, BankAccountId, TenantId, BankReference?, TransactionDate, Amount, CurrencyCode, Description?, ImportedAt, MatchedPaymentId?, IsReconciled, CreatedAt, CreatedBy?
Bill: CompanyId, VendorId, BillId, SupplierId, BillNumber, BillDate, DueDate, CurrencyCode, SubTotal, TaxAmount, TaxTotal, TotalAmount, Balance, Status, PurchaseOrderId?, Notes, ApprovedDate?, PaidDate?, PostedAt?, PostedBy?, IsLocked
BillLine (no BaseEntity): BillLineId, BillId, ProductId?, Description, Quantity, UnitPrice, LineTotal, TaxRate, GLAccountId, CreatedAt, CreatedBy?
Budget: AccountId, BudgetId, BudgetName, StartDate, EndDate, BudgetPeriodStart, BudgetPeriodEnd, BudgetAmount, ActualAmount, CurrencyCode, Status
ChartOfAccount: AccountId, AccountCode, AccountName, AccountType, ParentAccountId?, CurrencyCode?, IsPostingAllowed
CostCenter: CostCenterCode, CostCenterName, CostCenterId, Description?
Currency (no BaseEntity): CurrencyCode, CurrencyName, Iso4217?, Symbol?, IsActive, CreatedAt
DepreciationEntry (no BaseEntity): DepreciationId, AssetId, TenantId, PeriodStart, PeriodEnd, DepreciationAmount, PostedJournalId?, CreatedAt, CreatedBy?
ExchangeRate (no BaseEntity): ExchangeRateId, TenantId, FromCurrency, ToCurrency, Rate, EffectiveDate, CreatedAt, CreatedBy?
FixedAsset: AssetCode, AssetName, AssetCategory, AccountId, AssetId, PurchaseCost, CurrentValue, PurchaseDate, DisposalDate?, Status, CurrencyCode
IntercompanyTransaction (no BaseEntity): IntercompanyId, TenantId, FromCompanyId, ToCompanyId, Amount, CurrencyCode, Description?, Status, CreatedAt, CreatedBy?
Invoice (AR): InvoiceId, CustomerId, InvoiceNumber, InvoiceDate, DueDate, CurrencyCode, SubTotal, TaxTotal, TotalAmount, Balance, Status, SalesOrderId?, PostedAt?, PostedBy?, IsLocked
InvoiceLine (no BaseEntity): InvoiceLineId, InvoiceId, ProductId?, Description, Quantity, UnitPrice, LineTotal, TaxRate, GLAccountId, CreatedAt, CreatedBy?
Journal: JournalId, JournalType, ReferenceNo?, JournalDate, Description?, CurrencyCode?, ExchangeRate?, TotalDebit, TotalCredit, Status, JournalNumber, PostedAt?, PostedBy?, IsLocked
JournalEntry (no BaseEntity): Id, EntryId, JournalId, TenantId, AccountId, CostCenterId?, ProjectId?, Debit, Credit, Narration?, EntityReferenceTable?, EntityReferenceId?, EntryDate, Reference?, CreatedAt, CreatedBy?
Payment: PaymentNumber, PaymentDate, PaymentType, PaymentMethod, Amount, Currency, CurrencyCode, BankAccountId?, RelatedInvoiceId?, RelatedBillId?, CustomerId?, SupplierId?, Status, Reference?, ReferenceNumber?, Notes?, ProcessedDate?, ProcessedAt?, ProcessedBy?, CompanyId?
PaymentApplication: PaymentId, InvoiceId, AppliedAmount, AppliedDate, Status
TaxCode: TaxCodeName, Code, TaxCodeDescription, TaxRate, TaxType, AccountId, TaxCodeId
TaxRate: Name, Code, Rate, Type, TaxType, EffectiveDate, ExpiryDate?
TaxTransaction (no BaseEntity): TaxTransactionId, TenantId, TaxCodeId, EntityReferenceTable, EntityReferenceId, TaxAmount, TaxableAmount, CreatedAt, CreatedBy?

----- HRM → hrm_ -----
Candidate: FirstName, LastName, Email, Phone?, Address?, City?, State?, ZipCode?, Country?, DateOfBirth?, Gender?, CurrentPosition?, CurrentCompany?, YearsOfExperience?, Skills?, Education?, Experience?, LinkedInProfile?, PortfolioUrl?, Status, Notes?
Employee: CompanyId?, EmployeeCode, Salutation?, FirstName, LastName, PreferredName?, Gender?, DateOfBirth?, NationalId?, Nationality?, MaritalStatus?, JobTitle?, DepartmentId?, ManagerId?, EmploymentType?, WorkEmail?, PersonalEmail?, WorkPhone?, MobilePhone?, WorkLocationId?, DateHired?, DateTerminated?, Status, IsRemote, EmploymentContractId?, IsDeleted
EmployeeAddress: EmployeeId, AddressType, AddressLine1, AddressLine2?, City, State?, CountryCode, PostalCode?, IsPrimary
EmployeeDocument: EmployeeId, DocumentType, FileUrl, UploadedBy, UploadedAt, ExpiryDate?, IsSensitive
EmploymentContract: EmployeeId, ContractType, StartDate, EndDate?, WorkingHoursPerWeek, BaseSalary, Currency, PayFrequency, NoticePeriodDays, ProbationPeriodDays, ContractDocumentUrl?, Status
HRSettings: DefaultPayFrequency, WorkingDaysJson?, OvertimePolicyJson?, LeaveAccrualPolicyJson?, TaxSettingsJson?, ComplianceSettingsJson?
Interview: JobApplicationId, CandidateId, InterviewerId, InterviewDate, ScheduledDate, Duration, InterviewType, Status, Location?, Notes?, Feedback, Rating
JobApplication: JobOpeningId, CandidateId, ResumePath, ResumeUrl, AppliedDate, Status, CoverLetter, Notes?
JobOpening: Title, Description, Department, Location, EmploymentType, ExperienceLevel, SkillsRequired, SalaryRange, Currency, MinSalary?, MaxSalary?, PostedDate, ClosingDate?, Status, PostedBy, Requirements?, Responsibilities?, Benefits?, HiringManagerId?
LeaveBalance: EmployeeId, LeaveTypeId, Accrued, Taken
LeaveRequest: EmployeeId, LeaveTypeId, StartDate, EndDate, TotalDays, DaysRequested, Status, RequestedAt, AppliedAt, ApprovedAt?, ApprovedBy?, Notes?, Reason?, RejectedBy?, RejectedAt?, RejectionReason?, CompanyId?
LeaveType: Name, Code, IsPaid, MaxAllowedPerYear, RequiresApproval
Payroll: CompanyId, EmployeeId?, PayrollPeriodStart, PayrollPeriodEnd, PayPeriodStart, PayPeriodEnd, PayDate, PayrollRunDate, Currency, BasicSalary, Allowances, Deductions, GrossSalary, NetSalary, TotalGross, TotalDeductions, TotalNet, Status, ProcessedAt?, PostedBy?, PostedAt?
PayrollComponent: Name, Type, CalculationType, Value, IsTaxable
PayrollLine: PayrollId, EmployeeId, Earnings, Allowances, Deductions, TaxAmount, NetPay, BankAccountId?, PayrollComponentId?
PerformanceReview: EmployeeId, ReviewerId, ReviewPeriodStart, ReviewPeriodEnd, ReviewDate, OverallRating, GoalsAchieved?, GoalsNotAchieved?, Strengths?, AreasForImprovement?, Comments?, Status, GoalsJson?, CompanyId?
Shift: ShiftName, StartTime, EndTime, IsOvernight, GracePeriodMinutes
TimeEntry: EmployeeId, ProjectId?, ClockIn, ClockOut?, Date, HoursWorked?, Device?, Location?, Notes?, Status, Source?
Training: Title, Description, Type, TrainingType, StartDate, EndDate, Status, Location?, Instructor?, Trainer?, Cost?, MaxParticipants, Prerequisites?, Materials?, ParticipantsJson?, EmployeeId?, InstructorId?
TrainingEnrollment: TrainingId, EmployeeId, EnrolledAt, Status, Score?, CertificateUrl?

----- SCM/Inventory → inv_ -----
Barcode: ItemId?, VariantId?, BarcodeValue, BarcodeType, IsPrimary
BatchNumber: ItemId, BatchNumberValue, ManufacturingDate?, ExpiryDate?, Quantity
BatchSerialTracking: ItemId, WarehouseId, BatchNo?, SerialNo?, ManufactureDate?, ExpiryDate?, Quantity, Status
InventoryAdjustment: CompanyId, WarehouseId, ItemId, AdjustmentType, Quantity, Reason, ApprovedBy?
InventoryValuationMethod: MethodName, MethodCode, Description?
Item: CompanyId, ItemCode, ItemName, Description?, CategoryId, UnitOfMeasureId, ItemType, HSNCode?, BarCode?, ReorderLevel, ReorderQuantity, DefaultWarehouseId?, CostPrice, SellingPrice, IsSerialized, IsBatchTracked
ItemAttribute: AttributeName, AttributeType, IsRequired
ItemCategory: CompanyId, CategoryCode, CategoryName, Name, ParentCategoryId?, Description?
ItemGroup: GroupCode, GroupName, ParentGroupId?, Description?
ItemPricing: ItemId, PriceListName, UnitPrice, MinQuantity, MaxQuantity?, ValidFrom, ValidTo?
ItemVariant: ItemId, VariantCode, VariantName, SKU?, CostPrice, SellingPrice
ItemVariantAttribute: ItemVariantId, ItemAttributeId, AttributeValue
ReorderRule: ItemId, WarehouseId, MinStockLevel, MaxStockLevel, ReorderPoint, ReorderQuantity, LeadTimeDays
SerialNumber: ItemId, SerialNumberValue, Status, Location?
StockBalance: CompanyId, ItemId, WarehouseId, CurrentQuantity, ReservedQuantity, LastUpdated
StockLedger: CompanyId, ItemId, WarehouseId, TransactionType, TransactionDate, QuantityIn, QuantityOut, BalanceQty, UnitPrice, TotalValue, ReferenceDocType?, ReferenceDocId?
StockTransfer: CompanyId, FromWarehouseId, ToWarehouseId, TransferDate, Status, ApprovedBy?
StockTransferDetail: TransferId, ItemId, Quantity, BatchSerialId?, UnitPrice
UnitOfMeasure: UnitCode, UnitName, Name, Description?, UnitType, ConversionFactor?, BaseUnitId?
Warehouse: CompanyId, WarehouseCode, WarehouseName, Name, Description?, AddressLine1, AddressLine2?, City, State, Country, PostalCode, ContactPerson?, PhoneNumber?, Email?, ManagerId?, Manager?

----- Procurement → prc_ -----
GoodsReceiptNote: CompanyId, POId, VendorId, GRNNumber, ReceivedDate, WarehouseId, ReceivedByUserId, Status, Remarks
GoodsReceiptNoteDetail: GRNId, ItemId, QuantityReceived, BatchSerialId?, UnitPrice, TaxAmount, TotalAmount
PurchaseInvoice: CompanyId, POId?, VendorId, InvoiceNumber, InvoiceDate, DueDate, Currency, TotalAmount, TaxAmount, NetPayable, Status
PurchaseInvoiceDetail: InvoiceId, ItemId, Quantity, UnitPrice, TaxAmount, NetAmount
PurchaseOrder: CompanyId, RFQId?, VendorId, PurchaseOrderNumber, OrderDate, ExpectedDeliveryDate, Currency, TotalAmount, PaymentTerms, DeliveryTerms, Status, ApprovedBy
PurchaseOrderDetail: POId, ItemId, QuantityOrdered, UnitPrice, Discount, TaxAmount, NetAmount, ReceivedQuantity
PurchaseRequisition: CompanyId, RequestedByUserId, RequestorId, DepartmentId?, RequestDate, RequiredByDate, Status, Remarks
PurchaseRequisitionDetail: PRId, ItemId, QuantityRequested, UnitOfMeasureId, EstimatedUnitPrice, Remarks
RequestForQuotation: CompanyId, PRId?, RequestorId, IssuedDate, DueDate, Status
RFQDetail: RFQId, ItemId, QuantityRequested, UnitOfMeasureId, Remarks
RFQVendorResponse: RFQId, VendorId, ResponseDate, TotalQuotedAmount, Currency, PaymentTerms, DeliveryTime, Status
RFQVendorResponseDetail: RFQResponseId, ItemId, QuotedUnitPrice, Discount, TaxAmount, FinalUnitPrice
Vendor: CompanyId, VendorCode, VendorName, ContactPerson, PhoneNumber, Email, AddressLine1, AddressLine2, City, State, Country, PostalCode, TaxNumber, PaymentTerms, CreditLimit
VendorBankAccount: VendorId, BankName, AccountNumber, IBAN, SWIFTCode, Currency, IsPrimary

----- Manufacturing → mfg_ -----
ProductionOrder: CompanyId?, ProductionOrderNumber, OrderNumber, ItemId?, WorkCenterId?, Quantity, StartDate?, EndDate?, PlannedStartDate?, PlannedEndDate?, ActualStartDate?, ActualEndDate?, Status, Priority, CompletedQuantity, Notes?, CancellationReason?
ProductionOrderLine: ProductionOrderId, ItemId?, Quantity, ConsumedQuantity, LineType, Notes?
ProductionRun: ProductionOrderId, RunNumber, StartTime, EndTime?, QuantityProduced, QuantityRejected, Status, Notes?
WorkCenter: CompanyId?, WorkCenterCode, WorkCenterName, Description?, WorkCenterType, Capacity, CapacityUnit, Notes?
(Add BOM header/lines + routing — missing in .NET)

----- Projects → prj_ -----
Project: CompanyId?, ProjectCode, ProjectName, Description?, ClientId? (external crm customer id — NOT FK), ProjectManagerId? (external hrm employee id — NOT FK), StartDate?, EndDate?, ActualStartDate?, ActualEndDate?, Status, Priority, Budget, ActualCost, Progress, Notes?
ProjectDocument: ProjectId, DocumentName, DocumentType, FilePath, Description?, FileSize, Version?, IsPublic
ProjectMilestone: ProjectId, MilestoneName, Description?, TargetDate?, ActualDate?, Status, Notes?
ProjectResource: ProjectId, EmployeeId?, ResourceType, ResourceName, AllocatedHours, ActualHours, HourlyRate, Status, Notes?
ProjectTask: ProjectId, TaskName, Description?, AssignedToId?, ParentTaskId?, StartDate?, DueDate?, CompletedDate?, Status, Priority, EstimatedHours, ActualHours, Progress, Notes?

----- Reports → rpt_ -----
Report: CompanyId?, Name, ReportName, ReportCode, Description?, ReportType, Category, Query (DO NOT port arbitrary Query to clients — whitelist query_key), Parameters, OutputFormat, IsPublic, Notes?
ReportExecution: ReportId, ScheduleId?, StartedAt, CompletedAt?, Status, Parameters?, OutputPath?, ErrorMessage?, ExecutionTimeMs, RecordCount, ResultData?, ExecutedBy?
ReportSchedule: ReportId, ScheduleName, ScheduleType, CronExpression, NextRunTime?, LastRunTime?, EmailRecipients?, Notes?

NEW SKUs with no .NET models (design from product prompts, not from ERP.Domain): REC rec_, CMS cms_, OUT out_, MBL mbl_.

When producing ERD/DDL/DTOs/Nest modules, every listed model must appear (normalized) in the target SKU. Prefer existing field names in contracts for React compatibility.

Honest gaps (close in Zallo design — do not port):
- Application references Infrastructure (AuthService/SetupService/PermissionService use DbContext directly)
- Many Finance/Procurement/Inventory services are STUBS (Guid.Empty / empty lists)
- Audit is dual-path and opt-in (EF admin API + Dapper GraphQL); no SaveChanges interceptor
- No domain events, outbox, message bus, or inter-module HTTP
- Invoice trinity: CRM SO.InvoiceAsync only sets status "Invoiced"; Sales.SalesInvoice; Finance.Invoice — no orchestration
- PurchaseInvoice vs Finance.Bill — parallel, no posting
- Inventory vs SCM vs Services/StockService duplication
- Duplicate Role repositories, Invoice service interfaces, AuditLogDto, mapper layers
- GraphQL Finance + Procurement exist but unregistered
- docs/FEATURES.md and ARCHITECTURE.md are partly stale vs ModuleServiceCollectionExtensions (all modules registered in DI; stubs remain)
- CORS AllowAll; secrets historically messy — target must be env + validation
- No crm.*/finance.*/inventory.* permission catalog

Cross-module reality: in-process only. LeadService creates Customer/Opportunity directly. Projects FK-navigates Customer+Employee. Manufacturing navigates Item.

==================================================
B. TARGET — ZALLO SUITE (NON-NEGOTIABLES)
==================================================

Stack:
- NestJS REST APIs (/v1 primary; /api compatibility layer on Platform gateway during migration)
- Next.js App Router portals + BFF /api/proxy
- TypeScript contracts packages
- PostgreSQL (Supabase-compatible) is the target of truth — NOT shared SQL Server across SKUs
- SQL Server .NET + React SPA remain sources to migrate from; backend-nestjs stubs are starting points to restructure

Suite constraints (always true):
1. NO shared tables across products. Integrate via HTTP, webhooks, and domain events only.
2. No cross-SKU foreign keys. External ids are opaque UUID/text references.
3. No sibling DB credentials in service links — only baseUrl, apiKey hash, productTenantId, portalUrl.
4. Soft delete (deleted_at) + tenant_id isolation + audit trails are mandatory.
5. Never hard-delete money, stock ledger, call metadata, or audit rows.
6. Dual-write without outbox is forbidden. Money is never float.
7. JSON file stores (if any scaffold uses them) are not production truth — Postgres is.

Table prefixes (own database or own schema per SKU — never mixed physical tables):
plat_, rec_ (or existing REC schema), crm_, pos_, fin_, hrm_, inv_, prc_, mfg_, prj_, rpt_, cms_, out_, mbl_

Professional layout per product:
  <product>/
    api/           NestJS REST (/v1), domain modules, CQRS where justified
    portal/        Next.js App Router + BFF /api/proxy
    contracts/     @zallo/<sku>-contracts (DTOs, events, error codes)
    supabase/      migrations/, seeds/, policies/, functions/ (SQL)
    docs/          ARCHITECTURE.md, DATABASE.md, API.md, RUNBOOK.md

Database-first integrity:
- 3NF (BCNF where it reduces anomalies); intentional denormalization only with written rationale
- Surrogate UUIDs (uuid v7 preferred, else gen_random_uuid) + natural unique keys where business requires
- tenant_id on every tenant-owned row; composite indexes (tenant_id, …)
- Soft delete deleted_at + partial indexes (deleted_at IS NULL)
- CHECK constraints; FK ON DELETE RESTRICT or SET NULL with explicit rules
- Row Level Security by tenant
- Views for read models; materialized views only when justified
- Stored procedures / SECURITY DEFINER functions for multi-table transactions
- Outbox table per SKU for reliable integration events
- Optimistic concurrency (version column and/or xmin) on mutable aggregates
- Idempotency keys on webhooks and payment/sale/journal posts

Application layer:
- Thin controllers → application services / CQRS handlers → repositories OR stored procedures for atomic domain ops
- Commands = writes; Queries = side-effect-free reads (views/SPs)
- Structured logging + correlation ids + audit log for every mutation
- Permission codes enforced on every mutating endpoint and sensitive reads
- Password hashing: argon2id (verify legacy PBKDF2 during identity migration, rehash on login)
- Refresh tokens hashed at rest; rotation + reuse detection
- API keys hashed (rk_…); never plaintext secrets; OAuth tokens envelope-encrypted

==================================================
C. SKU MAP FROM THIS ERP → ZALLO
==================================================

| Zallo SKU | Ports (API/Portal) | Prefix | Maps from this repo | Notes |
|-----------|--------------------|--------|---------------------|--------|
| Platform (plat) | 4000 / 4001 | plat_ | Core + Admin + Audit + gateway/identity/admin/audit apps | Suite gateway; entitlements; handoff tokens; federated dashboard |
| REC | admin-api / client-api | rec_ | NEW (not in this ERP) | Inbound AI receptionist — design fully; do not share tables with OUT |
| CRM | 3100 / 3101 | crm_ | ERP CRM module + frontend/src/modules/crm | Richest .NET module; fix convert/invoice orchestration via events |
| POS | 3200 / 3201 | pos_ | Sales/POS + frontend pos | Separate from Finance invoices; SaleCompleted → CRM/FIN events |
| Finance | 3300 / 3301 | fin_ | ERP Finance | Accounting-grade; consume PRC bill-draft + POS/CRM invoice events |
| HRM | 3400 / 3401 | hrm_ | ERP HRM | PII + GDPR erase; interop ids for REC/SCH |
| Inventory | 3500 / 3501 | inv_ | Inventory + SCM merged | Single stock writer; ledger+balance only inside procs |
| Procurement | 3600 / 3601 | prc_ | ERP Procurement | 3-way match; outbox bill draft to FIN — no fin_ tables |
| Manufacturing | 3700 / 3701 | mfg_ | ERP Manufacturing + design BOM/routing | Async INV only; no dual stock writers |
| Projects | 3800 / 3801 | prj_ | ERP Projects | crm_customer_id / hrm_employee_id as external refs only |
| Reports | 3900 / 3901 | rpt_ | ERP Reports | Whitelisted query_key; no arbitrary SQL |
| AI CMS | 4100 / 4101 | cms_ | NEW | Knowledge sync → REC via outbox/events |
| Outbound OUT | 4300 / 4301 | out_ | NEW | Completely separate from REC inbound schema |
| Medical Billing MBL | 4200 / 4201 | mbl_ | NEW | Patient payments only — NOT claims/837/ERA |

Platform is the only identity/entitlement source. Products validate JWT/handoff/API keys and check entitlements via documented SECURITY DEFINER function contract or Platform HTTP — never by reading plat_ tables.

Resolve invoice trinity in design:
- CRM quote → sales order stays in crm_
- Commercial sale completion / register sale lives in pos_
- Statutory AR lives in fin_ (created via event/API from POS or CRM, not shared table)
- PRC purchase invoice stays in prc_; FIN AP bill created via event

==================================================
D. PER-SKU DOMAIN REQUIREMENTS (DESIGN ALL)
==================================================

----- 1. PLATFORM (plat_) -----
Purpose: Suite gateway — org tenants, companies, members, roles/permissions, entitlements (SKU flags), service links to sibling products, auth login/session, single-use handoff tokens, audit log, billing surface (plan/SKU metadata only; Stripe optional), federated suite dashboard aggregating entitled product overview metrics.

Normalize: tenants, companies, users/members, roles, permissions, role_permissions, entitlements, service_links, sessions, handoff_tokens, audit_events, billing_plans, subscription_snapshots.

Special:
- Handoff tokens: single-use, short TTL, hashed at rest, atomic redeem via stored procedure
- Entitlement checks as SECURITY DEFINER function; document for all products
- Service link registry modes native|read|sync; store baseUrl, apiKey hash, productTenantId, portalUrl only
- Federated dashboard: async fan-out with timeouts; cache overview payloads (TTL + tenant key)
- Soft-delete membership without orphaning audit history
- Port .NET setup super-admin, auth flows, tenant CRUD, users/roles/permissions/settings
- Expand permission catalog to all SKUs (not Core-only)
- Migrate JWT/refresh/lockout/session behavior; strengthen vs current .NET

----- 2. REC (rec_) — AI inbound receptionist -----
Purpose: Dialed-number → tenant → Vapi assistant; booking/FAQ/transfer tools; transcripts & call outcomes; clinic CRM-lite (contacts, pipeline, activities) OR event-out to Zallo CRM; SMS inbox/sequences; knowledge base; team RBAC (~90 permission codes); Stripe billing/POS Connect; phone inventory; applications queue; calendar OAuth; public booking links.

Layout:
  receptionist/
    admin-api/ client-api/
    admin-portal/ client-portal/
    supabase/ (or under client-api)
    marketing site at package root allowed

Special:
- RLS by tenant_id; platform_users for super_admin/support
- Voice webhook authenticity (provider signatures) + idempotent end-of-call write
- Outbox for Platform link-activated mirroring and sibling CRM/MBL events
- Booking concurrency: exclusion constraints or advisory locks in procs
- Soft deletes + immutable call/recording metadata
- Separate analytics read models (optional matviews) vs OLTP booking
- API key hashing (rk_…); never plaintext
- CQRS: show when commands call stored procs vs domain services
- Proc catalog must include: create_appointment, reschedule, cancel, apply_sms_sequence_step, finalize_call, grant_role_permissions
- Do not invent shared tables with OUT, CRM, or MBL

----- 3. CRM (crm_) -----
Purpose: Customers, contacts (import/export/merge/dedupe), leads → convert, opportunities (pipeline/forecast), quotes → sales orders, activities, campaigns, tickets, tags/segments/custom fields, conversations, workflows, calendars/bookings, public forms (token submit), funnels, reputation reviews, channels, webhooks, API keys, GDPR erase, migration helpers, inbound webhooks from REC/POS.

Port existing .NET CRM entities/DTOs/validators/workflows as the baseline feature set, then extend to tickets/tags/segments/custom fields/webhooks/GDPR as specified.

Special:
- Choose party model vs separate customer/contact — justify, 3NF
- Lead convert + quote→order as single stored procedures (transactional, audit, outbox)
- Dedupe merge: surviving/losing records, re-point FKs, soft-delete loser
- Custom fields: EAV vs JSONB+GIN — pick with indexing strategy
- GDPR erase: anonymize PII, retain financial/audit stubs
- Webhook ingress idempotency table
- Segments: saved queries vs materialized membership + refresh strategy
- Do not call Finance/Inventory DB; emit events (OrderApproved, OrderInvoicedRequested, etc.)

----- 4. POS (pos_) -----
Purpose: Registers, open/close sessions, products (projection or link), inventory view (link to INV), customers (projection/external id), carts (hold), complete sale with tenders, sales history, sales invoices, SCH collect-payment webhook, notify CRM on sale completed.

Port .NET SalesInvoice + POSSession + POSTransaction as starting aggregates.

Special:
- Session open/close and sale completion MUST be stored procedures (tender split, invoice sequences per tenant; optional stock reservation via INV event/API)
- Money: pick numeric(19,4) OR integer minor units — enforce everywhere (prefer integer minor units or numeric(19,4); never float)
- Idempotent sale post by client_sale_id
- Register session cash variance & tender reconciliation
- Hold carts with TTL and exclusive lock
- Outbox: SaleCompleted → CRM; payment webhooks → mark invoice paid; optional AR draft → FIN
- Read-only inventory projection vs live INV service link — document consistency model

----- 5. FINANCE (fin_) -----
Purpose: Currencies, CoA, journals (draft/post), AR invoices, AP bills, payments (apply), bank accounts, service links, Platform link-activated webhook.

Port .NET Finance entities; replace stubs with real posting rules.

Special:
- Double-entry: journal header + lines; post proc enforces Σ debit = Σ credit, period open, account active
- Immutable posted journals (reversing entries only)
- AR/AP apply-payment proc with partial applications and remaining balance
- Multi-currency: currency_code + fx_rate + functional amount
- Fiscal periods / locking
- Bank balances derived from ledger — no dual-write without reconciliation
- Document sequences INV-/BILL-/PAY- per tenant; document gapless vs near-gapless tradeoffs
- Accounting-grade constraints over app-only validation

----- 6. HRM (hrm_) -----
Purpose: Departments, employees (interop port APIs), addresses, documents, contracts, leave types/balances/requests (approve/reject/cancel), payrolls (post), trainings, performance reviews, time entries, settings, GDPR erase, migration import, SCH provider-changed webhook.

Special:
- Leave approve/reject/cancel procs updating balances; CHECK (balance >= 0) or tenant allowed-negative flag
- Payroll post: freeze snapshot; immutable after post; outbox JournalRequested → FIN
- Employee interop ports: stable external ids for SCH/REC
- Document metadata + virus-scan status; object storage key, not blobs
- GDPR erase anonymization procedure
- Org hierarchy (department tree) with cycle prevention
- PII minimization + access logging
- Design a real Department model (missing/stub in .NET)

----- 7. INVENTORY (inv_) -----
Purpose: Items, categories, warehouses, UoM (+ conversions), stock balances, transfers (complete), adjustments (post), service links. Merge .NET Inventory+SCM; drop duplicate service paths.

Special:
- Immutable stock ledger + balance table updated ONLY inside stored procedures
- Transfer complete & adjustment post with row locks (SELECT … FOR UPDATE)
- UoM conversion table; store base UoM qty on ledger
- Never negative stock unless tenant setting allows — enforce in proc
- Outbox StockChanged → POS/MFG/PRC
- ABC/reorder optional read models — not OLTP critical path
- Batch/serial/barcode/reorder/valuation from .NET SCM must be modeled

----- 8. PROCUREMENT (prc_) -----
Purpose: Vendors, requisitions, RFQs (send), POs (approve, create GRN), goods receipts (post, create invoice), purchase invoices.

Special:
- State machines: requisition → RFQ → PO → GRN → invoice — legal transitions in procs
- PO approve + GRN post; 3-way match fields (PO/GRN/invoice qty & amount)
- Partial receipts; over-receipt policy flag
- Create purchase invoice from GRN atomically + outbox to Finance (bill draft event) — no shared fin_ tables
- Vendor master normalization (addresses, contacts, payment terms)
- Replace .NET stub PurchaseOrderService with real behavior

----- 9. MANUFACTURING (mfg_) -----
Purpose: Work centers, production orders (release/start/complete), production runs, BOM/routing (design even if .NET scaffold thin), service links.

Special:
- Lifecycle procs: release (reserve components via INV API/event), start, complete (finished goods receipt event)
- BOM explosion proc or service; versioned BOM headers/lines
- Work center capacity calendars (phase 2 tables still modeled)
- Scrap/yield on complete
- Idempotent run reporting
- Async INV only — Inventory is the only stock writer

----- 10. PROJECTS (prj_) -----
Purpose: Projects, tasks (complete), milestones (reach), resources, CRM customer id references (external only), service links.

Special:
- Task graph: parent/child, dependency edges, cycle detection on write
- Milestone reach proc (all required tasks complete or manual override + audit)
- Resource allocation without overlapping bookings (exclusion constraint or proc)
- Time-rollups via views; estimates vs actuals
- external crm_customer_id / hrm_resource_id as UUID text — NOT cross-DB FKs
- Project documents → object storage metadata

----- 11. REPORTS (rpt_) -----
Purpose: Report definitions (query_key + output format), execute (parameterized), schedules (cron), execution history.

Special:
- Whitelist query_key → registered SQL views/functions — NO arbitrary client SQL
- Execute via SECURITY DEFINER functions that force tenant_id
- Schedules + history: row_count, duration_ms, status, error, artifact_uri
- Concurrent schedule fire: lease/lock row so only one worker runs
- Export csv/xlsx/pdf as jobs table; never block API on large runs
- Optional federation connectors to sibling overview APIs — document rate limits
- Injection safety + tenant isolation emphasized

----- 12. AI CMS (cms_) -----
Purpose: Brand voices, campaigns, briefs, posts (submit review / approve / publish / sync knowledge), AI generate/repurpose/images/A-B variants, SEO score, channels + OAuth, publish jobs + schedule worker, calendar, assets, Studio/Workflow UI.

Special:
- Content state machine in proc: draft → in_review → approved → scheduled → published → archived
- Publish jobs with retries, backoff, idempotent provider publish keys
- OAuth tokens envelope-encrypted at rest
- Assets metadata normalized; binaries in object storage
- Outbox “sync knowledge” → REC FAQ when linked
- AI provider call audit (prompt hash, model, tokens, cost) without storing secrets
- Calendar as projection of scheduled posts

----- 13. OUTBOUND (out_) -----
Purpose: Standalone AI outbound dialer (not Receptionist). Campaigns with AI-disclosure scripts, CSV import with consent, calling windows + DNC, Vapi dial (or simulate), bookings/usage metering, CrmAdapter (Zallo CRM + GoHighLevel), speed-to-lead webhooks.

Special:
- Consent & DNC first-class tables; dial eligibility CHECKED inside start_dial proc
- Campaign contact queue with SKIP LOCKED worker pattern
- Calling windows in tenant TZ; timestamptz + window rules
- Idempotent webhook ingest (Vapi status, speed-to-lead)
- Usage metering ledger (minutes, dials) for billing
- CrmAdapter anti-corruption layer — map external contact ids; no shared CRM schema
- TCPA-oriented audit (disclosure played, consent source, timestamp)
- Completely separate from REC inbound schema

----- 14. MEDICAL BILLING (mbl_) -----
Purpose: Independent patient-payments SKU (NOT insurance claims/coding): service catalog + deposits, invoices, packages, Stripe Connect pay links, SCH deposit/fee webhooks, CON PMS charge-sync webhook. Voice tools (getPriceEstimate, getPatientBalance, sendPaymentLink) call MBL when configured.

Special:
- Patient balances derived from ledger of charges, deposits, payments, adjustments
- send_payment_link + record_stripe_payment procs (idempotent by stripe event id)
- Packages: prepaid units remaining; consume atomically
- Price estimate read model from catalog + optional modifiers
- PHI minimization; encryption for sensitive identifiers
- Webhooks SCH/CON/REC: signature verify + idempotency
- Connect account mapping per tenant clinic
- Explicitly exclude claims/837/ERA unless listed as future BC

==================================================
E. CROSS-CUTTING STANDARDS (SUITE PLAYBOOK)
==================================================

Standardize across all SKUs:
- Tenancy: tenant_id + RLS + JWT tenant claim; SuperAdmin impersonation via Platform handoff + audit
- AuthZ: permission codes `<sku>.<resource>.<action>` enforced in API guards AND optional in SECURITY DEFINER procs
- Outbox: out_<sku> local table name still prefixed with sku_ actually use `{prefix}outbox` e.g. crm_outbox — include aggregate_type, aggregate_id, event_type, payload jsonb, headers, created_at, published_at, attempts
- Idempotency: `{prefix}idempotency_keys` (tenant_id, key, request_hash, response_status, response_body, expires_at)
- Audit: `{prefix}audit_events` (who, action, entity, entity_id, old/new jsonb redacted, ip, ua, correlation_id, at)
- Entity history for money/PII/security aggregates (version timeline + field diffs)
- Soft delete: deleted_at timestamptz; unique constraints must be partial WHERE deleted_at IS NULL
- Error codes: stable machine codes in contracts packages
- Correlation ids: inbound header X-Correlation-Id; propagate to logs, audit, outbox headers
- Observability labels: sku, tenant_id, correlation_id, permission, route
- Migration naming: `YYYYMMDDHHMMSS__<sku>__<action>.sql`
- Proc naming: `<sku>_<action>_<entity>` e.g. fin_post_journal, pos_complete_sale, crm_convert_lead
- OpenAPI: /v1, problem+json or suite envelope `{ success, data, errors[{code,message,field}], meta{correlationId,timestamp} }`
- Pagination: { items, page, pageSize, totalCount }
- Forbidden anti-patterns: cross-SKU FKs, shared “core” schema, dual writes without outbox, money as float/double, app-only stock/ledger updates, plaintext API keys/refresh tokens, AllowAll CORS, arbitrary SQL from clients

==================================================
F. MIGRATION FROM CURRENT SCAFFOLD
==================================================

Phased cutover (zero-downtime where possible):

Phase 0 — Suite playbook + Platform Postgres + contracts + gateway.
  Restructure backend-nestjs stubs toward zallo/product layout (or apps/<sku> if staying one monorepo — choose one layout, justify, and show both the Zallo <product>/ tree and how it maps from backend-nestjs/).
  Do not keep a god DbContext.

Phase 1 — Platform identity/admin/audit replacing .NET Core/Admin. Dual-run: React SPA → new Platform /api compatibility routes.

Phase 2 — CRM (richest). Event contracts ready before POS/FIN split.

Phase 3 — Inventory (single stock writer) then POS + Procurement.

Phase 4 — Finance (consume events; replace stubs).

Phase 5 — HRM, Manufacturing, Projects, Reports.

Phase 6 — New SKUs: REC, MBL, OUT, CMS (can parallel after Platform entitlements + service links exist).

Phase 7 — Decommission SQL Server ERP.Api + EF. Frontend: either keep React behind BFF temporarily or replace per-SKU with Next.js portals.

Data migration:
- Map SQL Server tables → Postgres prefixed tables
- Preserve Guids where possible
- IsActive=false → deleted_at or status; document mapping
- Rehash passwords on login; migrate PBKDF2 verifier
- Backfill outbox none; start outbox at cutover
- Tenant-by-tenant or big-bang per SKU with read-only dual-write window documented

Risk rank JSON/SQL Server scaffolds vs stub modules vs new voice/billing SKUs.

==================================================
G. REQUIRED OUTPUT (PRODUCE ALL, IN THIS ORDER)
==================================================

0. One-page suite thesis + SKU map + anti-pattern list
1. Suite C4 context diagram (mermaid) + integration matrix (who calls whom, events, sync mode native|read|sync)
2. Then for EACH SKU (Platform first, then CRM, POS, INV, FIN, PRC, HRM, MFG, PRJ, RPT, then REC, MBL, OUT, CMS):
   A. C4 context + container diagrams (mermaid)
   B. Full repo tree with file responsibilities
   C. Normalized ERD (mermaid erDiagram) + SQL DDL migrations (tables, indexes, constraints, RLS policies). MUST cover every existing domain model listed above for that SKU, plus required new tables (outbox, idempotency, audit, history).
   D. Stored procedures / functions catalog (name, params, invariants, error codes) — include outbox publish helpers
   E. Nest module map + key endpoints (/v1 and /api compatibility if Platform/CRM/POS)
   F. Portal information architecture (Next.js routes) + mapping from current React routes where they exist
   G. Integration contracts (webhooks/events/OpenAPI sketches) — no shared DB
   H. Security model (authn/authz/RLS/API keys/handoff)
   I. Observability + backup/DR notes
   J. Phased migration plan from current .NET/SQL Server (and any JSON store) → Postgres
3. Suite-wide ADR: cross-cutting patterns, engineering playbook, cutover order, risk ranking
4. Explicit diff vs current ERP: what is ported, what is stub-replaced, what is new, what is dropped (GraphQL? god context? in-process cross-module writes?)

Style: precise, senior, no fluff. Prefer concrete SQL, proc names, folder names, and event names over slogans.
If a choice exists (party model, money representation, gapless sequences, monorepo vs polyrepo products), pick one, justify in 3–6 lines, and proceed consistently.

Start now with section 0–1 (suite), then Platform complete A–J, then CRM, POS, INV, FIN. Continue through all SKUs without stopping early.
```

---

## SKU follow-up prompts (after master output)

Use when a single product needs a deeper pass. Always keep the master constraints (no shared tables, prefixes, RLS, outbox, Postgres).

### Platform

```text
Continue from the Zallo complete architecture. Deepen PLATFORM only (plat_, ports 4000/4001).
Maximize DDL, RLS, handoff redeem proc, entitlement SECURITY DEFINER function, service_links, federated dashboard caching.
Diff against D:\Projects\erp Core/Admin/Audit + backend-nestjs apps gateway/identity/admin/audit.
Produce A–J only for Platform. Prefix plat_.
```

### REC

```text
Deepen REC only. Assume Postgres/Supabase is live. Focus production hardening + procs:
create_appointment, reschedule, cancel, apply_sms_sequence_step, finalize_call, grant_role_permissions.
No shared tables with OUT/CRM/MBL. Produce A–J. Prefix rec_.
```

### CRM

```text
Deepen CRM only (crm_, 3100/3101). Diff against ERP.Domain/CRM, ERP.Application/CRM, ERP.Shared/DTOs/CRM, docs/CRM_MODULE_STATUS.md.
Lead convert + quote→order as procs. Merge/dedupe + GDPR erase. Events to POS/FIN/REC only. Produce A–J.
```

### POS

```text
Deepen POS only (pos_, 3200/3201). Diff against ERP Sales/POS.
Session open/close + complete_sale MUST be procs. Idempotent client_sale_id. Money type consistent with suite ADR.
Outbox SaleCompleted. Inventory is read model or INV link — no dual stock writer. Produce A–J.
```

### Finance

```text
Deepen FINANCE only (fin_, 3300/3301). Diff against ERP.Domain/Finance + stub services.
Double-entry post/reverse, apply payment, fiscal lock, multi-currency, document sequences.
No shared prc_/pos_ tables. Produce A–J.
```

### HRM

```text
Deepen HRM only (hrm_, 3400/3401). Diff against ERP.Domain/HRM.
Leave + payroll procs, department tree cycle prevention, GDPR erase, document object-storage metadata, access logging. Produce A–J.
```

### Inventory

```text
Deepen INV only (inv_, 3500/3501). Merge Inventory+SCM from .NET; delete duplicate conceptual paths.
Ledger+balance only inside procs. Transfer complete + adjustment post with FOR UPDATE. StockChanged outbox. Produce A–J.
```

### Procurement

```text
Deepen PRC only (prc_, 3600/3601). State machine procs, 3-way match, partial/over-receipt policy, GRN→invoice + fin bill-draft event. Produce A–J.
```

### Manufacturing

```text
Deepen MFG only (mfg_, 3700/3701). Design BOM/routing even if .NET is thin. Release/start/complete procs. INV is only stock writer. Produce A–J.
```

### Projects

```text
Deepen PRJ only (prj_, 3800/3801). Task graph cycle detection, milestone reach proc, resource exclusion, external crm_customer_id only. Produce A–J.
```

### Reports

```text
Deepen RPT only (rpt_, 3900/3901). Whitelisted query_key, SECURITY DEFINER execute, schedule leases, async export jobs. Injection + tenant isolation. Produce A–J.
```

### AI CMS

```text
Deepen CMS only (cms_, 4100/4101). Content state machine proc, publish jobs, encrypted OAuth, AI usage audit, knowledge-sync outbox to REC. Produce A–J.
```

### Outbound

```text
Deepen OUT only (out_, 4300/4301). Consent/DNC, start_dial eligibility proc, SKIP LOCKED queue, usage ledger, CrmAdapter ACL, TCPA audit. Separate from REC. Produce A–J.
```

### Medical Billing

```text
Deepen MBL only (mbl_, 4200/4201). Patient payment ledger, send_payment_link + record_stripe_payment, packages, PHI minimization. Exclude claims/837/ERA. Produce A–J.
```

### Suite review (after 2–3 SKUs exist)

```text
Review the Zallo multi-product suite as a principal architect (Platform, REC, CRM, POS, FIN, HRM, INV, PRC, MFG, PRJ, RPT, CMS, OUT, MBL).

1. Suite C4 + integration matrix (calls, events, sync mode).
2. Standardize tenancy, API keys, outbox, idempotency, soft delete, audit, error codes, correlation ids.
3. Shared engineering playbook (not shared DB): OpenAPI, migration naming, proc naming `<sku>_<action>_<entity>`, observability labels.
4. Forbid: cross-SKU FKs, shared core schema, dual writes without outbox, money as float.
5. Phased Postgres cutover from D:\Projects\erp SQL Server + backend-nestjs stubs + any JSON stores, with risk ranking.

Output one ADR-style document + mermaid. No code dumps unless illustrating a standard.
```

---

## Prompt modifiers (append one line)

| Goal | Add this line |
|------|----------------|
| SQL only | Skip Nest/Next code; maximize DDL, procs, indexes, RLS. |
| Repo scaffold | Generate the full folder tree and stub file list ready for `nest new` / `create-next-app`. |
| Review existing | Diff against current `D:\Projects\erp` and `backend-nestjs/`; list gaps as a prioritized backlog. |
| Interview/whiteboard | Keep answer to 2 pages; focus on suite C4 + Platform ERD + 5 critical stored procedures. |
| Security audit | Threat-model STRIDE for multi-tenant SaaS; map mitigations to RLS/procs/API. |
| Implement next | After design, implement Platform Phase 0–1 in repo without breaking .NET until compatibility routes exist. |

---

## How to run a design session

1. New chat → paste **Master prompt**.
2. If output stops early: “Continue from SKU \<name\>, complete A–J, then proceed in the stated order.”
3. Deepen with a **SKU follow-up**.
4. After 3+ SKUs: paste **Suite review**.
5. When ready to build: append “Implement next” and point at `backend-nestjs/` + new `zallo/` (or agreed monorepo layout).
