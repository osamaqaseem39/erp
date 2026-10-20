using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using HotChocolate;
using ICurrencyService = ERP.Application.Interfaces.ICurrencyService;
using IExchangeRateService = ERP.Application.Interfaces.IExchangeRateService;

namespace ERP.Api.GraphQL.Finance;

[ExtendObjectType("Mutation")]
public class FinanceMutations
{
    private readonly IAccountingService _accountingService;
    private readonly IInvoiceService _invoiceService;
    private readonly IBillService _billService;
    private readonly IPaymentService _paymentService;
    private readonly IBankService _bankService;
    private readonly IFixedAssetService _fixedAssetService;

    public FinanceMutations(
        IAccountingService accountingService,
        IInvoiceService invoiceService,
        IBillService billService,
        IPaymentService paymentService,
        IBankService bankService,
        IFixedAssetService fixedAssetService)
    {
        _accountingService = accountingService;
        _invoiceService = invoiceService;
        _billService = billService;
        _paymentService = paymentService;
        _bankService = bankService;
        _fixedAssetService = fixedAssetService;
    }

    // Chart of Accounts Mutations
    public async Task<Guid> AddChartOfAccount(CreateChartOfAccountDto input, Guid tenantId, Guid userId)
    {
        return await _accountingService.AddChartOfAccountAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateChartOfAccount(UpdateChartOfAccountDto input, Guid tenantId, Guid userId)
    {
        return await _accountingService.UpdateChartOfAccountAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteChartOfAccount(Guid accountId, Guid tenantId)
    {
        return await _accountingService.DeleteChartOfAccountAsync(accountId, tenantId);
    }

    // Journal Mutations
    public async Task<Guid> AddJournal(JournalCreateDto input, Guid tenantId, Guid userId)
    {
        return await _accountingService.AddJournalAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateJournal(JournalUpdateDto input, Guid tenantId, Guid userId)
    {
        return await _accountingService.UpdateJournalAsync(input, tenantId, userId);
    }

    public async Task<Guid> ApproveJournal(Guid journalId, Guid tenantId, Guid approvedBy)
    {
        return await _accountingService.ApproveJournalAsync(journalId, tenantId, approvedBy);
    }

    public async Task<Guid> RejectJournal(Guid journalId, Guid tenantId, Guid rejectedBy, string? reason = null)
    {
        return await _accountingService.RejectJournalAsync(journalId, tenantId, rejectedBy, reason);
    }

    public async Task<Guid> PostJournal(Guid journalId, Guid tenantId, Guid postedBy)
    {
        return await _accountingService.PostJournalAsync(journalId, tenantId, postedBy);
    }

    public async Task<Guid> UnpostJournal(Guid journalId, Guid tenantId, Guid unpostedBy)
    {
        return await _accountingService.UnpostJournalAsync(journalId, tenantId, unpostedBy);
    }

    public async Task<Guid> ReverseJournal(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null)
    {
        return await _accountingService.ReverseJournalAsync(journalId, tenantId, reversedBy, reversalReason);
    }

    public async Task<JournalDto?> SetJournalChequeStatus(
        Guid journalId,
        SetChequeStatusDto input,
        Guid tenantId,
        Guid userId)
    {
        var ok = await _accountingService.SetChequeStatusAsync(journalId, input, tenantId, userId);
        if (!ok) return null;
        return await _accountingService.GetJournalByIdAsync(journalId, tenantId);
    }

    public async Task<Guid> AddJournalEntry(CreateJournalEntryDto input, Guid journalId, Guid tenantId, Guid userId)
    {
        return await _accountingService.AddJournalEntryAsync(input, journalId, tenantId, userId);
    }

    public async Task<Guid> UpdateJournalEntry(UpdateJournalEntryDto input, Guid tenantId, Guid userId)
    {
        return await _accountingService.UpdateJournalEntryAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteJournalEntry(Guid entryId, Guid tenantId)
    {
        return await _accountingService.DeleteJournalEntryAsync(entryId, tenantId);
    }

    // Invoice Mutations
    public async Task<Guid> AddInvoice(InvoiceCreateDto input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.AddInvoiceAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateInvoice(InvoiceUpdateDto input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.UpdateInvoiceAsync(input, tenantId, userId);
    }

    public async Task<Guid> PostInvoice(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        return await _invoiceService.PostInvoiceAsync(invoiceId, tenantId, postedBy);
    }

    public async Task<bool> VoidInvoice(Guid invoiceId, VoidInvoiceDto? input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.VoidInvoiceAsync(
            invoiceId,
            input ?? new VoidInvoiceDto { InvoiceId = invoiceId },
            tenantId,
            userId);
    }

    public async Task<bool> SendInvoice(Guid invoiceId, Guid tenantId, Guid userId, CancellationToken ct = default)
        => await _invoiceService.SendInvoiceAsync(invoiceId, tenantId, userId, ct);

    public async Task<bool> DeleteInvoice(Guid invoiceId, Guid tenantId)
    {
        return await _invoiceService.DeleteInvoiceAsync(invoiceId, tenantId);
    }

    public async Task<Guid> AddInvoiceLine(InvoiceLineCreateDto input, Guid invoiceId, Guid tenantId, Guid userId)
    {
        return await _invoiceService.AddInvoiceLineAsync(input, invoiceId, tenantId, userId);
    }

    public async Task<Guid> UpdateInvoiceLine(InvoiceLineUpdateDto input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.UpdateInvoiceLineAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteInvoiceLine(Guid invoiceLineId, Guid tenantId)
    {
        return await _invoiceService.DeleteInvoiceLineAsync(invoiceLineId, tenantId);
    }

    // Bill Mutations
    public async Task<Guid> AddBill(BillCreateDto input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.AddBillAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateBill(BillUpdateDto input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.UpdateBillAsync(input, tenantId, userId);
    }

    public async Task<Guid> PostBill(Guid billId, Guid tenantId, Guid postedBy)
    {
        return await _invoiceService.PostBillAsync(billId, tenantId, postedBy);
    }

    public async Task<BillDto> ApproveBill(Guid billId, Guid tenantId, Guid userId)
    {
        return await _billService.ApproveAsync(billId, tenantId, userId);
    }

    public async Task<BillDto> MarkBillAsPaid(Guid billId, PaymentDto paymentDto, Guid tenantId, Guid userId)
    {
        return await _billService.MarkAsPaidAsync(billId, paymentDto, tenantId, userId);
    }

    public async Task<BillDto> VoidBill(Guid billId, VoidBillDto? input, Guid tenantId, Guid userId)
    {
        return await _billService.VoidBillAsync(
            billId,
            input ?? new VoidBillDto { BillId = billId },
            tenantId,
            userId);
    }

    public async Task<bool> DeleteBill(Guid billId, Guid tenantId)
    {
        return await _invoiceService.DeleteBillAsync(billId, tenantId);
    }

    public async Task<Guid> AddBillLine(BillLineCreateDto input, Guid billId, Guid tenantId, Guid userId)
    {
        return await _invoiceService.AddBillLineAsync(input, billId, tenantId, userId);
    }

    public async Task<Guid> UpdateBillLine(BillLineUpdateDto input, Guid tenantId, Guid userId)
    {
        return await _invoiceService.UpdateBillLineAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteBillLine(Guid billLineId, Guid tenantId)
    {
        return await _invoiceService.DeleteBillLineAsync(billLineId, tenantId);
    }

    // Payment Mutations
    public async Task<Guid> AddPayment(CreatePaymentDto input, Guid tenantId, Guid userId)
    {
        return await _paymentService.AddPaymentAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdatePayment(UpdatePaymentDto input, Guid tenantId, Guid userId)
    {
        return await _paymentService.UpdatePaymentAsync(input, tenantId, userId);
    }

    public async Task<bool> ProcessPayment(Guid paymentId, Guid tenantId, Guid userId)
    {
        return await _paymentService.ProcessPaymentAsync(paymentId, tenantId, userId);
    }

    public async Task<bool> ReconcilePayment(Guid paymentId, Guid tenantId, Guid userId)
    {
        return await _paymentService.ReconcilePaymentAsync(paymentId, tenantId, userId);
    }

    public async Task<bool> ApplyPaymentToInvoice(PaymentApplicationDto input, Guid tenantId, Guid userId)
    {
        return await _paymentService.ApplyPaymentToInvoiceAsync(input, tenantId, userId);
    }

    public async Task<bool> ApplyPaymentToBill(PaymentApplicationDto input, Guid tenantId, Guid userId)
    {
        return await _paymentService.ApplyPaymentToBillAsync(input, tenantId, userId);
    }

    public async Task<PaymentDto?> VoidPayment(Guid paymentId, VoidPaymentDto? input, Guid tenantId, Guid userId)
    {
        var ok = await _paymentService.VoidAsync(
            paymentId,
            input ?? new VoidPaymentDto { PaymentId = paymentId },
            tenantId,
            userId);
        if (!ok) return null;
        return await _paymentService.GetByIdAsync(paymentId, tenantId);
    }

    public async Task<PaymentDto?> RefundPayment(Guid paymentId, RefundDto? input, Guid tenantId, Guid userId)
    {
        var dto = input ?? new RefundDto { PaymentId = paymentId };
        dto.PaymentId = paymentId;
        var ok = await _paymentService.RefundAsync(paymentId, dto, tenantId, userId);
        if (!ok) return null;
        return await _paymentService.GetByIdAsync(paymentId, tenantId);
    }

    // Bank master Mutations
    public async Task<Guid> AddBank(CreateBankDto input, Guid tenantId, Guid userId)
        => await _bankService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateBank(UpdateBankDto input, Guid tenantId, Guid userId)
        => await _bankService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteBank(Guid bankId, Guid tenantId)
        => await _bankService.DeleteAsync(bankId, tenantId);

    // Bank Account Mutations
    public async Task<Guid> AddBankAccount(CreateBankAccountDto input, Guid tenantId, Guid userId)
    {
        return await _bankService.AddBankAccountAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateBankAccount(UpdateBankAccountDto input, Guid tenantId, Guid userId)
    {
        return await _bankService.UpdateBankAccountAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteBankAccount(Guid bankAccountId, Guid tenantId)
    {
        return await _bankService.DeleteBankAccountAsync(bankAccountId, tenantId);
    }

    // Bank Transaction Mutations
    public async Task<Guid> AddBankTransaction(CreateBankTransactionDto input, Guid tenantId, Guid userId)
    {
        return await _bankService.AddBankTransactionAsync(input, tenantId, userId);
    }

    public async Task<bool> ImportBankTransactions(Guid bankAccountId, List<CreateBankTransactionDto> transactions, Guid tenantId, Guid userId)
    {
        return await _bankService.ImportBankTransactionsAsync(bankAccountId, transactions, tenantId, userId);
    }

    public async Task<bool> MatchBankTransactionToPayment(BankTransactionMatchDto input, Guid tenantId, Guid userId)
    {
        return await _bankService.MatchBankTransactionToPaymentAsync(input, tenantId, userId);
    }

    public async Task<bool> UnmatchBankTransaction(Guid bankTxId, Guid tenantId, Guid userId)
    {
        return await _bankService.UnmatchBankTransactionAsync(bankTxId, tenantId, userId);
    }

    // Bank Reconciliation Mutations
    public async Task<Guid> CreateReconciliation(CreateBankReconciliationDto input, Guid tenantId, Guid userId)
    {
        return await _bankService.CreateReconciliationAsync(input, tenantId, userId);
    }

    public async Task<bool> SetTransactionReconciled(
        Guid reconciliationId,
        Guid bankTransactionId,
        bool reconciled,
        Guid tenantId,
        Guid userId)
        => await _bankService.SetTransactionReconciledAsync(
            reconciliationId, bankTransactionId, reconciled, tenantId, userId);

    public async Task<bool> CloseReconciliation(Guid reconciliationId, Guid tenantId, Guid userId)
    {
        return await _bankService.CloseReconciliationAsync(reconciliationId, tenantId, userId);
    }

    // Fixed Asset Mutations
    public async Task<Guid> AddFixedAsset(CreateFixedAssetDto input, Guid tenantId, Guid userId)
    {
        return await _fixedAssetService.AddAssetAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateFixedAsset(UpdateFixedAssetDto input, Guid tenantId, Guid userId)
    {
        return await _fixedAssetService.UpdateAssetAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteFixedAsset(Guid assetId, Guid tenantId)
    {
        return await _fixedAssetService.DeleteAssetAsync(assetId, tenantId);
    }

    public async Task<bool> DisposeAsset(Guid assetId, decimal disposalValue, DateTime disposalDate, Guid tenantId, Guid userId)
    {
        return await _fixedAssetService.DisposeAssetAsync(assetId, disposalValue, disposalDate, tenantId, userId);
    }

    public async Task<bool> UndoDisposal(Guid assetId, Guid tenantId, Guid userId)
    {
        return await _fixedAssetService.UndoDisposalAsync(assetId, tenantId, userId);
    }

    // Depreciation Mutations
    public async Task<bool> RunDepreciation(DepreciationRunDto input, Guid tenantId, Guid userId)
    {
        return await _fixedAssetService.RunDepreciationAsync(input, tenantId, userId);
    }

    public async Task<bool> PostDepreciationForPeriod(DateTime periodStart, DateTime periodEnd, Guid tenantId, Guid userId)
    {
        return await _fixedAssetService.PostDepreciationForPeriodAsync(periodStart, periodEnd, tenantId, userId);
    }

    public async Task<decimal> CalculateDepreciation(
        Guid assetId,
        DateTime periodStart,
        DateTime periodEnd,
        Guid tenantId)
        => await _fixedAssetService.CalculateDepreciationAsync(assetId, periodStart, periodEnd, tenantId);

    // Tax codes
    public async Task<Guid> AddTaxCode(
        [Service] ITaxCodeService service,
        CreateTaxCodeDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateTaxCode(
        [Service] ITaxCodeService service,
        UpdateTaxCodeDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteTaxCode(
        [Service] ITaxCodeService service,
        Guid taxCodeId,
        Guid tenantId)
        => await service.DeleteAsync(taxCodeId, tenantId);

    // Cost centers
    public async Task<Guid> AddCostCenter(
        [Service] ICostCenterService service,
        CreateCostCenterDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<Guid> UpdateCostCenter(
        [Service] ICostCenterService service,
        UpdateCostCenterDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteCostCenter(
        [Service] ICostCenterService service,
        Guid costCenterId,
        Guid tenantId)
        => await service.DeleteAsync(costCenterId, tenantId);

    public async Task<bool> SoftDeactivateCostCenter(
        [Service] ICostCenterService service,
        Guid costCenterId,
        Guid tenantId)
        => await service.SoftDeactivateAsync(costCenterId, tenantId);

    // Account purpose defaults
    public async Task<IEnumerable<AccountDefaultDto>> UpsertAccountDefaults(
        [Service] IAccountDefaultService service,
        UpsertAccountDefaultsDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpsertAsync(input, tenantId, userId);

    // Currencies
    public async Task<CurrencyDto> AddCurrency(
        [Service] ICurrencyService service,
        CreateCurrencyDto input)
        => await service.CreateAsync(input);

    public async Task<CurrencyDto> UpdateCurrency(
        [Service] ICurrencyService service,
        UpdateCurrencyDto input)
        => await service.UpdateAsync(input);

    public async Task<bool> DeleteCurrency(
        [Service] ICurrencyService service,
        string currencyCode)
        => await service.DeleteAsync(currencyCode);

    // Exchange rates
    public async Task<ExchangeRateDto> AddExchangeRate(
        [Service] IExchangeRateService service,
        CreateExchangeRateDto input)
        => await service.CreateAsync(input);

    public async Task<ExchangeRateDto> UpdateExchangeRate(
        [Service] IExchangeRateService service,
        UpdateExchangeRateDto input)
        => await service.UpdateAsync(input);

    public async Task<bool> DeleteExchangeRate(
        [Service] IExchangeRateService service,
        Guid exchangeRateId)
        => await service.DeleteAsync(exchangeRateId);
}