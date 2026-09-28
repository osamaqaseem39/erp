using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using System;
using System.Linq;

namespace ERP.Application.Finance;

/// <summary>DI stub for IFixedAssetService. Replace with real business logic incrementally.</summary>
public class FixedAssetService : IFixedAssetService
{
    public Task<Guid> AddAssetAsync(CreateFixedAssetDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<Guid> UpdateAssetAsync(UpdateFixedAssetDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> DeleteAssetAsync(Guid assetId, Guid tenantId)
    {
        return Task.FromResult(false);
    }

    public Task<FixedAssetDto?> GetAssetByIdAsync(Guid assetId, Guid tenantId)
    {
        return Task.FromResult<FixedAssetDto?>(null);
    }

    public Task<(IEnumerable<FixedAssetDto> Assets, int TotalCount)> SearchAssetsAsync(FixedAssetQuery query)
    {
        throw new NotImplementedException("FixedAssetService.SearchAssetsAsync is not yet implemented");
    }

    public Task<bool> DisposeAssetAsync(Guid assetId, decimal disposalValue, DateTime disposalDate, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<bool> UndoDisposalAsync(Guid assetId, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<bool> RunDepreciationAsync(DepreciationRunDto dto, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<DepreciationEntryDto>> GetDepreciationEntriesAsync(Guid assetId, Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<DepreciationEntryDto>());
    }

    public Task<IEnumerable<DepreciationEntryDto>> GetDepreciationEntriesByPeriodAsync(Guid tenantId, DateTime periodStart, DateTime periodEnd)
    {
        return Task.FromResult(Enumerable.Empty<DepreciationEntryDto>());
    }

    public Task<bool> PostDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd, Guid tenantId, Guid userId)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetAvailableDepreciationMethodsAsync()
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<decimal> CalculateDepreciationAsync(Guid assetId, DateTime periodStart, DateTime periodEnd, Guid tenantId)
    {
        return Task.FromResult(0m);
    }

    public Task<decimal> CalculateStraightLineDepreciationAsync(decimal cost, decimal salvageValue, int usefulLifeMonths, DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd)
    {
        return Task.FromResult(0m);
    }

    public Task<decimal> CalculateReducingBalanceDepreciationAsync(decimal cost, decimal rate, decimal accumulatedDepreciation, DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd)
    {
        return Task.FromResult(0m);
    }

    public Task<decimal> GetAssetNetBookValueAsync(Guid assetId, DateTime asOfDate, Guid tenantId)
    {
        return Task.FromResult(0m);
    }

    public Task<object> GetAssetValuationReportAsync(Guid tenantId, DateTime asOfDate)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<object> GetDepreciationScheduleAsync(Guid assetId, Guid tenantId)
    {
        return Task.FromResult<object>(new { });
    }

    public Task<IEnumerable<string>> GetAssetCategoriesAsync()
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<bool> ValidateAssetCategoryAsync(string category)
    {
        return Task.FromResult(false);
    }

}
