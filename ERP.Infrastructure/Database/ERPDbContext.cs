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
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<DocumentSequence> DocumentSequences { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    // HRM Entities
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeAddress> EmployeeAddresses { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<EmploymentContract> EmploymentContracts { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<PayrollLine> PayrollLines { get; set; }
    public DbSet<PayrollComponent> PayrollComponents { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
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
    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<TaxCode> TaxCodes { get; set; }
    public DbSet<TaxRate> TaxRates { get; set; }
    public DbSet<PaymentApplication> PaymentApplications { get; set; }
    public DbSet<DepreciationEntry> DepreciationEntries { get; set; }
    public DbSet<IntercompanyTransaction> IntercompanyTransactions { get; set; }

    // CRM Entities
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<CustomerGroup> CustomerGroups { get; set; }
    public DbSet<PriceList> PriceLists { get; set; }
    public DbSet<SalesTerritory> SalesTerritories { get; set; }
    public DbSet<ReturnOrder> ReturnOrders { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceLine> SalesInvoiceLines { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<QuoteLine> QuoteLines { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

    // Manufacturing Entities
    public DbSet<WorkCenter> WorkCenters { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionOrderLine> ProductionOrderLines { get; set; }
    public DbSet<ProductionRun> ProductionRuns { get; set; }

    // Project Entities
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ProjectResource> ProjectResources { get; set; }
    public DbSet<ProjectDocument> ProjectDocuments { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }

    // Procurement Entities
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorBankAccount> VendorBankAccounts { get; set; }
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
    public DbSet<GoodsReceiptNoteDetail> GoodsReceiptNoteDetails { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<PurchaseRequisitionDetail> PurchaseRequisitionDetails { get; set; }
    public DbSet<RequestForQuotation> RequestForQuotations { get; set; }
    public DbSet<RFQDetail> RFQDetails { get; set; }
    public DbSet<RFQVendorResponse> RFQVendorResponses { get; set; }
    public DbSet<RFQVendorResponseDetail> RFQVendorResponseDetails { get; set; }

    // Sales/POS Entities
    public DbSet<POSSession> POSSessions { get; set; }
    public DbSet<POSTransaction> POSTransactions { get; set; }
    public DbSet<POSTransactionLine> POSTransactionLines { get; set; }

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
    public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }
    public DbSet<InventoryValuationMethod> InventoryValuationMethods { get; set; }
    public DbSet<ReorderRule> ReorderRules { get; set; }
    public DbSet<Barcode> Barcodes { get; set; }
    public DbSet<BatchNumber> BatchNumbers { get; set; }
    public DbSet<SerialNumber> SerialNumbers { get; set; }
    public DbSet<BatchSerialTracking> BatchSerialTrackings { get; set; }

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
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Ignore(e => e.CompanyCode);
            entity.Ignore(e => e.TaxId);
            entity.Ignore(e => e.Address);
            entity.Ignore(e => e.PostalCode);
            entity.Ignore(e => e.Currency);
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
            entity.HasIndex(e => e.PrimaryEmail).IsUnique();
            
            // Tenant relationship will be handled by TenantId foreign key
                
            entity.HasOne(e => e.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CompanyId)
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
            
            entity.HasIndex(e => new { e.TenantId, e.AccountCode }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.AccountType });
            
            // Self-referencing relationship for parent-child accounts
            entity.HasOne(e => e.ParentAccount)
                .WithMany(e => e.ChildAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
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
            
            // Relationship with ChartOfAccount
            entity.HasOne(e => e.Account)
                .WithMany(c => c.FixedAssets)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
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
        modelBuilder.Entity<Customer>().HasKey(e => e.Id);
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
        modelBuilder.Entity<Quote>().HasKey(e => e.Id);
        modelBuilder.Entity<QuoteLine>().HasKey(e => e.Id);
        modelBuilder.Entity<SystemSetting>().HasKey(e => e.Id);
        modelBuilder.Entity<SystemLog>().HasKey(e => e.Id);

        // Manufacturing Entities
        modelBuilder.Entity<WorkCenter>().HasKey(e => e.Id);
        modelBuilder.Entity<ProductionOrder>().HasKey(e => e.Id);
        modelBuilder.Entity<ProductionOrderLine>().HasKey(e => e.Id);
        modelBuilder.Entity<ProductionRun>().HasKey(e => e.Id);

        // Project Entities
        modelBuilder.Entity<Project>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectTask>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectResource>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectDocument>().HasKey(e => e.Id);
        modelBuilder.Entity<ProjectMilestone>().HasKey(e => e.Id);

        // Procurement Entities
        modelBuilder.Entity<Vendor>().HasKey(e => e.Id);
        modelBuilder.Entity<VendorBankAccount>().HasKey(e => e.Id);
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
        modelBuilder.Entity<RequestForQuotation>().HasKey(e => e.Id);
        // Configure RFQDetail with proper cascade settings
        modelBuilder.Entity<RFQDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // RequestForQuotation relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.RequestForQuotation)
                .WithMany(rfq => rfq.Details)
                .HasForeignKey(e => e.RFQId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // UnitOfMeasure relationship - use Restrict to avoid cascade path issues
            entity.HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        // Configure RFQVendorResponse with proper cascade settings
        modelBuilder.Entity<RFQVendorResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Vendor relationship - use Restrict to avoid cascade path with Company
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
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
        });
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
        modelBuilder.Entity<BankTransaction>().HasKey(e => e.BankTxId);
        modelBuilder.Entity<BankReconciliation>().HasKey(e => e.ReconciliationId);
        modelBuilder.Entity<Budget>().HasKey(e => e.Id);
        modelBuilder.Entity<CostCenter>().HasKey(e => e.Id);
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
        });
    }
}