using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IIntercompanyTransactionRepository
{
    Task<IntercompanyTransaction?> GetByIdAsync(int id);
    Task<IEnumerable<IntercompanyTransaction>> GetAllAsync();
    Task<IntercompanyTransaction> AddAsync(IntercompanyTransaction entity);
    Task UpdateAsync(IntercompanyTransaction entity);
    Task DeleteAsync(int id);
}