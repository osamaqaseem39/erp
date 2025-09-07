using ERP.Shared.DTOs.Inventory;

namespace ERP.Application.SCM.Interfaces;

public interface IUnitOfMeasureService
{
    Task<IEnumerable<UnitOfMeasureDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<UnitOfMeasureDto> CreateAsync(UnitOfMeasureDto dto, Guid tenantId, string createdBy);
    Task<UnitOfMeasureDto> UpdateAsync(UnitOfMeasureDto dto, Guid tenantId, string updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}