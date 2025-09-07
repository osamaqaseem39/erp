using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Services;

public interface IBankApiService : IApiService<BankDto, CreateBankDto, UpdateBankDto>
{
    Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync(Guid bankId);
    Task<Guid> CreateBankAccountAsync(CreateBankAccountDto dto);
    Task<bool> UpdateBankAccountAsync(UpdateBankAccountDto dto);
    Task<bool> DeleteBankAccountAsync(Guid id);
}