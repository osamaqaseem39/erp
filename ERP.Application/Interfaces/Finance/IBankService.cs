using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IBankService
{
    Task<BankDto?> GetByIdAsync(Guid id);
    Task<BankDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BankDto>> GetAllAsync();
    Task<IEnumerable<BankDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateBankDto dto);
    Task<Guid> CreateAsync(CreateBankDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateBankDto dto);
    Task<bool> UpdateAsync(UpdateBankDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BankDto>> GetActiveAsync();
    Task<IEnumerable<BankDto>> GetActiveAsync(Guid tenantId);
    Task<IEnumerable<BankDto>> SearchAsync(string searchTerm);
    Task<IEnumerable<BankDto>> SearchAsync(string searchTerm, Guid tenantId);
    
    // Bank Account methods
    Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(Guid bankId);
    Task<BankAccountDto?> GetBankAccountByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BankAccountDto>> GetAllBankAccountsAsync(Guid tenantId);
    Task<Guid> CreateBankAccountAsync(CreateBankAccountDto dto);
    Task<bool> UpdateBankAccountAsync(UpdateBankAccountDto dto);
    Task<bool> DeleteBankAccountAsync(Guid id);
    
    // Additional methods for GraphQL mutations
    Task<Guid> AddBankAccountAsync(CreateBankAccountDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateBankAccountAsync(UpdateBankAccountDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteBankAccountAsync(Guid bankAccountId, Guid tenantId);
    
    // Bank Transaction methods
    Task<Guid> AddBankTransactionAsync(CreateBankTransactionDto dto, Guid tenantId, Guid userId);
    Task<bool> ImportBankTransactionsAsync(Guid bankAccountId, List<CreateBankTransactionDto> transactions, Guid tenantId, Guid userId);
    Task<IEnumerable<BankTransactionDto>> GetBankTransactionsAsync(
        Guid bankAccountId, Guid tenantId, bool? unreconciledOnly = null, DateTime? toDate = null);
    Task<bool> MatchBankTransactionToPaymentAsync(BankTransactionMatchDto dto, Guid tenantId, Guid userId);
    Task<bool> UnmatchBankTransactionAsync(Guid bankTxId, Guid tenantId, Guid userId);
    
    // Bank Reconciliation methods
    Task<Guid> CreateReconciliationAsync(CreateBankReconciliationDto dto, Guid tenantId, Guid userId);
    Task<bool> CloseReconciliationAsync(Guid reconciliationId, Guid tenantId, Guid userId);
    Task<BankReconciliationDto?> GetReconciliationAsync(Guid reconciliationId, Guid tenantId);
    Task<IEnumerable<BankReconciliationDto>> ListReconciliationsAsync(Guid bankAccountId, Guid tenantId, string? status = null);
    Task<bool> SetTransactionReconciledAsync(Guid reconciliationId, Guid bankTxId, bool reconciled, Guid tenantId, Guid userId);
}