using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IBatchNumberService
{
    Task<IEnumerable<BatchNumberDto>> GetByItemAsync(Guid itemId, Guid tenantId);
    Task<BatchNumberDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<BatchNumberDto> CreateAsync(BatchNumberDto dto, Guid tenantId, string createdBy);
    Task<BatchNumberDto> UpdateAsync(BatchNumberDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}