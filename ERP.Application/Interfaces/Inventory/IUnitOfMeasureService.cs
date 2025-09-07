using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.Interfaces.Inventory;

public interface IUnitOfMeasureService
{
    Task<UnitOfMeasureDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UnitOfMeasureDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateUnitOfMeasureDto dto);
    Task<bool> UpdateAsync(UpdateUnitOfMeasureDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<UnitOfMeasureDto>> GetActiveAsync();
    Task<UnitOfMeasureDto?> GetByCodeAsync(string code);
}