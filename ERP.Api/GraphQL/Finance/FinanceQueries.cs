using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.GraphQL.Finance;

public class FinanceQueries
{
    private readonly IAccountingService _accountingService;
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly IBankService _bankService;
    private readonly IFixedAssetService _fixedAssetService;

    public FinanceQueries(
        IAccountingService accountingService,
        IInvoiceService invoiceService,
        IPaymentService paymentService,
        IBankService bankService,
        IFixedAssetService fixedAssetService)
    {
        _accountingService = accountingService;
        _invoiceService = invoiceService;
        _paymentService = paymentService;
        _bankService = bankService;
        _fixedAssetService = fixedAssetService;
    }

    // Chart of Accounts
    public async Task<ChartOfAccountType?> GetChartOfAccount(Guid accountId, Guid tenantId)
    {
        var account = await _accountingService.GetChartOfAccountByIdAsync(accountId, tenantId);
        return account != null ? MapToChartOfAccountType(account) : null;
    }

    public async Task<List<ChartOfAccountType>> GetAllChartOfAccounts(Guid tenantId, string? accountType = null, bool? isActive = null)
    {
        var accounts = await _accountingService.GetAllChartOfAccountsAsync(tenantId, accountType, isActive);
        return accounts.Select(MapToChartOfAccountType).ToList();
    }

    // Journals
    public async Task<JournalType?> GetJournal(Guid journalId, Guid tenantId)
    {
        var journal = await _accountingService.GetJournalByIdAsync(journalId, tenantId);
        return journal != null ? MapToJournalType(journal) : null;
    }

    public async Task<PagedResultType<JournalType>> SearchJournals(
        Guid tenantId, 
        string? journalType = null, 
        string? status = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = new JournalQuery
        {
            TenantId = tenantId,
            JournalType = journalType,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (journals, totalCount) = await _accountingService.SearchJournalsAsync(query);
        
        return new PagedResultType<JournalType>
        {
            Items = journals.Select(MapToJournalType).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Invoices
    public async Task<InvoiceType?> GetInvoice(Guid invoiceId, Guid tenantId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId, tenantId);
        return invoice != null ? MapToInvoiceType(invoice) : null;
    }

    public async Task<PagedResultType<InvoiceType>> SearchInvoices(
        Guid tenantId,
        Guid? customerId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = new InvoiceQuery
        {
            TenantId = tenantId,
            CustomerId = customerId,
            Status = status,
            StartDate = fromDate,
            EndDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (invoices, totalCount) = await _invoiceService.SearchInvoicesAsync(query);
        
        return new PagedResultType<InvoiceType>
        {
            Items = invoices.Select(MapToInvoiceType).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Payments
    public async Task<PaymentType?> GetPayment(Guid paymentId, Guid tenantId)
    {
        var payment = await _paymentService.GetByIdAsync(paymentId);
        return payment != null ? MapToPaymentType(payment) : null;
    }

    public async Task<PagedResultType<PaymentType>> SearchPayments(
        Guid tenantId,
        string? status = null,
        string? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = new PaymentQuery
        {
            TenantId = tenantId,
            Status = status,
            PaymentMethod = paymentMethod,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var payments = await _paymentService.GetAllAsync();
        var totalCount = payments.Count();
        
        return new PagedResultType<PaymentType>
        {
            Items = payments.Select(MapToPaymentType).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Bank Accounts
    public async Task<BankAccountType?> GetBankAccount(Guid bankAccountId, Guid tenantId)
    {
        // Note: IBankService doesn't have GetBankAccountByIdAsync method
        // This would need to be implemented or use a different approach
        return null;
    }

    public async Task<List<BankAccountType>> GetAllBankAccounts(Guid tenantId)
    {
        // Note: IBankService doesn't have GetAllBankAccountsAsync method
        // This would need to be implemented or use a different approach
        return new List<BankAccountType>();
    }

    // Fixed Assets
    public async Task<FixedAssetType?> GetFixedAsset(Guid assetId, Guid tenantId)
    {
        var asset = await _fixedAssetService.GetAssetByIdAsync(assetId, tenantId);
        return asset != null ? MapToFixedAssetType(asset) : null;
    }

    public async Task<PagedResultType<FixedAssetType>> SearchFixedAssets(
        Guid tenantId,
        string? depreciationMethod = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = new FixedAssetQuery
        {
            TenantId = tenantId,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (assets, totalCount) = await _fixedAssetService.SearchAssetsAsync(query);
        
        return new PagedResultType<FixedAssetType>
        {
            Items = assets.Select(MapToFixedAssetType).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Financial Reports
    public async Task<List<TrialBalanceType>> GetTrialBalance(Guid tenantId, DateTime fromDate, DateTime toDate, string? currencyCode = null)
    {
        var trialBalance = await _accountingService.GetTrialBalanceAsync(tenantId, fromDate, toDate, currencyCode);
        return MapToTrialBalanceType(trialBalance);
    }

    public async Task<List<AgedReceivablesType>> GetAgedReceivables(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        var agedReceivables = await _invoiceService.GetAgedReceivablesAsync(tenantId, asOfDate, agingBuckets);
        return MapToAgedReceivablesType(agedReceivables);
    }

    public async Task<object> GetCustomerAging(Guid customerId, Guid tenantId, DateTime asOfDate)
    {
        return await _invoiceService.GetCustomerAgingAsync(customerId, tenantId, asOfDate);
    }

    public async Task<object> GetGLBalance(Guid accountId, Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        return await _accountingService.GetGeneralLedgerBalanceAsync(accountId, tenantId, fromDate, toDate);
    }

    // Mapping methods
    private static ChartOfAccountType MapToChartOfAccountType(ChartOfAccountDto dto)
    {
        return new ChartOfAccountType
        {
            AccountId = dto.AccountId ?? Guid.Empty,
            TenantId = dto.TenantId ?? Guid.Empty,
            AccountCode = dto.AccountCode,
            AccountName = dto.AccountName,
            AccountType = dto.AccountType,
            ParentAccountId = dto.ParentAccountId,
            ParentAccountName = dto.ParentAccountName,
            CurrencyCode = dto.CurrencyCode,
            IsActive = dto.IsActive ?? false,
            IsPostingAllowed = dto.IsPostingAllowed ?? false,
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = dto.UpdatedAt,
            CreatedBy = dto.CreatedBy,
            UpdatedBy = dto.UpdatedBy
        };
    }

    private static JournalType MapToJournalType(JournalDto dto)
    {
        return new JournalType
        {
            JournalId = dto.JournalId ?? Guid.NewGuid(),
            TenantId = dto.TenantId ?? Guid.NewGuid(),
            Type = dto.JournalType,
            ReferenceNo = dto.ReferenceNo,
            JournalDate = dto.JournalDate ?? DateTime.UtcNow,
            Description = dto.Description ?? string.Empty,
            CurrencyCode = dto.CurrencyCode ?? string.Empty,
            ExchangeRate = dto.ExchangeRate ?? 1.0m,
            TotalDebit = dto.TotalDebit ?? 0m,
            TotalCredit = dto.TotalCredit ?? 0m,
            Status = dto.Status ?? string.Empty,
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
            CreatedBy = dto.CreatedBy,
            PostedAt = dto.PostedAt,
            PostedBy = dto.PostedBy,
            IsLocked = dto.IsLocked ?? false,
            JournalEntries = dto.JournalEntries.Select(MapToJournalEntryType).ToList()
        };
    }

    private static JournalEntryType MapToJournalEntryType(JournalEntryDto dto)
    {
        return new JournalEntryType
        {
            EntryId = dto.EntryId,
            JournalId = dto.JournalId,
            TenantId = dto.TenantId,
            AccountId = dto.AccountId,
            AccountCode = dto.AccountCode,
            AccountName = dto.AccountName,
            CostCenterId = dto.CostCenterId,
            CostCenterName = dto.CostCenterName,
            ProjectId = dto.ProjectId,
            Debit = dto.Debit,
            Credit = dto.Credit,
            Narration = dto.Narration,
            EntityReferenceTable = dto.EntityReferenceTable,
            EntityReferenceId = dto.EntityReferenceId,
            CreatedAt = dto.CreatedAt,
            CreatedBy = Guid.TryParse(dto.CreatedBy, out var createdByGuid) ? createdByGuid : null
        };
    }

    private static InvoiceType MapToInvoiceType(InvoiceDto dto)
    {
        return new InvoiceType
        {
            InvoiceId = dto.InvoiceId,
            TenantId = dto.TenantId,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            InvoiceNumber = dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            CurrencyCode = dto.CurrencyCode,
            SubTotal = dto.SubTotal,
            TaxTotal = dto.TaxTotal,
            TotalAmount = dto.TotalAmount,
            Balance = dto.Balance,
            Status = dto.Status,
            SalesOrderId = dto.SalesOrderId,
            CreatedAt = dto.CreatedAt,
            CreatedBy = Guid.TryParse(dto.CreatedBy, out var createdByGuid) ? createdByGuid : null,
            PostedAt = dto.PostedAt,
            PostedBy = dto.PostedBy,
            IsLocked = dto.IsLocked,
            InvoiceLines = dto.InvoiceLines.Select(MapToInvoiceLineType).ToList()
        };
    }

    private static InvoiceLineType MapToInvoiceLineType(InvoiceLineDto dto)
    {
        return new InvoiceLineType
        {
            InvoiceLineId = dto.Id,
            InvoiceId = dto.InvoiceId,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            LineTotal = dto.TotalPrice,
            TaxRate = dto.TaxRate ?? 0m,
            GLAccountId = dto.GLAccountId ?? Guid.Empty,
            GLAccountCode = dto.GLAccountCode,
            GLAccountName = dto.GLAccountName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
    }

    private static PaymentType MapToPaymentType(PaymentDto dto)
    {
        return new PaymentType
        {
            PaymentId = dto.Id,
            TenantId = dto.TenantId,
            PaymentNumber = dto.PaymentNumber,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod,
            Amount = dto.Amount ?? 0m,
            CurrencyCode = dto.CurrencyCode,
            BankAccountId = dto.BankAccountId,
            BankAccountName = dto.BankAccountName,
            RelatedInvoiceId = dto.RelatedInvoiceId,
            RelatedInvoiceNumber = dto.RelatedInvoiceNumber,
            RelatedBillId = dto.RelatedBillId,
            RelatedBillNumber = dto.RelatedBillNumber,
            Status = dto.Status,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CreatedAt = dto.CreatedAt,
            CreatedBy = Guid.TryParse(dto.CreatedBy, out var createdByGuid) ? createdByGuid : null,
            ProcessedAt = dto.ProcessedAt,
            ProcessedBy = dto.ProcessedBy
        };
    }

    private static BankAccountType MapToBankAccountType(BankAccountDto dto)
    {
        return new BankAccountType
        {
            BankAccountId = dto.Id,
            TenantId = dto.TenantId,
            BankName = dto.BankName,
            AccountNumber = dto.AccountNumber,
            IBAN = dto.IBAN,
            SWIFT = dto.SWIFT,
            CurrencyCode = dto.CurrencyCode,
            GLAccountId = dto.GLAccountId,
            GLAccountCode = dto.GLAccountCode,
            GLAccountName = dto.GLAccountName,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            CreatedBy = Guid.TryParse(dto.CreatedBy, out var createdByGuid) ? createdByGuid : null
        };
    }

    private static FixedAssetType MapToFixedAssetType(FixedAssetDto dto)
    {
        return new FixedAssetType
        {
            AssetId = dto.Id,
            TenantId = dto.TenantId,
            AssetCode = dto.AssetCode,
            AssetName = dto.AssetName,
            Description = dto.Description,
            AcquisitionDate = dto.AcquisitionDate ?? DateTime.UtcNow,
            AcquisitionCost = dto.AcquisitionCost ?? 0m,
            DepreciationMethod = dto.DepreciationMethod,
            UsefulLifeMonths = dto.UsefulLifeMonths ?? 0,
            AccumulatedDepreciation = dto.AccumulatedDepreciation,
            NetBookValue = dto.NetBookValue ?? 0m,
            GLAccountId = dto.GLAccountId ?? Guid.Empty,
            GLAccountCode = dto.GLAccountCode,
            GLAccountName = dto.GLAccountName,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            CreatedBy = Guid.TryParse(dto.CreatedBy, out var createdByGuid) ? createdByGuid : null,
            DisposedAt = dto.DisposedAt,
            DisposedBy = Guid.TryParse(dto.DisposedBy, out var disposedByGuid) ? disposedByGuid : null,
            DepreciationEntries = dto.DepreciationEntries?.Select(MapToDepreciationEntryType).ToList() ?? new List<DepreciationEntryType>()
        };
    }

    private static DepreciationEntryType MapToDepreciationEntryType(DepreciationEntryDto dto)
    {
        return new DepreciationEntryType
        {
            DepreciationId = dto.Id,
            AssetId = dto.FixedAssetId,
            AssetName = dto.AssetName,
            TenantId = dto.TenantId,
            PeriodStart = dto.DepreciationDate,
            PeriodEnd = dto.DepreciationDate,
            DepreciationAmount = dto.DepreciationAmount,
            PostedJournalId = null, // DepreciationEntryDto doesn't have PostedJournalId
            CreatedAt = dto.CreatedAt,
            CreatedBy = Guid.TryParse(dto.CreatedBy, out var createdByGuid) ? createdByGuid : null
        };
    }

    private static List<TrialBalanceType> MapToTrialBalanceType(object trialBalance)
    {
        // Implementation depends on the actual structure returned by GetTrialBalanceAsync
        return new List<TrialBalanceType>();
    }

    private static List<AgedReceivablesType> MapToAgedReceivablesType(object agedReceivables)
    {
        // Implementation depends on the actual structure returned by GetAgedReceivablesAsync
        return new List<AgedReceivablesType>();
    }
}