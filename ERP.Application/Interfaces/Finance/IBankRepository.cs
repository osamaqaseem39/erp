using ERP.Domain.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IBankRepository
{
    // Bank Accounts
    Task<BankAccount?> GetByIdAsync(Guid bankAccountId, Guid tenantId);
    Task<IEnumerable<BankAccount>> GetAllAsync(Guid tenantId);
    Task<BankAccount> AddAsync(BankAccount bankAccount);
    Task<BankAccount> UpdateAsync(BankAccount bankAccount);
    Task<bool> DeleteAsync(Guid bankAccountId, Guid tenantId);
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber, Guid tenantId);
    Task<IEnumerable<BankAccount>> GetByCurrencyAsync(string currencyCode, Guid tenantId);
    
    // Bank Transactions
    Task<BankTransaction?> GetTransactionByIdAsync(Guid bankTxId, Guid tenantId);
    Task<IEnumerable<BankTransaction>> GetTransactionsByAccountAsync(Guid bankAccountId, Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<BankTransaction>> GetUnmatchedTransactionsAsync(Guid bankAccountId, Guid tenantId);
    Task<IEnumerable<BankTransaction>> GetMatchedTransactionsAsync(Guid bankAccountId, Guid tenantId);
    Task<BankTransaction> AddTransactionAsync(BankTransaction bankTransaction);
    Task<BankTransaction> UpdateTransactionAsync(BankTransaction bankTransaction);
    Task<bool> DeleteTransactionAsync(Guid bankTxId, Guid tenantId);
    Task<bool> ImportTransactionsAsync(IEnumerable<BankTransaction> transactions, Guid tenantId, Guid userId);
    
    // Transaction Matching
    Task<bool> MatchTransactionToPaymentAsync(Guid bankTxId, Guid paymentId, Guid tenantId, Guid userId);
    Task<bool> UnmatchTransactionAsync(Guid bankTxId, Guid tenantId, Guid userId);
    Task<IEnumerable<BankTransaction>> GetMatchingSuggestionsAsync(Guid bankTxId, Guid tenantId);
    Task<IEnumerable<Payment>> GetMatchingPaymentsAsync(BankTransaction bankTransaction, Guid tenantId);
    
    // Bank Reconciliation
    Task<BankReconciliation?> GetReconciliationByIdAsync(Guid reconciliationId, Guid tenantId);
    Task<IEnumerable<BankReconciliation>> GetReconciliationsByAccountAsync(Guid bankAccountId, Guid tenantId);
    Task<BankReconciliation> CreateReconciliationAsync(BankReconciliation reconciliation);
    Task<BankReconciliation> UpdateReconciliationAsync(BankReconciliation reconciliation);
    Task<bool> CloseReconciliationAsync(Guid reconciliationId, Guid tenantId, Guid userId);
    Task<BankReconciliation?> GetOpenReconciliationAsync(Guid bankAccountId, Guid tenantId);
    
    // Bank Feeds
    Task<bool> ImportBankFeedAsync(Guid bankAccountId, string feedData, string feedFormat, Guid tenantId, Guid userId);
    Task<bool> ValidateBankFeedAsync(string feedData, string feedFormat);
    Task<IEnumerable<string>> GetSupportedFeedFormatsAsync();
    
    // Reports
    Task<object> GetReconciliationReportAsync(Guid bankAccountId, Guid tenantId, DateTime reconciliationDate);
    Task<object> GetUnmatchedTransactionsReportAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<object> GetBankStatementAsync(Guid bankAccountId, Guid tenantId, DateTime fromDate, DateTime toDate);
}