using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IVendorBankAccountRepository
{
    Task<VendorBankAccount?> GetByIdAsync(int id);
    Task<IEnumerable<VendorBankAccount>> GetAllAsync();
    Task<VendorBankAccount> AddAsync(VendorBankAccount entity);
    Task UpdateAsync(VendorBankAccount entity);
    Task DeleteAsync(int id);
}