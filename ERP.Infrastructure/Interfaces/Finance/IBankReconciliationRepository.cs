using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IBankReconciliationRepository
{
    Task<BankReconciliation?> GetByIdAsync(int id);
    Task<IEnumerable<BankReconciliation>> GetAllAsync();
    Task<BankReconciliation> AddAsync(BankReconciliation entity);
    Task UpdateAsync(BankReconciliation entity);
    Task DeleteAsync(int id);
}