using ERP.Shared.DTOs.Sales;

namespace ERP.Application.Interfaces.Sales;

public interface IPOSTransactionService
{
    Task<POSTransactionDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSTransactionDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePOSTransactionDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePOSTransactionDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<POSTransactionDto>> GetBySessionAsync(Guid sessionId, Guid tenantId);
    Task<IEnumerable<POSTransactionDto>> GetByCustomerAsync(Guid customerId, Guid tenantId);
    Task<IEnumerable<POSTransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
}

