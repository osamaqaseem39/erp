using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface ISerialNumberService
{
    Task<IEnumerable<SerialNumberDto>> GetByItemAsync(Guid itemId, Guid tenantId, string? status = null);
    Task<SerialNumberDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<SerialNumberDto> CreateAsync(SerialNumberDto dto, Guid tenantId, string createdBy);
    Task<SerialNumberDto> UpdateAsync(SerialNumberDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}