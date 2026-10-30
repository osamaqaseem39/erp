using Microsoft.EntityFrameworkCore;
using ERP.Domain.Core;
using ERP.Domain.HRM;
using ERP.Domain.Finance;
using ERP.Domain.CRM;
using ERP.Domain.Admin;
using ERP.Domain.Manufacturing;
using ERP.Domain.Projects;
using ERP.Domain.Procurement;
using ERP.Domain.Reports;
using ERP.Domain.Sales;
using ERP.Domain.SCM;

namespace ERP.Infrastructure.Database;

public class ERPDbContext : DbContext
{
    public ERPDbContext(DbContextOptions<ERPDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }
    public DbSet<AdminEmail> AdminEmails { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<PaymentTerm> PaymentTerms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<DocumentSequence> DocumentSequences { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // HRM Entities
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeAddress> EmployeeAddresses { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<EmploymentContract> EmploymentContracts { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<PayrollLine> PayrollLines { get; set; }
    public DbSet<PayrollComponent> PayrollComponents { get; set; }
    public DbSet<EmployeeSalaryComponent> EmployeeSalaryComponents { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Benefit> Benefits { get; set; }
    public DbSet<EmployeeBenefitEnrollment> EmployeeBenefitEnrollments { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<PerformanceReview> PerformanceReviews { get; set; }
    public DbSet<JobOpening> JobOpenings { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<Training> Trainings { get; set; }
    public DbSet<TrainingEnrollment> TrainingEnrollments { get; set; }
    public DbSet<HRSettings> HRSettings { get; set; }
    public DbSet<EmployeeAdvance> EmployeeAdvances { get; set; }
    public DbSet<Team> Teams { get; set; }

    // Finance Entities
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<BillLine> BillLines { get; set; }
    public DbSet<Journal> Journals { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLine> InvoiceLines { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<BankTransaction> BankTransactions { get; set; }
    public DbSet<BankReconciliation> BankReconciliations { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<AccountDefault> AccountDefaults { get; set; }
    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<TaxCode> TaxCodes { get; set; }
    public DbSet<TaxRate> TaxRates { get; set; }
    public DbSet<PaymentApplication> PaymentApplications { get; set; }
    public DbSet<DepreciationEntry> DepreciationEntries { get; set; }
    public DbSet<IntercompanyTransaction> IntercompanyTransactions { get; set; }
    public DbSet<OpeningBalance> OpeningBalances { get; set; }
    public DbSet<OpeningBalanceLine> OpeningBalanceLines { get; set; }
    public DbSet<AccountingPeriod> AccountingPeriods { get; set; }
    public DbSet<ChequeBook> ChequeBooks { get; set; }
    public DbSet<ChequeLeaf> ChequeLeaves { get; set; }
    public DbSet<ExpenseClaim> ExpenseClaims { get; set; }
    public DbSet<ExpenseClaimLine> ExpenseClaimLines { get; set; }
    public DbSet<TaxTransaction> TaxTransactions { get; set; }
    public DbSet<BudgetLine> BudgetLines { get; set; }
    public DbSet<TaxGroup> TaxGroups { get; set; }
    public DbSet<TaxGroupLine> TaxGroupLines { get; set; }
    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<AssetDisposal> AssetDisposals { get; set; }
    public DbSet<AssetTransfer> AssetTransfers { get; set; }
    public DbSet<BankReconciliationLine> BankReconciliationLines { get; set; }
    public DbSet<FundTransfer> FundTransfers { get; set; }
    public DbSet<CreditMemo> CreditMemos { get; set; }
    public DbSet<CreditMemoLine> CreditMemoLines { get; set; }
    public DbSet<DebitMemo> DebitMemos { get; set; }
    public DbSet<DebitMemoLine> DebitMemoLines { get; set; }
    public DbSet<CustomerAdvance> CustomerAdvances { get; set; }
    public DbSet<VendorAdvance> VendorAdvances { get; set; }
    public DbSet<AdvanceApplication> AdvanceApplications { get; set; }
    public DbSet<WithholdingTaxLine> WithholdingTaxLines { get; set; }
    public DbSet<RecurringJournal> RecurringJournals { get; set; }
    public DbSet<RecurringJournalLine> RecurringJournalLines { get; set; }
    public DbSet<FxRevaluationRun> FxRevaluationRuns { get; set; }
    public DbSet<FxRevaluationLine> FxRevaluationLines { get; set; }
    public DbSet<IntercompanySettlement> IntercompanySettlements { get; set; }

    // CRM Entities
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CustomerGroup> CustomerGroups { get; set; }
    public DbSet<PriceList> PriceLists { get; set; }
    public DbSet<SalesTerritory> SalesTerritories { get; set; }
    public DbSet<Salesman> Salesmen { get; set; }
    public DbSet<ReturnOrder> ReturnOrders { get; set; }
    public DbSet<ReturnOrderLine> ReturnOrderLines { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceLine> SalesInvoiceLines { get; set; }
    public DbSet<SalesCreditNote> SalesCreditNotes { get; set; }
    public DbSet<SalesCreditNoteLine> SalesCreditNoteLines { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteLine> QuoteLines { get; set; }
    public DbSet<PouchType> PouchTypes { get; set; }
    public DbSet<ProductionProcess> ProductionProcesses { get; set; }
    public DbSet<FOHType> FOHTypes { get; set; }
    public DbSet<SaleEnquiry> SaleEnquiries { get; set; }
    public DbSet<SaleEnquiryLine> SaleEnquiryLines { get; set; }
    public DbSet<SalePreCost> SalePreCosts { get; set; }
    public DbSet<SalePreCostLine> SalePreCostLines { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

    // Manufacturing Entities
    public DbSet<WorkCenter> WorkCenters { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionOrderLine> ProductionOrderLines { get; set; }
    public DbSet<ProductionRun> ProductionRuns { get; set; }
    public DbSet<BillOfMaterial> BillOfMaterials { get; set; }
    public DbSet<BillOfMaterialLine> BillOfMaterialLines { get; set; }
    public DbSet<Routing> Routings { get; set; }
    public DbSet<RoutingStep> RoutingSteps { get; set; }
    public DbSet<ProductionScrap> ProductionScraps { get; set; }
    public DbSet<ScrapReason> ScrapReasons { get; set; }
    public DbSet<ProductionOrderOperation> ProductionOrderOperations { get; set; }
    public DbSet<QualityCheck> QualityChecks { get; set; }
    public DbSet<QualityCheckLine> QualityCheckLines { get; set; }

    // Project Entities
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ProjectResource> ProjectResources { get; set; }
    public DbSet<ProjectDocument> ProjectDocuments { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
    public DbSet<ProjectTimesheet> ProjectTimesheets { get; set; }
    public DbSet<ProjectTimesheetLine> ProjectTimesheetLines { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }

    // Procurement Entities
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorBankAccount> VendorBankAccounts { get; set; }
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
    public DbSet<GoodsReceiptNoteDetail> GoodsReceiptNoteDetails { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<PurchaseReturnLine> PurchaseReturnLines { get; set; }
    public DbSet<PurchaseDebitNote> PurchaseDebitNotes { get; set; }
    public DbSet<PurchaseDebitNoteLine> PurchaseDebitNoteLines { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<PurchaseRequisitionDetail> PurchaseRequisitionDetails { get; set; }
    public DbSet<RequestForQuotation> RequestForQuotations { get; set; }
    public DbSet<RFQDetail> RFQDetails { get; set; }
    public DbSet<RFQVendorResponse> RFQVendorResponses { get; set; }
    public DbSet<RFQVendorResponseDetail> RFQVendorResponseDetails { get; set; }
    public DbSet<LandedCostVoucher> LandedCostVouchers { get; set; }
    public DbSet<LandedCostVoucherLine> LandedCostVoucherLines { get; set; }
    public DbSet<VendorContract> VendorContracts { get; set; }

    // Sales/POS Entities
    public DbSet<POSSession> POSSessions { get; set; }
    public DbSet<POSTransaction> POSTransactions { get; set; }
    public DbSet<POSTransactionLine> POSTransactionLines { get; set; }
    public DbSet<DispatchAdvice> DispatchAdvices { get; set; }
    public DbSet<DispatchAdviceLine> DispatchAdviceLines { get; set; }

    // SCM/Inventory Entities
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemCategory> ItemCategories { get; set; }
    public DbSet<ItemGroup> ItemGroups { get; set; }
    public DbSet<ItemAttribute> ItemAttributes { get; set; }
    public DbSet<ItemVariant> ItemVariants { get; set; }
    public DbSet<ItemVariantAttribute> ItemVariantAttributes { get; set; }
    public DbSet<ItemPricing> ItemPricings { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockBalance> StockBalances { get; set; }
    public DbSet<StockLedger> StockLedgers { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferDetail> StockTransferDetails { get; set; }
    public DbSet<StockAdjustment> StockAdjustments { get; set; }
    public DbSet<StockAdjustmentLine> StockAdjustmentLines { get; set; }
    public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }
    public DbSet<InventoryValuationMethod> InventoryValuationMethods { get; set; }
    public DbSet<ReorderRule> ReorderRules { get; set; }
    public DbSet<Barcode> Barcodes { get; set; }
    public DbSet<BatchNumber> BatchNumbers { get; set; }
    public DbSet<SerialNumber> SerialNumbers { get; set; }
    public DbSet<BatchSerialTracking> BatchSerialTrackings { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
    public DbSet<StockReservation> StockReservations { get; set; }
    public DbSet<StockReservationLine> StockReservationLines { get; set; }
    public DbSet<ItemKit> ItemKits { get; set; }
    public DbSet<ItemKitComponent> ItemKitComponents { get; set; }
    public DbSet<CycleCount> CycleCounts { get; set; }
    public DbSet<CycleCountLine> CycleCountLines { get; set; }
    public DbSet<PutawayTask> PutawayTasks { get; set; }
    public DbSet<PutawayTaskLine> PutawayTaskLines { get; set; }
    public DbSet<PickList> PickLists { get; set; }
    public DbSet<PickListLine> PickListLines { get; set; }
    public DbSet<ReplenishmentTask> ReplenishmentTasks { get; set; }
    public DbSet<ReplenishmentTaskLine> ReplenishmentTaskLines { get; set; }
    public DbSet<PackOrder> PackOrders { get; set; }
    public DbSet<PackOrderLine> PackOrderLines { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentLine> ShipmentLines { get; set; }

    // Report Entities
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportExecution> ReportExecutions { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tenant (Core schema)
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants", "Core");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Domain).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionString).IsRequired();
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        // Configure Plan (Core schema)
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.ToTable("Plans", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlanName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.FeaturesJSON).HasMaxLength(4000);
        });

        // Configure Subscription (Core schema)
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LegalName).HasMaxLength(200);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.MobilePhone).HasMaxLength(30);
            entity.Property(e => e.ContactPerson).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Ignore(e => e.CompanyCode);
            entity.Ignore(e => e.TaxId);
            entity.Ignore(e => e.Address);
            entity.Ignore(e => e.PostalCode);
            entity.Ignore(e => e.Currency);
        });

        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.ToTable("FiscalYears", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => new { e.TenantId, e.FiscalYearId });
            entity.HasIndex(e => new { e.TenantId, e.StartDate, e.EndDate });
        });

        modelBuilder.Entity<AdminEmail>(entity =>
        {
            entity.ToTable("AdminEmails", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.Email });
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.TenantId, e.Code });
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("Cities", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.TenantId, e.CountryId });
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.HasIndex(e => new { e.TenantId, e.Code });
        });

        modelBuilder.Entity<PaymentTerm>(entity =>
        {
            entity.ToTable("PaymentTerms", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.TenantId, e.Name });
        });

        // Configure Role (Core schema)
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            // DB column is RoleName only — Name/Notes are app-layer aliases
            entity.Ignore(e => e.Name);
            entity.Ignore(e => e.Notes);
        });

        // Configure User (Core schema)
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Salutation).HasMaxLength(10);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Nationality).HasMaxLength(100);
            entity.Property(e => e.PrimaryEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SecondaryEmail).HasMaxLength(200);
            entity.Property(e => e.PrimaryPhone).HasMaxLength(20);
            entity.Property(e => e.SecondaryPhone).HasMaxLength(20);
            entity.Property(e => e.MobilePhone).HasMaxLength(20);
            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.ProfilePicture).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Fax).HasMaxLength(20);
            entity.Property(e => e.LinkedIn).HasMaxLength(200);
            entity.Property(e => e.Twitter).HasMaxLength(200);
            entity.Property(e => e.Facebook).HasMaxLength(200);
            entity.Property(e => e.Instagram).HasMaxLength(200);
            entity.Property(e => e.PreferredCommunicationMethod).HasMaxLength(20);
            entity.Property(e => e.LanguagePreference).HasMaxLength(10);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PasswordSalt).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(128);
            entity.Property(e => e.TwoFactorPendingSecret).HasMaxLength(128);
            entity.Property(e => e.TwoFactorChallengeTokenHash).HasMaxLength(128);
            entity.HasIndex(e => e.PrimaryEmail).IsUnique();
            entity.HasIndex(e => e.EmployeeId)
                .IsUnique()
                .HasFilter("[EmployeeId] IS NOT NULL");
            
            // Tenant relationship will be handled by TenantId foreign key
                
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserRole (Core schema)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignedAt).HasColumnName("AssignedDate");
            entity.Ignore(e => e.AssignedBy);
            entity.Ignore(e => e.ExpiresAt);
            entity.Ignore(e => e.IsActive);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(64).IsUnicode(false);
            entity.Property(e => e.RevokedReason).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(64);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.SessionId });
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Permission (Core schema)
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ActionName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Ignore(e => e.Name);
            entity.Ignore(e => e.Module);
            entity.Ignore(e => e.Action);
            entity.Ignore(e => e.Notes);
        });

        // Configure RolePermission (Core schema)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions", "Core");
            entity.HasKey(e => e.Id);
            // DB has IsAllowed and no assignment columns.
            entity.Ignore(e => e.AssignedAt);
            entity.Ignore(e => e.AssignedBy);
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuditLog (Core schema)
        modelBuilder.Entity<DocumentSequence>(entity =>
        {
            entity.ToTable("DocumentSequences", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SequenceKey).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Prefix).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ResetPolicy).IsRequired().HasMaxLength(10);
            // One sequence per key per tenant; the atomic increment relies on this.
            entity.HasIndex(e => new { e.TenantId, e.SequenceKey }).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OldValues).HasMaxLength(4000);
            entity.Property(e => e.NewValues).HasMaxLength(4000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.AuditLogs)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Setting (Core schema)
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Settings", "Core");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SettingValue).HasMaxLength(1000);
            
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Settings)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure HRM Entities
        ConfigureHRMEntities(modelBuilder);
        
        // Configure Finance Entities
        ConfigureFinanceEntities(modelBuilder);
        
        // Configure SCM/Inventory Entities
        ConfigureSCMEntities(modelBuilder);
        
        // Configure remaining entities with minimal configuration to prevent migration errors
        ConfigureRemainingEntities(modelBuilder);
    }

    private void ConfigureHRMEntities(ModelBuilder modelBuilder)
    {
        // Configure Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Salutation).HasMaxLength(10);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PreferredName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(5);
            entity.Property(e => e.NationalId).HasMaxLength(100);
            entity.Property(e => e.Nationality).HasMaxLength(100);
            entity.Property(e => e.MaritalStatus).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(150);
            entity.Property(e => e.EmploymentType).HasMaxLength(50);
            entity.Property(e => e.WorkEmail).HasMaxLength(255);
            entity.Property(e => e.PersonalEmail).HasMaxLength(255);
            entity.Property(e => e.WorkPhone).HasMaxLength(50);
            entity.Property(e => e.MobilePhone).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Active");
            
            entity.HasIndex(e => new { e.TenantId, e.EmployeeCode }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.WorkEmail });
            
            entity.HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure EmployeeAddress
        modelBuilder.Entity<EmployeeAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AddressType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.Addresses)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EmployeeDocument
        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure EmploymentContract
        modelBuilder.Entity<EmploymentContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContractType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PayFrequency).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ContractDocumentUrl).HasMaxLength(1000);
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WorkingHoursPerWeek).HasColumnType("decimal(5,2)");
            
            entity.HasOne(e => e.Employee)
                .WithOne(e => e.EmploymentContract)
                .HasForeignKey<EmploymentContract>(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Payroll
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Draft");
            entity.Property(e => e.TotalGross).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalNet).HasColumnType("decimal(18,2)");
        });

        // Configure PayrollLine
        modelBuilder.Entity<PayrollLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Earnings).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Allowances).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Deductions).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.NetPay).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Payroll)
                .WithMany(p => p.PayrollLines)
                .HasForeignKey(e => e.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.PayrollLines)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PayrollComponent
        modelBuilder.Entity<PayrollComponent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CalculationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).HasColumnType("decimal(18,6)");
            entity.Property(e => e.Code).HasMaxLength(30);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<EmployeeSalaryComponent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasColumnType("decimal(18,6)");
            entity.HasIndex(e => new { e.TenantId, e.EmployeeId });
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PayrollComponent).WithMany().HasForeignKey(e => e.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TimeEntry
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Device).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
            
            entity.HasIndex(e => new { e.EmployeeId, e.ClockIn });
            
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.TimeEntries)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.TenantId);
            entity.HasOne(e => e.ParentDepartment)
                .WithMany(e => e.ChildDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Benefit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.TenantId);
            entity.HasMany(e => e.Enrollments)
                .WithOne(e => e.Benefit)
                .HasForeignKey(e => e.BenefitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeBenefitEnrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.HasIndex(e => new { e.TenantId, e.EmployeeId });
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeAdvance>(entity =>
        {
            entity.ToTable("EmployeeAdvances", "HRM");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ReferenceNo }).IsUnique();
        });
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("Teams", "HRM");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Shift
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShiftName).IsRequired().HasMaxLength(100);
        });

        // Configure LeaveType
        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MaxAllowedPerYear).HasColumnType("decimal(6,2)");
        });

        // Configure LeaveRequest
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.TotalDays).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.LeaveType)
                .WithMany(lt => lt.LeaveRequests)
                .HasForeignKey(e => e.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure LeaveBalance
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Accrued).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Taken).HasColumnType("decimal(6,2)");
            
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.LeaveBalances)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.LeaveType)
                .WithMany(lt => lt.LeaveBalances)
                .HasForeignKey(e => e.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PerformanceReview
        modelBuilder.Entity<PerformanceReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OverallRating).HasColumnType("decimal(3,2)");
            entity.Property(e => e.Comments).HasMaxLength(4000);
            entity.Property(e => e.GoalsJson).HasMaxLength(4000);
            
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.PerformanceReviews)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure JobOpening
        modelBuilder.Entity<JobOpening>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
            entity.Property(e => e.MinSalary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxSalary).HasColumnType("decimal(18,2)");
        });

        // Configure JobApplication
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResumeUrl).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Applied");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.JobOpening)
                .WithMany(jo => jo.JobApplications)
                .HasForeignKey(e => e.JobOpeningId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Candidate)
                .WithMany(c => c.JobApplications)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Candidate
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.CurrentPosition).HasMaxLength(150);
            entity.Property(e => e.CurrentCompany).HasMaxLength(200);
            entity.Property(e => e.Skills).HasMaxLength(1000);
            entity.Property(e => e.Education).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // Configure Interview
        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InterviewType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Rating).HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Scheduled");
            
            entity.HasOne(e => e.JobApplication)
                .WithMany(ja => ja.Interviews)
                .HasForeignKey(e => e.JobApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Candidate)
                .WithMany()
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Interviewer)
                .WithMany()
                .HasForeignKey(e => e.InterviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Training
        modelBuilder.Entity<Training>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.Trainer).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.TrainingType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Scheduled");
            entity.Property(e => e.ParticipantsJson).HasMaxLength(4000);
        });

        // Configure HRSettings
        modelBuilder.Entity<HRSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultPayFrequency).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WorkingDaysJson).HasMaxLength(1000);
            entity.Property(e => e.OvertimePolicyJson).HasMaxLength(2000);
            entity.Property(e => e.LeaveAccrualPolicyJson).HasMaxLength(2000);
            entity.Property(e => e.TaxSettingsJson).HasMaxLength(2000);
            entity.Property(e => e.ComplianceSettingsJson).HasMaxLength(2000);
        });
    }

    private void ConfigureFinanceEntities(ModelBuilder modelBuilder)
    {
        // Configure ChartOfAccount
        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AccountName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AccountType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CurrencyCode).HasMaxLength(10);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            
            entity.HasIndex(e => new { e.TenantId, e.AccountCode }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.AccountType });
            
            // Self-referencing relationship for parent-child accounts
            entity.HasOne(e => e.ParentAccount)
                .WithMany(e => e.ChildAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AccountDefault>(entity =>
        {
            entity.HasKey(e => e.AccountDefaultId);
            entity.Property(e => e.Purpose).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.Purpose }).IsUnique();
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Journal>(entity =>
        {
            entity.Property(e => e.JournalNumber).HasMaxLength(50);
            entity.Property(e => e.ChequeNumber).HasMaxLength(50);
            entity.Property(e => e.ChequeStatus).HasMaxLength(30);
            entity.Property(e => e.ChequeStatusNotes).HasMaxLength(500);
            entity.Property(e => e.PaidBy).HasMaxLength(200);
            entity.Property(e => e.ReceivedBy).HasMaxLength(200);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.EntityReferenceTable).HasMaxLength(100);
            entity.HasIndex(e => new { e.TenantId, e.ChequeNumber });
            entity.HasIndex(e => new { e.TenantId, e.ChequeStatus });
            entity.HasIndex(e => new { e.TenantId, e.InvoiceNumber });
            entity.HasIndex(e => new { e.TenantId, e.EntityReferenceTable, e.EntityReferenceId });
            entity.HasOne(e => e.CashOrBankAccount)
                .WithMany()
                .HasForeignKey(e => e.CashOrBankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReversedFromJournal)
                .WithMany()
                .HasForeignKey(e => e.ReversedFromJournalId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.FinancialYearId);
            entity.HasIndex(e => new { e.TenantId, e.FinancialYearId });
        });

        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.HasKey(e => e.FiscalYearId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => new { e.TenantId, e.Id }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status, e.StartDate, e.EndDate });
        });

        modelBuilder.Entity<CostCenter>(entity =>
        {
            entity.HasKey(e => e.CostCenterId);
            entity.Property(e => e.CostCenterCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CostCenterName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.TenantId, e.CostCenterCode }).IsUnique();
        });

        // Configure FixedAsset
        modelBuilder.Entity<FixedAsset>(entity =>
        {
            entity.HasKey(e => e.AssetId);
            entity.Property(e => e.AssetCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AssetName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AssetCategory).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PurchaseCost).HasPrecision(18, 2);
            entity.Property(e => e.CurrentValue).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(10);
            
            entity.HasIndex(e => new { e.TenantId, e.AssetCode }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.AssetCategory });
            
            // Relationship with ChartOfAccount (Account + legacy GLAccount share same CoA)
            entity.HasOne(e => e.Account)
                .WithMany(c => c.FixedAssets)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GLAccount)
                .WithMany()
                .HasForeignKey(e => e.GLAccountAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.FixedAssets)
                .HasForeignKey(e => e.AssetCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure DepreciationEntry
        modelBuilder.Entity<DepreciationEntry>(entity =>
        {
            entity.HasKey(e => e.DepreciationId);
            entity.Property(e => e.DepreciationAmount).HasPrecision(28, 8);
            
            entity.HasIndex(e => new { e.TenantId, e.AssetId });
            entity.HasIndex(e => new { e.TenantId, e.PeriodStart, e.PeriodEnd });
            
            // Relationship with FixedAsset
            entity.HasOne(e => e.Asset)
                .WithMany(a => a.DepreciationEntries)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Relationship with Journal (optional)
            entity.HasOne(e => e.PostedJournal)
                .WithMany()
                .HasForeignKey(e => e.PostedJournalId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Currency
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyCode);
            entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CurrencyName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Iso4217).HasMaxLength(3);
            entity.Property(e => e.Symbol).HasMaxLength(10);
        });

        // Configure ExchangeRate
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.ExchangeRateId);
            entity.Property(e => e.FromCurrency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ToCurrency).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Rate).HasPrecision(18, 8);
            
            entity.HasIndex(e => new { e.TenantId, e.FromCurrency, e.ToCurrency });
            entity.HasIndex(e => new { e.TenantId, e.EffectiveDate });
            
            // Relationship with Currency (From)
            entity.HasOne(e => e.FromCurrencyNavigation)
                .WithMany(c => c.FromExchangeRates)
                .HasForeignKey(e => e.FromCurrency)
                .HasPrincipalKey(c => c.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Relationship with Currency (To)
            entity.HasOne(e => e.ToCurrencyNavigation)
                .WithMany(c => c.ToExchangeRates)
                .HasForeignKey(e => e.ToCurrency)
                .HasPrincipalKey(c => c.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSCMEntities(ModelBuilder modelBuilder)
    {
        // Configure Warehouse
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WarehouseCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WarehouseName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Manager).HasMaxLength(100);
            
            entity.HasIndex(e => new { e.TenantId, e.WarehouseCode }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.CompanyId });
            
            // Relationship with Company
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure StockTransfer
        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            
            entity.HasIndex(e => new { e.TenantId, e.TransferDate });
            entity.HasIndex(e => new { e.TenantId, e.FromWarehouseId, e.ToWarehouseId });
            
            // Relationship with Company
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Relationship with FromWarehouse
            entity.HasOne(e => e.FromWarehouse)
                .WithMany(w => w.FromTransfers)
                .HasForeignKey(e => e.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Relationship with ToWarehouse
            entity.HasOne(e => e.ToWarehouse)
                .WithMany(w => w.ToTransfers)
                .HasForeignKey(e => e.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure StockTransferDetail
        modelBuilder.Entity<StockTransferDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            
            entity.HasIndex(e => new { e.TenantId, e.TransferId });
            entity.HasIndex(e => new { e.TenantId, e.ItemId });
            
            // Relationship with StockTransfer
            entity.HasOne(e => e.Transfer)
                .WithMany(t => t.StockTransferDetails)
                .HasForeignKey(e => e.TransferId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Relationship with Item
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Relationship with BatchSerialTracking (optional)
            entity.HasOne(e => e.BatchSerial)
                .WithMany()
                .HasForeignKey(e => e.BatchSerialId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureRemainingEntities(ModelBuilder modelBuilder)
    {
        // CRM Entities - Basic configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Salesman)
                .WithMany()
                .HasForeignKey(e => e.SalesmanId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Lead>().HasKey(e => e.Id);
        modelBuilder.Entity<Opportunity>().HasKey(e => e.Id);
        modelBuilder.Entity<Activity>().HasKey(e => e.Id);
        modelBuilder.Entity<Campaign>().HasKey(e => e.Id);
        ConfigureCrmExtendedEntities(modelBuilder);
        modelBuilder.Entity<Contact>().HasKey(e => e.Id);
        modelBuilder.Entity<SalesOrder>().HasKey(e => e.Id);
        modelBuilder.Entity<SalesOrderLine>().HasKey(e => e.Id);
        modelBuilder.Entity<SalesInvoice>().HasKey(e => e.Id);
        modelBuilder.Entity<SalesInvoiceLine>().HasKey(e => e.Id);
        modelBuilder.Entity<SalesCreditNote>().HasKey(e => e.Id);
        modelBuilder.Entity<SalesCreditNoteLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<SalesCreditNote>()
                .WithMany()
                .HasForeignKey(e => e.CreditNoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Quote>().HasKey(e => e.Id);
        modelBuilder.Entity<QuoteLine>().HasKey(e => e.Id);
        modelBuilder.Entity<SystemSetting>().HasKey(e => e.Id);
        modelBuilder.Entity<SystemLog>().HasKey(e => e.Id);

        // Manufacturing Entities
        modelBuilder.Entity<WorkCenter>().HasKey(e => e.Id);
        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.BillOfMaterial)
                .WithMany()
                .HasForeignKey(e => e.BomId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Routing)
                .WithMany()
                .HasForeignKey(e => e.RoutingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.WorkCenter)
                .WithMany()
                .HasForeignKey(e => e.WorkCenterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Operations)
                .WithOne(o => o.ProductionOrder)
                .HasForeignKey(o => o.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.QualityChecks)
                .WithOne(q => q.ProductionOrder)
                .HasForeignKey(q => q.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TenantId, e.SalesOrderLineId });
        });
        modelBuilder.Entity<ProductionOrderLine>().HasKey(e => e.Id);
        modelBuilder.Entity<ProductionRun>().HasKey(e => e.Id);
        modelBuilder.Entity<BillOfMaterial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.BomNumber }).IsUnique();
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.BillOfMaterial)
                .HasForeignKey(l => l.BillOfMaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<BillOfMaterialLine>().HasKey(e => e.Id);
        modelBuilder.Entity<Routing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.RoutingNumber }).IsUnique();
            entity.HasMany(e => e.Steps)
                .WithOne(s => s.Routing)
                .HasForeignKey(s => s.RoutingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RoutingStep>().HasKey(e => e.Id);
        modelBuilder.Entity<ProductionScrap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ScrapReason)
                .WithMany()
                .HasForeignKey(e => e.ScrapReasonId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ScrapReason>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        });
        modelBuilder.Entity<ProductionOrderOperation>().HasKey(e => e.Id);
        modelBuilder.Entity<QualityCheck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.QualityCheck)
                .HasForeignKey(l => l.QualityCheckId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<QualityCheckLine>().HasKey(e => e.Id);

        // Project Entities
        modelBuilder.Entity<Project>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectTask>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectResource>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectDocument>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectMilestone>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("ProjectMembers", "Projects");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ProjectId });
            entity.HasIndex(e => new { e.TenantId, e.EmployeeId });
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ProjectTimesheet>(entity =>
        {
            entity.ToTable("ProjectTimesheets", "Projects");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.ProjectTimesheet)
                .HasForeignKey(l => l.ProjectTimesheetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProjectTimesheetLine>(entity =>
        {
            entity.ToTable("ProjectTimesheetLines", "Projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HourlyRate).HasPrecision(18, 4);
        });

        // Procurement Entities
        modelBuilder.Entity<Vendor>().HasKey(e => e.Id);
        modelBuilder.Entity<VendorBankAccount>().HasKey(e => e.Id);
        modelBuilder.Entity<LandedCostVoucher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.LandedCostVoucher)
                .HasForeignKey(l => l.LandedCostVoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<LandedCostVoucherLine>().HasKey(e => e.Id);
        modelBuilder.Entity<VendorContract>().HasKey(e => e.Id);
        // Configure GoodsReceiptNote with proper cascade settings
        modelBuilder.Entity<GoodsReceiptNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // PurchaseOrder relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(po => po.GoodsReceiptNotes)
                .HasForeignKey(e => e.POId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // User relationship - use Restrict to avoid cascade path with Tenant
            entity.HasOne(e => e.ReceivedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReceivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Vendor relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure GoodsReceiptNoteDetail with proper cascade settings
        modelBuilder.Entity<GoodsReceiptNoteDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Item relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure PurchaseOrder with proper cascade settings
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Vendor relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure PurchaseOrderDetail with proper cascade settings
        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // PurchaseOrder relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(po => po.Details)
                .HasForeignKey(e => e.POId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure PurchaseInvoice with proper cascade settings
        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Vendor relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure PurchaseInvoiceDetail with proper cascade settings
        modelBuilder.Entity<PurchaseInvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // PurchaseInvoice relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.PurchaseInvoice)
                .WithMany(pi => pi.Details)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.ToTable("PurchaseReturns");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Lines)
                .WithOne(l => l.PurchaseReturn)
                .HasForeignKey(l => l.PurchaseReturnId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseDebitNote>().HasKey(e => e.Id);
        modelBuilder.Entity<PurchaseDebitNoteLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<PurchaseDebitNote>()
                .WithMany()
                .HasForeignKey(e => e.DebitNoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseReturnLine>(entity =>
        {
            entity.ToTable("PurchaseReturnLines");
            entity.HasKey(e => e.Id);
        });
        // Configure PurchaseRequisition with proper cascade settings
        modelBuilder.Entity<PurchaseRequisition>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.RequestedByUser)
                .WithMany()
                .HasForeignKey(e => e.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure PurchaseRequisitionDetail with proper cascade settings
        modelBuilder.Entity<PurchaseRequisitionDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // UnitOfMeasure relationship - use Restrict to avoid cascade path issues
            entity.HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<RequestForQuotation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure RFQDetail with proper cascade settings
        modelBuilder.Entity<RFQDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // RequestForQuotation relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.RequestForQuotation)
                .WithMany(rfq => rfq.Details)
                .HasForeignKey(e => e.RFQId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // UnitOfMeasure relationship - use Restrict to avoid cascade path issues
            entity.HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure RFQVendorResponse with proper cascade settings
        modelBuilder.Entity<RFQVendorResponse>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.RequestForQuotation)
                .WithMany(rfq => rfq.VendorResponses)
                .HasForeignKey(e => e.RFQId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Vendor relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure RFQVendorResponseDetail with proper cascade settings
        modelBuilder.Entity<RFQVendorResponseDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // RFQVendorResponse relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.RFQVendorResponse)
                .WithMany(rfqvr => rfqvr.Details)
                .HasForeignKey(e => e.RFQResponseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Sales/POS Entities
        modelBuilder.Entity<POSSession>().HasKey(e => e.Id);
        modelBuilder.Entity<POSTransaction>().HasKey(e => e.Id);
        modelBuilder.Entity<POSTransactionLine>().HasKey(e => e.Id);

        // Additional SCM/Inventory Entities
        // Configure Item with proper cascade settings
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // ItemCategory relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Brand)
                .WithMany()
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<Brand>().HasKey(e => e.Id);
        modelBuilder.Entity<WarehouseLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.StockReservation)
                .HasForeignKey(l => l.StockReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<StockReservationLine>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemKit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Components)
                .WithOne(c => c.ItemKit)
                .HasForeignKey(c => c.ItemKitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ItemKitComponent>().HasKey(e => e.Id);
        modelBuilder.Entity<CycleCount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.CycleCount)
                .HasForeignKey(l => l.CycleCountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CycleCountLine>().HasKey(e => e.Id);
        modelBuilder.Entity<PutawayTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.PutawayTask)
                .HasForeignKey(l => l.PutawayTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PutawayTaskLine>().HasKey(e => e.Id);
        modelBuilder.Entity<PickList>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.PickList)
                .HasForeignKey(l => l.PickListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PickListLine>().HasKey(e => e.Id);
        modelBuilder.Entity<ReplenishmentTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.ReplenishmentTask)
                .HasForeignKey(l => l.ReplenishmentTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ReplenishmentTaskLine>().HasKey(e => e.Id);
        modelBuilder.Entity<PackOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.PackOrder)
                .HasForeignKey(l => l.PackOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PackOrderLine>().HasKey(e => e.Id);
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.Shipment)
                .HasForeignKey(l => l.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ShipmentLine>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemCategory>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemGroup>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemAttribute>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemVariant>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemVariantAttribute>().HasKey(e => e.Id);
        modelBuilder.Entity<ItemPricing>().HasKey(e => e.Id);
        modelBuilder.Entity<UnitOfMeasure>().HasKey(e => e.Id);
        // Configure StockBalance with proper cascade settings
        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Item relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.WarehouseLocation)
                .WithMany()
                .HasForeignKey(e => e.WarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
        // Configure StockLedger with proper cascade settings
        modelBuilder.Entity<StockLedger>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Item relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.WarehouseLocation)
                .WithMany()
                .HasForeignKey(e => e.WarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
        // Configure InventoryAdjustment with proper cascade settings
        modelBuilder.Entity<InventoryAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Item relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Item)
                .WithMany(i => i.InventoryAdjustments)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.StockAdjustment)
                .HasForeignKey(l => l.StockAdjustmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<StockAdjustmentLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<InventoryValuationMethod>().HasKey(e => e.Id);
        modelBuilder.Entity<ReorderRule>().HasKey(e => e.Id);
        modelBuilder.Entity<Barcode>().HasKey(e => e.Id);
        modelBuilder.Entity<BatchNumber>().HasKey(e => e.Id);
        modelBuilder.Entity<SerialNumber>().HasKey(e => e.Id);
        modelBuilder.Entity<BatchSerialTracking>().HasKey(e => e.Id);

        // Report Entities
        modelBuilder.Entity<Report>().HasKey(e => e.Id);
        modelBuilder.Entity<ReportExecution>().HasKey(e => e.Id);
        modelBuilder.Entity<ReportSchedule>().HasKey(e => e.Id);

        // Additional Finance Entities
        modelBuilder.Entity<Payment>().HasKey(e => e.Id);
        modelBuilder.Entity<Bill>().HasKey(e => e.Id);
        modelBuilder.Entity<BillLine>().HasKey(e => e.BillLineId);
        modelBuilder.Entity<Journal>().HasKey(e => e.Id);
        modelBuilder.Entity<JournalEntry>().HasKey(e => e.Id);
        modelBuilder.Entity<Invoice>().HasKey(e => e.Id);
        modelBuilder.Entity<InvoiceLine>().HasKey(e => e.InvoiceLineId);
        modelBuilder.Entity<Bank>().HasKey(e => e.Id);
        modelBuilder.Entity<BankAccount>().HasKey(e => e.Id);
        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasKey(e => e.BankTxId);
            entity.HasOne(e => e.Reconciliation)
                .WithMany(r => r.BankTransactions)
                .HasForeignKey(e => e.ReconciliationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<BankReconciliation>().HasKey(e => e.ReconciliationId);
        modelBuilder.Entity<Budget>().HasKey(e => e.Id);
        ConfigureFinanceCustomerDepthEntities(modelBuilder);
        ConfigureFinanceGapEntities(modelBuilder);
        // CostCenter PK configured in ConfigureFinanceEntities (CostCenterId)
        modelBuilder.Entity<TaxCode>().HasKey(e => e.Id);
        modelBuilder.Entity<TaxRate>().HasKey(e => e.Id);
        modelBuilder.Entity<PaymentApplication>().HasKey(e => e.Id);
        modelBuilder.Entity<IntercompanyTransaction>().HasKey(e => e.IntercompanyId);
    }

    private static void ConfigureCrmExtendedEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerGroup>(entity =>
        {
            entity.ToTable("CustomerGroups");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.ToTable("PriceLists");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<SalesTerritory>(entity =>
        {
            entity.ToTable("SalesTerritories");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Salesman>(entity =>
        {
            entity.ToTable("Salesmen");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ReturnOrder>(entity =>
        {
            entity.ToTable("ReturnOrders");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalesOrder)
                .WithMany()
                .HasForeignKey(e => e.SalesOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Lines)
                .WithOne(l => l.ReturnOrder)
                .HasForeignKey(l => l.ReturnOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReturnOrderLine>(entity =>
        {
            entity.ToTable("ReturnOrderLines");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PouchType>(entity =>
        {
            entity.ToTable("PouchTypes", "CRM");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<ProductionProcess>(entity =>
        {
            entity.ToTable("ProductionProcesses", "CRM");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<FOHType>(entity =>
        {
            entity.ToTable("FOHTypes", "CRM");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<SaleEnquiry>(entity =>
        {
            entity.ToTable("SaleEnquiries", "CRM");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.SaleEnquiry)
                .HasForeignKey(l => l.SaleEnquiryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<SaleEnquiryLine>(entity =>
        {
            entity.ToTable("SaleEnquiryLines", "CRM");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<SalePreCost>(entity =>
        {
            entity.ToTable("SalePreCosts", "CRM");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.SalePreCost)
                .HasForeignKey(l => l.SalePreCostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<SalePreCostLine>(entity =>
        {
            entity.ToTable("SalePreCostLines", "CRM");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<DispatchAdvice>(entity =>
        {
            entity.ToTable("DispatchAdvices");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.DispatchAdvice)
                .HasForeignKey(l => l.DispatchAdviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DispatchAdviceLine>(entity =>
        {
            entity.ToTable("DispatchAdviceLines");
            entity.HasKey(e => e.Id);
        });
    }

    private static void ConfigureFinanceCustomerDepthEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpeningBalance>(entity =>
        {
            entity.ToTable("OpeningBalances", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.OpeningBalance)
                .HasForeignKey(l => l.OpeningBalanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OpeningBalanceLine>(entity =>
        {
            entity.ToTable("OpeningBalanceLines", "Finance");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<AccountingPeriod>(entity =>
        {
            entity.ToTable("AccountingPeriods", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.StartDate, e.EndDate });
        });
        modelBuilder.Entity<ChequeBook>(entity =>
        {
            entity.ToTable("ChequeBooks", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.BookNumber }).IsUnique();
            entity.HasMany(e => e.Leaves)
                .WithOne(l => l.ChequeBook)
                .HasForeignKey(l => l.ChequeBookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ChequeLeaf>(entity =>
        {
            entity.ToTable("ChequeLeaves", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ChequeBookId, e.LeafNumber }).IsUnique();
        });
        modelBuilder.Entity<ExpenseClaim>(entity =>
        {
            entity.ToTable("ExpenseClaims", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ClaimNumber }).IsUnique();
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.ExpenseClaim)
                .HasForeignKey(l => l.ExpenseClaimId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<ExpenseClaimLine>(entity =>
        {
            entity.ToTable("ExpenseClaimLines", "Finance");
            entity.HasKey(e => e.Id);
        });
    }

    private static void ConfigureFinanceGapEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.SourceSalesInvoiceId)
                .IsUnique()
                .HasFilter("[SourceSalesInvoiceId] IS NOT NULL");
            entity.Property(e => e.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasIndex(e => e.SourcePurchaseInvoiceId)
                .IsUnique()
                .HasFilter("[SourcePurchaseInvoiceId] IS NOT NULL");
        });

        modelBuilder.Entity<PaymentApplication>(entity =>
        {
            entity.Property(e => e.InvoiceId).IsRequired(false);
            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Bill)
                .WithMany()
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_PaymentApplications_XorTarget",
                "([InvoiceId] IS NOT NULL AND [BillId] IS NULL) OR ([InvoiceId] IS NULL AND [BillId] IS NOT NULL)"));
        });

        modelBuilder.Entity<TaxTransaction>(entity =>
        {
            entity.ToTable("TaxTransactions", "Finance");
            entity.HasKey(e => e.TaxTransactionId);
            entity.HasOne(e => e.TaxCode)
                .WithMany()
                .HasForeignKey(e => e.TaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BudgetLine>(entity =>
        {
            entity.ToTable("BudgetLines", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.Lines)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaxGroup>(entity =>
        {
            entity.ToTable("TaxGroups", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.TaxGroup)
                .HasForeignKey(l => l.TaxGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<TaxGroupLine>(entity =>
        {
            entity.ToTable("TaxGroupLines", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.TaxCode)
                .WithMany()
                .HasForeignKey(e => e.TaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.ToTable("AssetCategories", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        });
        modelBuilder.Entity<AssetDisposal>(entity =>
        {
            entity.ToTable("AssetDisposals", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Asset)
                .WithMany(a => a.Disposals)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AssetTransfer>(entity =>
        {
            entity.ToTable("AssetTransfers", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Asset)
                .WithMany(a => a.Transfers)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BankReconciliationLine>(entity =>
        {
            entity.ToTable("BankReconciliationLines", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ReconciliationId, e.BankTransactionId }).IsUnique();
        });
        modelBuilder.Entity<FundTransfer>(entity =>
        {
            entity.ToTable("FundTransfers", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.TransferNumber }).IsUnique();
        });

        modelBuilder.Entity<CreditMemo>(entity =>
        {
            entity.ToTable("CreditMemos", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.MemoNumber }).IsUnique();
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.CreditMemo)
                .HasForeignKey(l => l.CreditMemoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CreditMemoLine>(entity =>
        {
            entity.ToTable("CreditMemoLines", "Finance");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<DebitMemo>(entity =>
        {
            entity.ToTable("DebitMemos", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.MemoNumber }).IsUnique();
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.DebitMemo)
                .HasForeignKey(l => l.DebitMemoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DebitMemoLine>(entity =>
        {
            entity.ToTable("DebitMemoLines", "Finance");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<CustomerAdvance>(entity =>
        {
            entity.ToTable("CustomerAdvances", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.AdvanceNumber }).IsUnique();
        });
        modelBuilder.Entity<VendorAdvance>(entity =>
        {
            entity.ToTable("VendorAdvances", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.AdvanceNumber }).IsUnique();
        });
        modelBuilder.Entity<AdvanceApplication>(entity =>
        {
            entity.ToTable("AdvanceApplications", "Finance");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<WithholdingTaxLine>(entity =>
        {
            entity.ToTable("WithholdingTaxLines", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Payment)
                .WithMany(p => p.WithholdingTaxLines)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TaxCode)
                .WithMany()
                .HasForeignKey(e => e.TaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecurringJournal>(entity =>
        {
            entity.ToTable("RecurringJournals", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.RecurringJournal)
                .HasForeignKey(l => l.RecurringJournalId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RecurringJournalLine>(entity =>
        {
            entity.ToTable("RecurringJournalLines", "Finance");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<FxRevaluationRun>(entity =>
        {
            entity.ToTable("FxRevaluationRuns", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.FxRevaluationRun)
                .HasForeignKey(l => l.FxRevaluationRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<FxRevaluationLine>(entity =>
        {
            entity.ToTable("FxRevaluationLines", "Finance");
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<IntercompanySettlement>(entity =>
        {
            entity.ToTable("IntercompanySettlements", "Finance");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.SettlementNumber }).IsUnique();
            entity.HasMany(e => e.Transactions)
                .WithOne(t => t.Settlement)
                .HasForeignKey(t => t.SettlementId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}