using ERP.Shared.DTOs.Manufacturing;

namespace ERP.Application.Interfaces.Manufacturing;

public interface IBillOfMaterialService
{
    Task<BillOfMaterialDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BillOfMaterialDto>> GetAllAsync(Guid tenantId, Guid? finishedGoodItemId = null);
    Task<BillOfMaterialDto> CreateAsync(CreateBillOfMaterialDto dto, Guid tenantId, Guid userId);
    Task<BillOfMaterialDto> UpdateAsync(UpdateBillOfMaterialDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<BillOfMaterialDto> ActivateAsync(Guid id, Guid tenantId, Guid userId);
    Task<BillOfMaterialDto> ObsoleteAsync(Guid id, Guid tenantId, Guid userId);
}
