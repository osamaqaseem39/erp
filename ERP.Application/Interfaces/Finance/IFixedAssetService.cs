using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IFixedAssetService
{
    // Fixed Assets
    Task<Guid> AddAssetAsync(CreateFixedAssetDto dto, Guid tenantId, Guid userId);
    Task<Guid> UpdateAssetAsync(UpdateFixedAssetDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAssetAsync(Guid assetId, Guid tenantId);
    Task<FixedAssetDto?> GetAssetByIdAsync(Guid assetId, Guid tenantId);
    Task<(IEnumerable<FixedAssetDto> Assets, int TotalCount)> SearchAssetsAsync(FixedAssetQuery query);
    
    // Asset Disposal
    Task<bool> DisposeAssetAsync(Guid assetId, decimal disposalValue, DateTime disposalDate, Guid tenantId, Guid userId);
    Task<bool> UndoDisposalAsync(Guid assetId, Guid tenantId, Guid userId);
    
    // Depreciation
    Task<bool> RunDepreciationAsync(DepreciationRunDto dto, Guid tenantId, Guid userId);
    Task<IEnumerable<DepreciationEntryDto>> GetDepreciationEntriesAsync(Guid assetId, Guid tenantId);
    Task<IEnumerable<DepreciationEntryDto>> GetDepreciationEntriesByPeriodAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd);
    Task<bool> PostDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd, Guid tenantId, Guid userId);
    
    // Depreciation Methods
    Task<IEnumerable<string>> GetAvailableDepreciationMethodsAsync();
    Task<decimal> CalculateDepreciationAsync(Guid assetId, DateTime periodStart, DateTime periodEnd, Guid tenantId);
    Task<decimal> CalculateStraightLineDepreciationAsync(decimal cost, decimal salvageValue, int usefulLifeMonths, DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd);
    Task<decimal> CalculateReducingBalanceDepreciationAsync(decimal cost, decimal rate, decimal accumulatedDepreciation, DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd);
    
    // Asset Valuation
    Task<decimal> GetAssetNetBookValueAsync(Guid assetId, DateTime asOfDate, Guid tenantId);
    Task<object> GetAssetValuationReportAsync(Guid tenantId, DateTime asOfDate);
    Task<object> GetDepreciationScheduleAsync(Guid assetId, Guid tenantId);
    
    // Asset Categories
    Task<IEnumerable<string>> GetAssetCategoriesAsync();
    Task<bool> ValidateAssetCategoryAsync(string category);
}