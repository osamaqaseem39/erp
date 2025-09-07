using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface IBankService
{
    Task<BankDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BankDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateBankDto dto);
    Task<bool> UpdateAsync(UpdateBankDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(Guid bankId);
    Task<Guid> CreateBankAccountAsync(CreateBankAccountDto dto);
    Task<bool> UpdateBankAccountAsync(UpdateBankAccountDto dto);
    Task<bool> DeleteBankAccountAsync(Guid id);
}