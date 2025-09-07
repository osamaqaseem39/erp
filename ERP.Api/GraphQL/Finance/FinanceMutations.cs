using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.GraphQL.Finance;

public class FinanceMutations
{
    private readonly IAccountingService _accountingService;
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly IBankService _bankService;
    private readonly IFixedAssetService _fixedAssetService;

    public FinanceMutations(
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

    public async Task<Guid> PostJournal(Guid journalId, Guid tenantId, Guid postedBy)
    {
        return await _accountingService.PostJournalAsync(journalId, tenantId, postedBy);
    }

    public async Task<Guid> ReverseJournal(Guid journalId, Guid tenantId, Guid reversedBy, string? reversalReason = null)
    {
        return await _accountingService.ReverseJournalAsync(journalId, tenantId, reversedBy, reversalReason);
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
}