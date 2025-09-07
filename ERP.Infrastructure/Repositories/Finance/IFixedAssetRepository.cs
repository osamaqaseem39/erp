using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IFixedAssetRepository
{
    Task<FixedAsset?> GetByIdAsync(Guid id);
    Task<IEnumerable<FixedAsset>> GetAllAsync();
    Task<IEnumerable<FixedAsset>> GetActiveAsync();
    Task<IEnumerable<FixedAsset>> GetByStatusAsync(string status);
    Task<FixedAsset?> GetByCodeAsync(string assetCode);
    Task<Guid> CreateAsync(FixedAsset fixedAsset);
    Task<bool> UpdateAsync(FixedAsset fixedAsset);
    Task<bool> DeleteAsync(Guid id);
}