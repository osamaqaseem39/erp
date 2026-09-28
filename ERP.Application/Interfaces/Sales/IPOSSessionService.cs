using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Interfaces.Sales;

public interface IPOSSessionService
{
    Task<POSSessionDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSSessionDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePOSSessionDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePOSSessionDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSSessionDto>> GetByCashierAsync(Guid cashierId, Guid tenantId);
    Task<IEnumerable<POSSessionDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<bool> CloseSessionAsync(ClosePOSSessionDto dto, Guid tenantId, Guid userId);
}

