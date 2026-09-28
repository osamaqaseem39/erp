using ERP.Domain.Sales;

namespace ERP.Infrastructure.Interfaces.Sales;

public interface IPOSSessionRepository
{
    Task<POSSession?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSSession>> GetAllAsync(Guid tenantId);
    Task<POSSession> AddAsync(POSSession entity);
    Task UpdateAsync(POSSession entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSSession>> GetByCashierAsync(Guid cashierId, Guid tenantId);
    Task<IEnumerable<POSSession>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> CloseSessionAsync(Guid sessionId, decimal closingBalance, string? notes, Guid tenantId, Guid userId);
}
