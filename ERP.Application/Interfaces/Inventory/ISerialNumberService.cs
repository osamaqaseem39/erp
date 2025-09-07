using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface ISerialNumberService
{
    Task<SerialNumberDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SerialNumberDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateSerialNumberDto dto);
    Task<bool> UpdateAsync(UpdateSerialNumberDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<SerialNumberDto?> GetBySerialNumberAsync(string serialNumber);
    Task<IEnumerable<SerialNumberDto>> GetByItemAsync(Guid itemId);
}