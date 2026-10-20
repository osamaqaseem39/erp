using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using HotChocolate;
using ICurrencyService = ERP.Application.Interfaces.ICurrencyService;
using IExchangeRateService = ERP.Application.Interfaces.IExchangeRateService;

namespace ERP.Api.GraphQL.Finance;

[ExtendObjectType("Query")]
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

    // Bills (AP)
    public async Task<BillDto?> GetBill(Guid billId, Guid tenantId)
        => await _invoiceService.GetBillByIdAsync(billId, tenantId);

    public async Task<PagedResultType<BillDto>> SearchBills(
        Guid tenantId,
        Guid? vendorId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var query = new BillQuery
        {
            TenantId = tenantId,
            VendorId = vendorId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (bills, totalCount) = await _invoiceService.SearchBillsAsync(query);
        return new PagedResultType<BillDto>
        {
            Items = bills.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<BillLineDto>> GetBillLines(Guid billId, Guid tenantId)
        => await _invoiceService.GetBillLinesByBillIdAsync(billId, tenantId);

    // Payments
    public async Task<PaymentType?> GetPayment(Guid paymentId, Guid tenantId)
    {
        var payment = await _paymentService.GetByIdAsync(paymentId, tenantId);
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
        if (tenantId == Guid.Empty)
        {
            return new PagedResultType<PaymentType>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        IEnumerable<PaymentDto> payments = await _paymentService.GetAllAsync(tenantId);
        if (!string.IsNullOrWhiteSpace(status))
            payments = payments.Where(p => string.Equals(p.Status, status, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(paymentMethod))
            payments = payments.Where(p => string.Equals(p.PaymentMethod, paymentMethod, StringComparison.OrdinalIgnoreCase));
        if (fromDate.HasValue)
            payments = payments.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue)
            payments = payments.Where(p => p.PaymentDate <= toDate.Value);

        var list = payments.ToList();
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 200);
        var items = list.Skip((page - 1) * size).Take(size).Select(MapToPaymentType).ToList();

        return new PagedResultType<PaymentType>
        {
            Items = items,
            TotalCount = list.Count,
            PageNumber = page,
            PageSize = size
        };
    }

    // Banks (master)
    public async Task<BankDto?> GetBank(Guid bankId, Guid tenantId)
        => await _bankService.GetByIdAsync(bankId, tenantId);

    public async Task<IEnumerable<BankDto>> GetBanks(Guid tenantId, bool activeOnly = false)
        => activeOnly
            ? await _bankService.GetActiveAsync(tenantId)
            : await _bankService.GetAllAsync(tenantId);

    public async Task<IEnumerable<BankDto>> SearchBanks(string searchTerm, Guid tenantId)
        => await _bankService.SearchAsync(searchTerm, tenantId);

    // Bank Accounts
    public async Task<BankAccountType?> GetBankAccount(Guid bankAccountId, Guid tenantId)
    {
        var account = await _bankService.GetBankAccountByIdAsync(bankAccountId, tenantId);
        return account != null ? MapToBankAccountType(account) : null;
    }

    public async Task<List<BankAccountType>> GetAllBankAccounts(Guid tenantId)
    {
        var accounts = await _bankService.GetAllBankAccountsAsync(tenantId);
        return accounts.Select(MapToBankAccountType).ToList();
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

    public async Task<List<DepreciationEntryType>> GetDepreciationEntries(Guid assetId, Guid tenantId)
    {
        var entries = await _fixedAssetService.GetDepreciationEntriesAsync(assetId, tenantId);
        return entries.Select(MapToDepreciationEntryType).ToList();
    }

    public async Task<List<DepreciationEntryType>> GetDepreciationEntriesByPeriod(
        Guid tenantId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var entries = await _fixedAssetService.GetDepreciationEntriesByPeriodAsync(tenantId, periodStart, periodEnd);
        return entries.Select(MapToDepreciationEntryType).ToList();
    }

    public async Task<IEnumerable<string>> GetAvailableDepreciationMethods()
        => await _fixedAssetService.GetAvailableDepreciationMethodsAsync();

    public async Task<decimal> GetAssetNetBookValue(Guid assetId, DateTime asOfDate, Guid tenantId)
        => await _fixedAssetService.GetAssetNetBookValueAsync(assetId, asOfDate, tenantId);

    public async Task<object> GetAssetValuationReport(Guid tenantId, DateTime asOfDate)
        => await _fixedAssetService.GetAssetValuationReportAsync(tenantId, asOfDate);

    public async Task<object> GetDepreciationSchedule(Guid assetId, Guid tenantId)
        => await _fixedAssetService.GetDepreciationScheduleAsync(assetId, tenantId);

    public async Task<IEnumerable<string>> GetAssetCategories()
        => await _fixedAssetService.GetAssetCategoriesAsync();

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

    public async Task<ChequeRegisterReportDto> GetChequeRegister(
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? chequeStatus = null,
        string? journalType = null,
        string? search = null)
        => await _accountingService.GetChequeRegisterAsync(
            tenantId, fromDate, toDate, chequeStatus, journalType, search);

    // Tax codes
    public async Task<TaxCodeDto?> GetTaxCode(
        [Service] ITaxCodeService service,
        Guid taxCodeId,
        Guid tenantId)
        => await service.GetByIdAsync(taxCodeId, tenantId);

    public async Task<IEnumerable<TaxCodeDto>> GetTaxCodes(
        [Service] ITaxCodeService service,
        Guid tenantId,
        string? taxType = null,
        bool activeOnly = false)
    {
        if (!string.IsNullOrWhiteSpace(taxType))
            return await service.GetByTypeAsync(taxType, tenantId);
        if (activeOnly)
            return await service.GetActiveAsync(tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Cost centers
    public async Task<CostCenterDto?> GetCostCenter(
        [Service] ICostCenterService service,
        Guid costCenterId,
        Guid tenantId)
        => await service.GetByIdAsync(costCenterId, tenantId);

    public async Task<IEnumerable<CostCenterDto>> GetCostCenters(
        [Service] ICostCenterService service,
        Guid tenantId,
        bool? isActive = null)
        => await service.GetAllAsync(tenantId, isActive);

    // Account purpose defaults + last-used voucher defaults
    public async Task<IEnumerable<AccountDefaultDto>> GetAccountDefaults(
        [Service] IAccountDefaultService service,
        Guid tenantId)
        => await service.GetAllAsync(tenantId);

    public async Task<AccountDefaultDto?> GetAccountDefaultByPurpose(
        [Service] IAccountDefaultService service,
        string purpose,
        Guid tenantId)
        => await service.GetByPurposeAsync(purpose, tenantId);

    public async Task<VoucherDefaultsDto> GetVoucherDefaults(Guid tenantId)
        => await _accountingService.GetVoucherDefaultsAsync(tenantId);

    // Bank reconciliations
    public async Task<BankReconciliationDto?> GetReconciliation(Guid reconciliationId, Guid tenantId)
        => await _bankService.GetReconciliationAsync(reconciliationId, tenantId);

    public async Task<IEnumerable<BankReconciliationDto>> ListReconciliations(
        Guid bankAccountId,
        Guid tenantId,
        string? status = null)
        => await _bankService.ListReconciliationsAsync(bankAccountId, tenantId, status);

    // Currencies
    public async Task<CurrencyDto?> GetCurrency(
        [Service] ICurrencyService service,
        string currencyCode)
        => await service.GetByIdAsync(currencyCode);

    public async Task<IEnumerable<CurrencyDto>> GetCurrencies(
        [Service] ICurrencyService service,
        bool activeOnly = false)
        => activeOnly ? await service.GetActiveAsync() : await service.GetAllAsync();

    public async Task<bool> CurrencyExists(
        [Service] ICurrencyService service,
        string currencyCode)
        => await service.ExistsAsync(currencyCode);

    // Exchange rates
    public async Task<ExchangeRateDto?> GetExchangeRate(
        [Service] IExchangeRateService service,
        Guid exchangeRateId)
        => await service.GetByIdAsync(exchangeRateId);

    public async Task<IEnumerable<ExchangeRateDto>> GetExchangeRates(
        [Service] IExchangeRateService service)
        => await service.GetAllAsync();

    public async Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesByCurrency(
        [Service] IExchangeRateService service,
        string fromCurrency,
        string toCurrency)
        => await service.GetByCurrencyAsync(fromCurrency, toCurrency);

    public async Task<ExchangeRateDto?> GetCurrentExchangeRate(
        [Service] IExchangeRateService service,
        string fromCurrency,
        string toCurrency)
        => await service.GetCurrentRateAsync(fromCurrency, toCurrency);

    public async Task<decimal> ConvertAmount(
        [Service] IExchangeRateService service,
        decimal amount,
        string fromCurrency,
        string toCurrency,
        DateTime? asOfDate = null)
        => await service.ConvertAmountAsync(amount, fromCurrency, toCurrency, asOfDate);

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
            InvoiceNumber = dto.InvoiceNumber,
            EntityReferenceTable = dto.EntityReferenceTable,
            EntityReferenceId = dto.EntityReferenceId,
            JournalEntries = (dto.JournalEntries ?? []).Select(MapToJournalEntryType).ToList()
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

    private static List<TrialBalanceType> MapToTrialBalanceType(TrialBalanceReportDto trialBalance)
    {
        return trialBalance.Lines.Select(l => new TrialBalanceType
        {
            AccountCode = l.AccountCode,
            AccountName = l.AccountName,
            AccountType = l.AccountType,
            DebitBalance = l.Debit,
            CreditBalance = l.Credit,
            NetBalance = l.Balance
        }).ToList();
    }

    private static List<AgedReceivablesType> MapToAgedReceivablesType(AgingReportDto agedReceivables)
    {
        return agedReceivables.Lines
            .GroupBy(l => l.PartnerId)
            .Select(g =>
            {
                var partnerLines = g.ToList();
                return new AgedReceivablesType
                {
                    CustomerId = g.Key,
                    Current = partnerLines.Where(x => x.Bucket == "Current").Sum(x => x.Balance),
                    Days30 = partnerLines.Where(x => x.Bucket is "1-30").Sum(x => x.Balance),
                    Days60 = partnerLines.Where(x => x.Bucket is "31-60").Sum(x => x.Balance),
                    Days90 = partnerLines.Where(x => x.Bucket is "61-90").Sum(x => x.Balance),
                    Over90 = partnerLines.Where(x => x.Bucket.StartsWith("Over ", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Balance),
                    Total = partnerLines.Sum(x => x.Balance)
                };
            })
            .ToList();
    }
}