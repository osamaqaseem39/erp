using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IAccountDefaultService
{
    Task<IEnumerable<AccountDefaultDto>> GetAllAsync(Guid tenantId);
    Task<AccountDefaultDto?> GetByPurposeAsync(string purpose, Guid tenantId);
    Task<IEnumerable<AccountDefaultDto>> UpsertAsync(UpsertAccountDefaultsDto dto, Guid tenantId, Guid userId);
}
