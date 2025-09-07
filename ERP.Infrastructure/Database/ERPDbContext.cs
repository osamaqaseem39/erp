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

        // Configure Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionString).IsRequired();
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        // Configure Plan
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlanName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.FeaturesJSON).HasMaxLength(4000);
        });

        // Configure Subscription
        modelBuilder.Entity<Subscription>(entity =>
        {
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
            
            // Tenant relationship will be handled by TenantId foreign key
        });

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Tenant relationship will be handled by TenantId foreign key
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
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

        // Configure UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ActionName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
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

        // Configure AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
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

        // Configure Setting
        modelBuilder.Entity<Setting>(entity =>
        {
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
                .OnDelete(DeleteBehavior.Cascade);
                
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
}