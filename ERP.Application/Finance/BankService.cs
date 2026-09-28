using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using System;
using System.Linq;

namespace ERP.Application.Finance;

/// <summary>DI stub for IBankService. Replace with real business logic incrementally.</summary>
public class BankService : IBankService
{
    public Task<BankDto?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<BankDto?>(null);
    }

    public Task<IEnumerable<BankDto>> GetAllAsync()
    {
        return Task.FromResult(Enumerable.Empty<BankDto>());
    }

    public Task<Guid> CreateAsync(CreateBankDto dto)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> UpdateAsync(UpdateBankDto dto)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<BankDto>> GetActiveAsync()
    {
        return Task.FromResult(Enumerable.Empty<BankDto>());
    }

    public Task<IEnumerable<BankDto>> SearchAsync(string searchTerm)
    {
        return Task.FromResult(Enumerable.Empty<BankDto>());
    }

    public Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(Guid bankId)
    {
        return Task.FromResult(Enumerable.Empty<BankAccountDto>());
    }

    public Task<Guid> CreateBankAccountAsync(CreateBankAccountDto dto)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> UpdateBankAccountAsync(UpdateBankAccountDto dto)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteBankAccountAsync(Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<Guid> AddBankAccountAsync(CreateBankAccountDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> UpdateBankAccountAsync(UpdateBankAccountDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> DeleteBankAccountAsync(Guid bankAccountId, Guid tenantId)
    {
        return Task.FromResult(false);
    }

    public Task<Guid> AddBankTransactionAsync(CreateBankTransactionDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> ImportBankTransactionsAsync(Guid bankAccountId, List<CreateBankTransactionDto> transactions, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<bool> MatchBankTransactionToPaymentAsync(BankTransactionMatchDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<bool> UnmatchBankTransactionAsync(Guid bankTxId, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<Guid> CreateReconciliationAsync(CreateBankReconciliationDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> CloseReconciliationAsync(Guid reconciliationId, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

}
