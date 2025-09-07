using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface IPOSSessionRepository
{
    Task<POSSession?> GetByIdAsync(int id);
    Task<IEnumerable<POSSession>> GetAllAsync();
    Task<POSSession> AddAsync(POSSession entity);
    Task UpdateAsync(POSSession entity);
    Task DeleteAsync(int id);
}