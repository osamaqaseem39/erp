using ERP.Api.Mappers;
using ERP.Api.Mappers.Sales;
using ERP.Api.Services;
using ERP.Api.Validators;
using ERP.Api.Validators.CRM;
using ERP.Api.Validators.Finance;
using ERP.Api.Validators.Procurement;
using ERP.Api.Validators.Sales;
using ERP.Application.Admin;
using ERP.Application.Core;
using ERP.Application.CRM;
using ERP.Application.Finance;
using ERP.Application.HRM;
using ERP.Application.Interfaces.Admin;
using ERP.Application.Interfaces.Core;
using ERP.Application.Interfaces.CRM;
using ERP.Application.Interfaces.Sales;
using ERP.Application.Inventory;
using ERP.Application.Manufacturing;
using ERP.Application.Procurement;
using ERP.Application.Projects;
using ERP.Application.Reports;
using ERP.Application.Sales;
using ERP.Application.SCM.Adapters;
using ERP.Application.Services.Procurement;
using ERP.Infrastructure.Interfaces;
using ERP.Infrastructure.Interfaces.Core;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Interfaces.Sales;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Infrastructure.Repositories.Core;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Infrastructure.Repositories.Manufacturing;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Infrastructure.Repositories.Reports;
using ERP.Infrastructure.Repositories.Sales;
using ERP.Infrastructure.Repositories.SCM;
using ERP.Infrastructure.Services;
using ERP.Shared.Abstractions;
using AdminRoleRepository = ERP.Infrastructure.Interfaces.Admin.IRoleRepository;
using AdminRoleRepositoryImpl = ERP.Infrastructure.Repositories.Admin.RoleRepository;

namespace ERP.Api.Extensions;

public static class ModuleServiceCollectionExtensions
{
    public static IServiceCollection AddErpInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        // App data paths use EF (DbContext registered in Program). Stored-procedure executor no longer registered.
        services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
        services.AddSingleton<ERP.Shared.Abstractions.ISecretProtector, ERP.Api.Security.DataProtectionSecretProtector>();
        services.AddScoped<ERP.Application.Core.Settings.ICoreSettingsService, ERP.Application.Core.Settings.CoreSettingsService>();
        services.AddScoped<ERP.Application.Core.ICompanyProfileService, ERP.Application.Core.CompanyProfileService>();
        services.AddScoped<ERP.Infrastructure.Repositories.Core.IFiscalYearRepository, ERP.Infrastructure.Repositories.Core.FiscalYearRepository>();
        services.AddScoped<ERP.Application.Interfaces.Core.IFiscalYearService, ERP.Application.Core.FiscalYearService>();
        services.AddScoped<ERP.Infrastructure.Repositories.Core.ICoreMasterRepository, ERP.Infrastructure.Repositories.Core.CoreMasterRepository>();
        services.AddScoped<ERP.Application.Core.ICoreMasterService, ERP.Application.Core.CoreMasterService>();
        services.AddScoped<ERP.Application.Core.ITenantEmailSender, ERP.Application.Core.SmtpTenantEmailSender>();
        services.AddScoped<ERP.Application.Core.IPdfDocumentService, ERP.Application.Core.SimplePdfDocumentService>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ERP.Application.Core.ICoreDashboardService, ERP.Application.Core.CoreDashboardService>();
        services.AddScoped<ERP.Infrastructure.Numbering.IDocumentNumberService>(sp => new ERP.Infrastructure.Numbering.DocumentNumberService(
            sp.GetRequiredService<ERP.Infrastructure.Database.ERPDbContext>(), sp.GetRequiredService<ITenantProvider>(), sp.GetRequiredService<TimeProvider>()));
        return services;
    }

    public static IServiceCollection AddErpCoreModule(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Core.IPermissionRepository, PermissionRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ERP.Application.Core.Settings.ISecurityPolicyProvider, ERP.Application.Core.Settings.SecurityPolicyProvider>();
        services.AddScoped<ERP.Application.Core.Auth.ISessionStore, ERP.Application.Core.Auth.SessionStore>();
        services.AddScoped<ERP.Application.Core.Auth.IPasswordResetNotifier, ERP.Api.Services.SmtpPasswordResetNotifier>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITenantApiService, TenantApiService>();
        services.AddScoped<IAuthApiService, AuthApiService>();
        services.AddScoped<TenantMapper>();
        services.AddScoped<CreateTenantValidator>();
        services.AddScoped<UpdateTenantValidator>();
        services.AddScoped<LoginValidator>();
        services.AddScoped<RefreshTokenValidator>();
        return services;
    }

    public static IServiceCollection AddErpAdminModule(this IServiceCollection services)
    {
        services.AddScoped<AdminRoleRepository, AdminRoleRepositoryImpl>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ERP.Application.Interfaces.Admin.IRoleService, RoleService>();
        services.AddScoped<ERP.Application.Admin.IAdminDashboardService, ERP.Application.Admin.AdminDashboardService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ISystemLogService, SystemLogService>();
        services.AddScoped<ERP.Infrastructure.Repositories.Core.IAdminEmailRepository, ERP.Infrastructure.Repositories.Core.AdminEmailRepository>();
        services.AddScoped<ERP.Application.Core.IAdminEmailService, ERP.Application.Core.AdminEmailService>();
        return services;
    }

    public static IServiceCollection AddErpSalesModule(this IServiceCollection services)
    {
        services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
        services.AddScoped<ISalesInvoiceLineRepository, SalesInvoiceLineRepository>();
        services.AddScoped<ISalesCreditNoteRepository, SalesCreditNoteRepository>();
        services.AddScoped<ISalesCreditNoteLineRepository, SalesCreditNoteLineRepository>();
        services.AddScoped<IPOSSessionRepository, POSSessionRepository>();
        services.AddScoped<IPOSTransactionRepository, POSTransactionRepository>();
        services.AddScoped<IPOSTransactionLineRepository, POSTransactionLineRepository>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<ERP.Infrastructure.Repositories.Sales.IDispatchAdviceRepository, ERP.Infrastructure.Repositories.Sales.DispatchAdviceRepository>();
        services.AddScoped<IDispatchAdviceService, DispatchAdviceService>();
        services.AddScoped<ISalesCreditNoteService, SalesCreditNoteService>();
        services.AddScoped<IPOSSessionService, POSSessionService>();
        services.AddScoped<IPOSTransactionService, POSTransactionService>();
        services.AddScoped<SalesInvoiceMapper>();
        services.AddScoped<POSSessionMapper>();
        services.AddScoped<POSTransactionMapper>();
        services.AddScoped<CreateSalesInvoiceValidator>();
        services.AddScoped<UpdateSalesInvoiceValidator>();
        services.AddScoped<CreateSalesCreditNoteValidator>();
        services.AddScoped<UpdateSalesCreditNoteValidator>();
        services.AddScoped<CreatePOSSessionValidator>();
        services.AddScoped<CreatePOSTransactionValidator>();
        return services;
    }

    public static IServiceCollection AddErpCrmModule(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IQuoteLineRepository, QuoteLineRepository>();
        services.AddScoped<ISalesOrderLineRepository, SalesOrderLineRepository>();
        services.AddScoped<ICustomerGroupRepository, CustomerGroupRepository>();
        services.AddScoped<IPriceListRepository, PriceListRepository>();
        services.AddScoped<ISalesTerritoryRepository, SalesTerritoryRepository>();
        services.AddScoped<IReturnOrderRepository, ReturnOrderRepository>();
        services.AddScoped<ISalesmanRepository, SalesmanRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.CRM.IPouchTypeRepository, ERP.Infrastructure.Repositories.CRM.PouchTypeRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.CRM.IProductionProcessRepository, ERP.Infrastructure.Repositories.CRM.ProductionProcessRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.CRM.IFOHTypeRepository, ERP.Infrastructure.Repositories.CRM.FOHTypeRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.CRM.ISaleEnquiryRepository, ERP.Infrastructure.Repositories.CRM.SaleEnquiryRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.CRM.ISalePreCostRepository, ERP.Infrastructure.Repositories.CRM.SalePreCostRepository>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICustomerGroupService, CustomerGroupService>();
        services.AddScoped<IPriceListService, PriceListService>();
        services.AddScoped<ISalesTerritoryService, SalesTerritoryService>();
        services.AddScoped<IReturnOrderService, ReturnOrderService>();
        services.AddScoped<ISalesmanService, SalesmanService>();
        services.AddScoped<IPouchTypeService, PouchTypeService>();
        services.AddScoped<IProductionProcessService, ProductionProcessService>();
        services.AddScoped<IFOHTypeService, FOHTypeService>();
        services.AddScoped<ISaleEnquiryService, SaleEnquiryService>();
        services.AddScoped<ISalePreCostService, SalePreCostService>();

        services.AddScoped<CreateCustomerValidator>();
        services.AddScoped<UpdateCustomerValidator>();
        services.AddScoped<CreateContactValidator>();
        services.AddScoped<UpdateContactValidator>();
        services.AddScoped<CreateLeadValidator>();
        services.AddScoped<UpdateLeadValidator>();
        services.AddScoped<CreateOpportunityValidator>();
        services.AddScoped<UpdateOpportunityValidator>();
        services.AddScoped<CreateQuoteValidator>();
        services.AddScoped<UpdateQuoteValidator>();
        services.AddScoped<CreateSalesOrderValidator>();
        services.AddScoped<UpdateSalesOrderValidator>();
        services.AddScoped<CreateActivityValidator>();
        services.AddScoped<UpdateActivityValidator>();
        services.AddScoped<CreateCampaignValidator>();
        services.AddScoped<UpdateCampaignValidator>();
        services.AddScoped<CreateCustomerGroupValidator>();
        services.AddScoped<UpdateCustomerGroupValidator>();
        services.AddScoped<CreatePriceListValidator>();
        services.AddScoped<UpdatePriceListValidator>();
        services.AddScoped<CreateSalesTerritoryValidator>();
        services.AddScoped<UpdateSalesTerritoryValidator>();
        services.AddScoped<CreateReturnOrderValidator>();
        services.AddScoped<UpdateReturnOrderValidator>();
        services.AddScoped<CreateSalesmanValidator>();
        services.AddScoped<UpdateSalesmanValidator>();
        return services;
    }

    public static IServiceCollection AddErpFinanceModule(this IServiceCollection services)
    {
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IChartOfAccountRepository, ChartOfAccountRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.Finance.IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IPaymentRepository, PaymentRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.ITaxCodeRepository, TaxCodeRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.Finance.ITaxRateRepository, TaxRateRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IBankRepository, BankRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IBillRepository, BillRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IFixedAssetRepository, FixedAssetRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.IJournalRepository, JournalRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.Finance.IChartOfAccountRepository, ChartOfAccountRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.Finance.IPaymentRepository, PaymentRepositoryAdapter>();

        services.AddScoped<ERP.Application.Interfaces.Finance.IInvoiceRepository, ApplicationInvoiceRepositoryAdapter>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IJournalRepository, ApplicationJournalRepositoryAdapter>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IChartOfAccountRepository, ApplicationChartOfAccountRepositoryAdapter>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IAccountingService, AccountingService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IBankService, BankService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IFixedAssetService, FixedAssetService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IBillService, BillService>();
        services.AddScoped<ERP.Application.Interfaces.ICurrencyService, CurrencyService>();
        services.AddScoped<ERP.Application.Interfaces.IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.ITaxService, TaxService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.ITaxCodeService, TaxCodeService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IPaymentService, PaymentService>();
        services.AddScoped<ERP.Application.Finance.IInvoiceDeliveryNotifier, ERP.Application.Finance.LoggingInvoiceDeliveryNotifier>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IInvoiceService, InvoiceService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IJournalService, JournalService>();
        services.AddScoped<ERP.Application.Interfaces.IChartOfAccountsService, ChartOfAccountsService>();
        services.AddScoped<ERP.Infrastructure.Repositories.Finance.ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<ERP.Application.Interfaces.Finance.ICostCenterService, CostCenterService>();
        services.AddScoped<ERP.Application.Interfaces.Finance.IAccountDefaultService, AccountDefaultService>();
        services.AddScoped<ERP.Application.Finance.IBudgetEnforcementService, ERP.Application.Finance.BudgetEnforcementService>();
        services.AddScoped<ERP.Application.Finance.IBudgetService, ERP.Application.Finance.BudgetService>();
        services.AddScoped<ERP.Application.Finance.IOpeningBalanceService, ERP.Application.Finance.OpeningBalanceService>();
        services.AddScoped<ERP.Application.Finance.IAccountingPeriodService, ERP.Application.Finance.AccountingPeriodService>();
        services.AddScoped<ERP.Application.Finance.IChequeBookService, ERP.Application.Finance.ChequeBookService>();
        services.AddScoped<ERP.Application.Finance.IExpenseClaimService, ERP.Application.Finance.ExpenseClaimService>();
        services.AddScoped<ERP.Application.Finance.IFinanceGapService, ERP.Application.Finance.FinanceGapService>();
        services.AddScoped<CreateTaxCodeValidator>();
        services.AddScoped<UpdateTaxCodeValidator>();
        return services;
    }

    public static IServiceCollection AddErpInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IStockBalanceRepository, StockBalanceRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IStockTransferRepository, StockTransferRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IStockLedgerRepository, StockLedgerRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemRepository, ItemRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.SCM.IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemCategoryRepository, ItemCategoryRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.SCM.IStockAdjustmentRepository, ERP.Infrastructure.Repositories.SCM.StockAdjustmentRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IInventoryAdjustmentRepository, InventoryAdjustmentRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IBarcodeRepository, BarcodeRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IBatchNumberRepository, BatchNumberRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.ISerialNumberRepository, SerialNumberRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IReorderRuleRepository, ReorderRuleRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemGroupRepository, ItemGroupRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemAttributeRepository, ItemAttributeRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemVariantRepository, ItemVariantRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemPricingRepository, ItemPricingRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IInventoryValuationMethodRepository, InventoryValuationMethodRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IWarehouseRepository, WarehouseRepositoryAdapter>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IItemRepository, ApplicationItemRepositoryAdapter>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IWarehouseRepository, ApplicationWarehouseRepositoryAdapter>();
        services.AddScoped<ERP.Shared.Mappers.Inventory.BarcodeMapper>();
        services.AddScoped<ERP.Shared.Mappers.Inventory.BatchNumberMapper>();

        services.AddScoped<ERP.Application.Interfaces.IStockService, StockService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IStockTransferService, StockTransferService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IWarehouseService, WarehouseService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IItemCategoryService, ItemCategoryService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IStockAdjustmentService, StockAdjustmentService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IInventoryService, ERP.Application.SCM.Services.InventoryService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IItemService, ERP.Application.SCM.Services.ItemService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IWarehouseService, ERP.Application.SCM.Services.WarehouseService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IBarcodeService, ERP.Application.SCM.Services.BarcodeService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IBatchNumberService, ERP.Application.SCM.Services.BatchNumberService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.ISerialNumberService, ERP.Application.SCM.Services.SerialNumberService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IReorderRuleService, ERP.Application.SCM.Services.ReorderRuleService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IUnitOfMeasureService, ERP.Application.SCM.Services.UnitOfMeasureService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IItemGroupService, ERP.Application.SCM.Services.ItemGroupService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IItemAttributeService, ERP.Application.SCM.Services.ItemAttributeService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IItemVariantService, ERP.Application.SCM.Services.ItemVariantService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IItemPricingService, ERP.Application.SCM.Services.ItemPricingService>();
        services.AddScoped<ERP.Application.SCM.Interfaces.IInventoryValuationMethodService, ERP.Application.SCM.Services.InventoryValuationMethodService>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IBrandRepository, ERP.Infrastructure.Repositories.SCM.BrandRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IWarehouseLocationRepository, ERP.Infrastructure.Repositories.SCM.WarehouseLocationRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IStockReservationRepository, ERP.Infrastructure.Repositories.SCM.StockReservationRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IItemKitRepository, ERP.Infrastructure.Repositories.SCM.ItemKitRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.ICycleCountRepository, ERP.Infrastructure.Repositories.SCM.CycleCountRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IPutawayTaskRepository, ERP.Infrastructure.Repositories.SCM.PutawayTaskRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IPickListRepository, ERP.Infrastructure.Repositories.SCM.PickListRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IReplenishmentTaskRepository, ERP.Infrastructure.Repositories.SCM.ReplenishmentTaskRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IPackOrderRepository, ERP.Infrastructure.Repositories.SCM.PackOrderRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.SCM.IShipmentRepository, ERP.Infrastructure.Repositories.SCM.ShipmentRepository>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IBrandService, ERP.Application.Inventory.BrandService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IWarehouseLocationService, ERP.Application.Inventory.WarehouseLocationService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IStockReservationService, ERP.Application.Inventory.StockReservationService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IItemKitService, ERP.Application.Inventory.ItemKitService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.ICycleCountService, ERP.Application.Inventory.CycleCountService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IPutawayTaskService, ERP.Application.Inventory.PutawayTaskService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IPickListService, ERP.Application.Inventory.PickListService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IReplenishmentTaskService, ERP.Application.Inventory.ReplenishmentTaskService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IShipmentService, ERP.Application.Inventory.ShipmentService>();
        services.AddScoped<ERP.Application.Interfaces.Inventory.IPackOrderService, ERP.Application.Inventory.PackOrderService>();
        return services;
    }

    public static IServiceCollection AddErpHrmModule(this IServiceCollection services)
    {
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IPayrollRepository, PayrollRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IPayrollLineRepository, PayrollLineRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IBenefitRepository, BenefitRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IPerformanceReviewRepository, PerformanceReviewRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.ITrainingRepository, TrainingRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IJobOpeningRepository, JobOpeningRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.ICandidateRepository, CandidateRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<ERP.Infrastructure.Repositories.HRM.IInterviewRepository, InterviewRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.IEmployeeRepository, EmployeeRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.ILeaveRequestRepository, LeaveRequestRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.IPayrollRepository, PayrollRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.IPerformanceReviewRepository, PerformanceReviewRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.IJobOpeningRepository, JobOpeningRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.IJobApplicationRepository, JobApplicationRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.ICandidateRepository, CandidateRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.IInterviewRepository, InterviewRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.ITimeEntryRepository, TimeEntryRepositoryAdapter>();
        services.AddScoped<ERP.Infrastructure.Interfaces.HRM.ITrainingRepository, TrainingRepositoryAdapter>();

        services.AddScoped<ERP.Application.HRM.IEmployeeService, EmployeeService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.ILeaveService, LeaveService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.IPayrollService, PayrollCrudService>();
        services.AddScoped<ERP.Application.HRM.IPayrollService, PayrollService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.IPerformanceService, PerformanceService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.IRecruitmentService, RecruitmentService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.ITimeTrackingService, TimeTrackingService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.ITrainingService, TrainingService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.IAttendanceService, AttendanceService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.IBenefitsService, BenefitsService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.IDepartmentService, DepartmentService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.ILeaveTypeService, LeaveTypeService>();
        services.AddScoped<ERP.Application.Interfaces.HRM.ILeaveBalanceService, LeaveBalanceService>();
        services.AddScoped<ERP.Application.HRM.IEmployeeAdvanceService, ERP.Application.HRM.EmployeeAdvanceService>();
        services.AddScoped<ERP.Application.HRM.ITeamService, ERP.Application.HRM.TeamService>();
        return services;
    }

    public static IServiceCollection AddErpProcurementModule(this IServiceCollection services)
    {
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IVendorBankAccountRepository, VendorBankAccountRepository>();
        services.AddScoped<IRequestForQuotationRepository, RequestForQuotationRepository>();
        services.AddScoped<IRFQDetailRepository, RFQDetailRepository>();
        services.AddScoped<IRFQVendorResponseRepository, RFQVendorResponseRepository>();
        services.AddScoped<IRFQVendorResponseDetailRepository, RFQVendorResponseDetailRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IPurchaseOrderDetailRepository, PurchaseOrderDetailRepository>();
        services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();
        services.AddScoped<IPurchaseInvoiceDetailRepository, PurchaseInvoiceDetailRepository>();
        services.AddScoped<IPurchaseRequisitionRepository, PurchaseRequisitionRepository>();
        services.AddScoped<IPurchaseRequisitionDetailRepository, PurchaseRequisitionDetailRepository>();
        services.AddScoped<IGoodsReceiptNoteRepository, GoodsReceiptNoteRepository>();
        services.AddScoped<IGoodsReceiptNoteDetailRepository, GoodsReceiptNoteDetailRepository>();
        services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
        services.AddScoped<IPurchaseDebitNoteRepository, PurchaseDebitNoteRepository>();
        services.AddScoped<IPurchaseDebitNoteLineRepository, PurchaseDebitNoteLineRepository>();

        services.AddScoped<ERP.Shared.Mappers.Procurement.VendorMapper>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IVendorService, VendorService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IPurchaseRequisitionService, PurchaseRequisitionService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IGoodsReceiptNoteService, GoodsReceiptNoteService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IRequestForQuotationService, RequestForQuotationService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IPurchaseReturnService, PurchaseReturnService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IPurchaseDebitNoteService, PurchaseDebitNoteService>();
        services.AddScoped<ERP.Infrastructure.Interfaces.Procurement.ILandedCostVoucherRepository, ERP.Infrastructure.Repositories.Procurement.LandedCostVoucherRepository>();
        services.AddScoped<ERP.Infrastructure.Interfaces.Procurement.IVendorContractRepository, ERP.Infrastructure.Repositories.Procurement.VendorContractRepository>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.ILandedCostVoucherService, ERP.Application.Procurement.LandedCostVoucherService>();
        services.AddScoped<ERP.Application.Interfaces.Procurement.IVendorContractService, ERP.Application.Procurement.VendorContractService>();
        services.AddScoped<CreatePurchaseDebitNoteValidator>();
        services.AddScoped<UpdatePurchaseDebitNoteValidator>();
        return services;
    }

    public static IServiceCollection AddErpManufacturingModule(this IServiceCollection services)
    {
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
        services.AddScoped<IProductionRunRepository, ProductionRunRepository>();
        services.AddScoped<IProductionOrderLineRepository, ProductionOrderLineRepository>();
        services.AddScoped<IBillOfMaterialRepository, BillOfMaterialRepository>();
        services.AddScoped<ERP.Application.Interfaces.Manufacturing.IProductionOrderService, ProductionOrderService>();
        services.AddScoped<ERP.Application.Interfaces.Manufacturing.IWorkCenterService, WorkCenterService>();
        services.AddScoped<ERP.Application.Interfaces.Manufacturing.IProductionRunService, ProductionRunService>();
        services.AddScoped<ERP.Application.Interfaces.Manufacturing.IBillOfMaterialService, BillOfMaterialService>();
        services.AddScoped<ERP.Application.Manufacturing.IRoutingService, ERP.Application.Manufacturing.RoutingService>();
        services.AddScoped<ERP.Application.Manufacturing.IProductionScrapService, ERP.Application.Manufacturing.ProductionScrapService>();
        services.AddScoped<ERP.Application.Manufacturing.IScrapReasonService, ERP.Application.Manufacturing.ScrapReasonService>();
        services.AddScoped<ERP.Application.Manufacturing.IQualityCheckService, ERP.Application.Manufacturing.QualityCheckService>();
        services.AddScoped<ERP.Application.Manufacturing.IManufacturingReleaseService, ERP.Application.Manufacturing.ManufacturingReleaseService>();
        return services;
    }

    public static IServiceCollection AddErpProjectsModule(this IServiceCollection services)
    {
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
        services.AddScoped<IProjectMilestoneRepository, ProjectMilestoneRepository>();
        services.AddScoped<IProjectResourceRepository, ProjectResourceRepository>();
        services.AddScoped<IProjectDocumentRepository, ProjectDocumentRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddSingleton<ERP.Shared.Abstractions.IProjectDocumentStorage>(sp =>
        {
            var env = sp.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            var root = env != null
                ? System.IO.Path.Combine(env.ContentRootPath, "App_Data", "project-docs")
                : System.IO.Path.Combine(System.IO.Path.GetTempPath(), "erp-project-docs");
            return new ERP.Infrastructure.Storage.LocalProjectDocumentStorage(root);
        });
        services.AddScoped<ERP.Application.Interfaces.Projects.IProjectService, ProjectService>();
        services.AddScoped<ERP.Application.Interfaces.Projects.IProjectTaskService, ProjectTaskService>();
        services.AddScoped<ERP.Application.Interfaces.Projects.IProjectMilestoneService, ProjectMilestoneService>();
        services.AddScoped<ERP.Application.Interfaces.Projects.IProjectResourceService, ProjectResourceService>();
        services.AddScoped<ERP.Application.Interfaces.Projects.IProjectDocumentService, ProjectDocumentService>();
        services.AddScoped<ERP.Application.Interfaces.Projects.IProjectMemberService, ProjectMemberService>();
        services.AddScoped<ERP.Application.Projects.IProjectTimesheetService, ERP.Application.Projects.ProjectTimesheetService>();
        services.AddScoped<ERP.Application.Projects.IProjectMilestoneBillingService, ERP.Application.Projects.ProjectMilestoneBillingService>();
        services.AddScoped<ERP.Application.Projects.IProjectCostService, ERP.Application.Projects.ProjectCostService>();
        return services;
    }

    public static IServiceCollection AddErpReportsModule(this IServiceCollection services)
    {
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportScheduleRepository, ReportScheduleRepository>();
        services.AddScoped<IReportExecutionRepository, ReportExecutionRepository>();
        services.AddScoped<ERP.Application.Reports.IModuleReportQueryService, ERP.Application.Reports.ModuleReportQueryService>();
        services.AddScoped<ERP.Application.Interfaces.Reports.IReportService, ReportService>();
        services.AddScoped<ERP.Application.Interfaces.Reports.IOperationalReportService, OperationalReportService>();
        services.AddHostedService<ERP.Api.Workers.ReportScheduleWorker>();
        return services;
    }
}
