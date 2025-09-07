using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IBankService
{
    Task<BankDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BankDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateBankDto dto);
    Task<bool> UpdateAsync(UpdateBankDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<BankDto>> GetActiveAsync();
    Task<IEnumerable<BankDto>> SearchAsync(string searchTerm);
    
    // Bank Account methods
    Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(Guid bankId);
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
    Task<bool> MatchBankTransactionToPaymentAsync(BankTransactionMatchDto dto, Guid tenantId, Guid userId);
    Task<bool> UnmatchBankTransactionAsync(Guid bankTxId, Guid tenantId, Guid userId);
    
    // Bank Reconciliation methods
    Task<Guid> CreateReconciliationAsync(CreateBankReconciliationDto dto, Guid tenantId, Guid userId);
    Task<bool> CloseReconciliationAsync(Guid reconciliationId, Guid tenantId, Guid userId);
}