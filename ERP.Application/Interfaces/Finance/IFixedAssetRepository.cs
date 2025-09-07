using ERP.Domain.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IFixedAssetRepository
{
    // Fixed Assets
    Task<FixedAsset?> GetByIdAsync(Guid assetId, Guid tenantId);
    Task<IEnumerable<FixedAsset>> GetAllAsync(Guid tenantId, string? depreciationMethod = null, bool? isActive = null, int pageNumber = 1, int pageSize = 50);
    Task<int> GetCountAsync(Guid tenantId, string? depreciationMethod = null, bool? isActive = null);
    Task<FixedAsset?> GetByAssetCodeAsync(string assetCode, Guid tenantId);
    Task<FixedAsset> AddAsync(FixedAsset fixedAsset);
    Task<FixedAsset> UpdateAsync(FixedAsset fixedAsset);
    Task<bool> DeleteAsync(Guid assetId, Guid tenantId);
    Task<bool> DisposeAssetAsync(Guid assetId, decimal disposalValue, DateTime disposalDate, Guid tenantId, Guid userId);
    Task<bool> UndoDisposalAsync(Guid assetId, Guid tenantId, Guid userId);
    
    // Depreciation
    Task<IEnumerable<DepreciationEntry>> GetDepreciationEntriesAsync(Guid assetId, Guid tenantId);
    Task<IEnumerable<DepreciationEntry>> GetDepreciationEntriesByPeriodAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd);
    Task<DepreciationEntry?> GetDepreciationEntryByIdAsync(Guid depreciationId, Guid tenantId);
    Task<DepreciationEntry> AddDepreciationEntryAsync(DepreciationEntry depreciationEntry);
    Task<DepreciationEntry> UpdateDepreciationEntryAsync(DepreciationEntry depreciationEntry);
    Task<bool> DeleteDepreciationEntryAsync(Guid depreciationId, Guid tenantId);
    Task<bool> PostDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd, Guid tenantId, Guid userId);
    
    // Depreciation Calculations
    Task<decimal> CalculateDepreciationAsync(Guid assetId, DateTime periodStart, DateTime periodEnd, Guid tenantId);
    Task<decimal> CalculateStraightLineDepreciationAsync(decimal cost, decimal salvageValue, int usefulLifeMonths, DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd);
    Task<decimal> CalculateReducingBalanceDepreciationAsync(decimal cost, decimal rate, decimal accumulatedDepreciation, DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd);
    Task<decimal> GetAssetNetBookValueAsync(Guid assetId, DateTime asOfDate, Guid tenantId);
    
    // Asset Categories
    Task<IEnumerable<string>> GetAssetCategoriesAsync();
    Task<bool> ValidateAssetCategoryAsync(string category);
    Task<IEnumerable<string>> GetDepreciationMethodsAsync();
    
    // Reports
    Task<object> GetAssetValuationReportAsync(Guid tenantId, DateTime asOfDate);
    Task<object> GetDepreciationScheduleAsync(Guid assetId, Guid tenantId);
    Task<object> GetAssetRegisterAsync(Guid tenantId, DateTime asOfDate);
    Task<object> GetDisposalReportAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
}