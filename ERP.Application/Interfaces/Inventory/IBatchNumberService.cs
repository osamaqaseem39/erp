using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IBatchNumberService
{
    Task<BatchNumberDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BatchNumberDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateBatchNumberDto dto);
    Task<bool> UpdateAsync(UpdateBatchNumberDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<BatchNumberDto?> GetByBatchNumberAsync(string batchNumber);
    Task<IEnumerable<BatchNumberDto>> GetByItemAsync(Guid itemId);
}