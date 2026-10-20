using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public class FixedAssetService : IFixedAssetService
{
    private static readonly string[] Methods = ["StraightLine", "ReducingBalance"];
    private readonly ERPDbContext _db;
    private readonly IAccountingService? _accounting;

    public FixedAssetService(ERPDbContext db, IAccountingService? accounting = null)
    {
        _db = db;
        _accounting = accounting;
    }

    public async Task<Guid> AddAssetAsync(CreateFixedAssetDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (string.IsNullOrWhiteSpace(dto.AssetName)) return Guid.Empty;
        if (!dto.PurchasePrice.HasValue || dto.PurchasePrice <= 0) return Guid.Empty;
        if (!dto.PurchaseDate.HasValue) return Guid.Empty;

        var glId = dto.GLAccountId ?? Guid.Empty;
        if (glId == Guid.Empty) return Guid.Empty;
        var accountExists = await _db.ChartOfAccounts.AnyAsync(a =>
            a.AccountId == glId && a.TenantId == tenantId);
        if (!accountExists) return Guid.Empty;

        var category = string.IsNullOrWhiteSpace(dto.AssetType) ? "General" : dto.AssetType.Trim();
        var code = string.IsNullOrWhiteSpace(dto.AssetCode)
            ? $"FA-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : dto.AssetCode.Trim();

        if (await _db.FixedAssets.AnyAsync(a => a.TenantId == tenantId && a.AssetCode == code))
            return Guid.Empty;

        var id = Guid.NewGuid();
        var cost = Math.Round(dto.PurchasePrice.Value, 2);
        _db.FixedAssets.Add(new FixedAsset
        {
            Id = id,
            AssetId = id,
            TenantId = tenantId,
            AssetCode = code,
            AssetName = dto.AssetName.Trim(),
            AssetCategory = category,
            AccountId = glId,
            GLAccountAccountId = glId,
            PurchaseCost = cost,
            CurrentValue = cost,
            PurchaseDate = dto.PurchaseDate.Value.Date,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            CurrencyCode = "USD",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId.ToString()
        });
        await _db.SaveChangesAsync();
        return id;
    }

    public async Task<Guid> UpdateAssetAsync(UpdateFixedAssetDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var id = dto.AssetId ?? dto.Id;
        if (id == Guid.Empty) return Guid.Empty;

        var entity = await LoadAsync(id, tenantId);
        if (entity == null) return Guid.Empty;
        if (string.Equals(entity.Status, "Disposed", StringComparison.OrdinalIgnoreCase))
            return Guid.Empty;

        if (!string.IsNullOrWhiteSpace(dto.AssetCode)
            && !string.Equals(dto.AssetCode.Trim(), entity.AssetCode, StringComparison.OrdinalIgnoreCase))
        {
            var clash = await _db.FixedAssets.AnyAsync(a =>
                a.TenantId == tenantId && a.AssetCode == dto.AssetCode.Trim() && a.AssetId != id);
            if (clash) return Guid.Empty;
            entity.AssetCode = dto.AssetCode.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.AssetName)) entity.AssetName = dto.AssetName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.AssetType)) entity.AssetCategory = dto.AssetType.Trim();
        if (dto.GLAccountId.HasValue && dto.GLAccountId != Guid.Empty)
        {
            var ok = await _db.ChartOfAccounts.AnyAsync(a =>
                a.AccountId == dto.GLAccountId && a.TenantId == tenantId);
            if (!ok) return Guid.Empty;
            entity.AccountId = dto.GLAccountId.Value;
            entity.GLAccountAccountId = dto.GLAccountId.Value;
        }
        if (dto.PurchasePrice.HasValue && dto.PurchasePrice > 0)
        {
            var delta = Math.Round(dto.PurchasePrice.Value - entity.PurchaseCost, 2);
            entity.PurchaseCost = Math.Round(dto.PurchasePrice.Value, 2);
            entity.CurrentValue = Math.Round(entity.CurrentValue + delta, 2);
        }
        if (dto.PurchaseDate.HasValue) entity.PurchaseDate = dto.PurchaseDate.Value.Date;
        if (!string.IsNullOrWhiteSpace(dto.Status)) entity.Status = dto.Status.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _db.SaveChangesAsync();
        return entity.AssetId;
    }

    public async Task<bool> DeleteAssetAsync(Guid assetId, Guid tenantId)
    {
        var entity = await LoadAsync(assetId, tenantId);
        if (entity == null) return false;
        if (await _db.DepreciationEntries.AnyAsync(d => d.AssetId == assetId))
            return false;
        _db.FixedAssets.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<FixedAssetDto?> GetAssetByIdAsync(Guid assetId, Guid tenantId)
    {
        var entity = await LoadAsync(assetId, tenantId);
        return entity == null ? null : await MapAsync(entity);
    }

    public async Task<(IEnumerable<FixedAssetDto> Assets, int TotalCount)> SearchAssetsAsync(FixedAssetQuery query)
    {
        var tenantId = query.TenantId;
        if (tenantId == Guid.Empty) return (Array.Empty<FixedAssetDto>(), 0);
        var q = _db.FixedAssets.AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query.AssetCode))
            q = q.Where(a => a.AssetCode.Contains(query.AssetCode));
        if (!string.IsNullOrWhiteSpace(query.AssetName))
            q = q.Where(a => a.AssetName.Contains(query.AssetName));
        if (!string.IsNullOrWhiteSpace(query.Category))
            q = q.Where(a => a.AssetCategory == query.Category);
        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(a => a.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            q = q.Where(a => a.AssetCode.Contains(term) || a.AssetName.Contains(term));
        }
        if (query.IsActive.HasValue) q = q.Where(a => a.IsActive == query.IsActive.Value);
        if (query.PurchaseDateFrom.HasValue) q = q.Where(a => a.PurchaseDate >= query.PurchaseDateFrom.Value.Date);
        if (query.PurchaseDateTo.HasValue) q = q.Where(a => a.PurchaseDate <= query.PurchaseDateTo.Value.Date);
        if (query.PurchaseValueFrom.HasValue) q = q.Where(a => a.PurchaseCost >= query.PurchaseValueFrom.Value);
        if (query.PurchaseValueTo.HasValue) q = q.Where(a => a.PurchaseCost <= query.PurchaseValueTo.Value);

        var total = await q.CountAsync();
        var page = Math.Max(1, query.PageNumber);
        var size = Math.Clamp(query.PageSize <= 0 ? 50 : query.PageSize, 1, 500);
        var rows = await q.OrderBy(a => a.AssetCode)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var mapped = new List<FixedAssetDto>();
        foreach (var row in rows)
            mapped.Add(await MapAsync(row));
        return (mapped, total);
    }

    public async Task<bool> DisposeAssetAsync(
        Guid assetId, decimal disposalValue, DateTime disposalDate, Guid tenantId, Guid userId)
    {
        var entity = await LoadAsync(assetId, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Disposed", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.Status = "Disposed";
        entity.DisposalDate = disposalDate.Date;
        entity.CurrentValue = Math.Round(Math.Max(0, disposalValue), 2);
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UndoDisposalAsync(Guid assetId, Guid tenantId, Guid userId)
    {
        var entity = await LoadAsync(assetId, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Disposed", StringComparison.OrdinalIgnoreCase))
            return false;

        var accumulated = await AccumulatedAsync(assetId, tenantId);
        entity.Status = "Active";
        entity.DisposalDate = null;
        entity.CurrentValue = Math.Round(entity.PurchaseCost - accumulated, 2);
        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RunDepreciationAsync(DepreciationRunDto dto, Guid tenantId, Guid userId)
    {
        var end = dto.RunDate == default ? DateTime.UtcNow.Date : dto.RunDate.Date;
        var start = new DateTime(end.Year, end.Month, 1);
        return await PostDepreciationForPeriodAsync(start, end, tenantId, userId);
    }

    public async Task<IEnumerable<DepreciationEntryDto>> GetDepreciationEntriesAsync(Guid assetId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<DepreciationEntryDto>();
        var rows = await _db.DepreciationEntries.AsNoTracking()
            .Where(d => d.AssetId == assetId && d.TenantId == tenantId)
            .OrderBy(d => d.PeriodStart)
            .ToListAsync();
        var asset = await LoadAsync(assetId, tenantId);
        decimal running = 0;
        var list = new List<DepreciationEntryDto>();
        foreach (var d in rows)
        {
            running += d.DepreciationAmount;
            list.Add(MapDep(d, asset, running));
        }
        return list;
    }

    public async Task<IEnumerable<DepreciationEntryDto>> GetDepreciationEntriesByPeriodAsync(
        Guid tenantId, DateTime periodStart, DateTime periodEnd)
    {
        if (tenantId == Guid.Empty) return Array.Empty<DepreciationEntryDto>();
        var rows = await _db.DepreciationEntries.AsNoTracking()
            .Where(d => d.TenantId == tenantId
                        && d.PeriodStart >= periodStart.Date
                        && d.PeriodEnd <= periodEnd.Date)
            .OrderBy(d => d.PeriodStart)
            .ToListAsync();
        return rows.Select(d => MapDep(d, null, d.DepreciationAmount));
    }

    public async Task<bool> PostDepreciationForPeriodAsync(
        DateTime periodStart, DateTime periodEnd, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var start = periodStart.Date;
        var end = periodEnd.Date;
        if (end < start) return false;

        var assets = await _db.FixedAssets
            .Where(a => a.TenantId == tenantId
                        && a.IsActive
                        && a.Status == "Active")
            .ToListAsync();
        if (assets.Count == 0) return false;

        var created = new List<(DepreciationEntry Entry, FixedAsset Asset, decimal Amount)>();
        foreach (var asset in assets)
        {
            var exists = await _db.DepreciationEntries.AnyAsync(d =>
                d.AssetId == asset.AssetId && d.PeriodStart == start && d.PeriodEnd == end);
            if (exists) continue;

            var amount = await CalculateDepreciationAsync(asset.AssetId, start, end, tenantId);
            if (amount <= 0) continue;

            amount = Math.Min(amount, asset.CurrentValue);
            if (amount <= 0) continue;

            var entry = new DepreciationEntry
            {
                DepreciationId = Guid.NewGuid(),
                AssetId = asset.AssetId,
                TenantId = asset.TenantId,
                PeriodStart = start,
                PeriodEnd = end,
                DepreciationAmount = amount,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId == Guid.Empty ? null : userId
            };
            _db.DepreciationEntries.Add(entry);
            asset.CurrentValue = Math.Round(asset.CurrentValue - amount, 2);
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
            created.Add((entry, asset, amount));
        }

        if (created.Count == 0) return false;
        await _db.SaveChangesAsync();

        // Post GL: Dr DepreciationExpense / Cr AccumulatedDepreciation (per asset)
        if (_accounting != null)
        {
            var expenseId = await ResolvePurposeAccountAsync(AccountPurposes.DepreciationExpense, tenantId);
            var accumId = await ResolvePurposeAccountAsync(AccountPurposes.AccumulatedDepreciation, tenantId);
            if (expenseId != Guid.Empty && accumId != Guid.Empty && expenseId != accumId)
            {
                foreach (var (entry, asset, amount) in created)
                {
                    var journalId = await PostDepreciationJournalAsync(
                        asset, amount, expenseId, accumId, start, end, tenantId, userId);
                    if (journalId == Guid.Empty) continue;
                    entry.PostedJournalId = journalId;
                }
                await _db.SaveChangesAsync();
            }
        }

        return true;
    }

    private async Task<Guid> ResolvePurposeAccountAsync(string purpose, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var mapped = await _db.AccountDefaults.AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.Purpose == purpose
                && d.IsActive
                && d.TenantId == tenantId);
        if (mapped != null) return mapped.AccountId;

        // Fallback: CoA name/code heuristic so thin installs still post
        var accounts = await _db.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.IsActive && a.IsPostingAllowed)
            .ToListAsync();
        if (purpose == AccountPurposes.DepreciationExpense)
        {
            return accounts.FirstOrDefault(a =>
                       a.AccountType.Equals("Expense", StringComparison.OrdinalIgnoreCase)
                       && a.AccountName.Contains("depreciation", StringComparison.OrdinalIgnoreCase))
                   ?.AccountId
                   ?? Guid.Empty;
        }

        return accounts.FirstOrDefault(a =>
                   a.AccountName.Contains("accumulated depreciation", StringComparison.OrdinalIgnoreCase))
               ?.AccountId
               ?? Guid.Empty;
    }

    private async Task<Guid> PostDepreciationJournalAsync(
        FixedAsset asset,
        decimal amount,
        Guid expenseAccountId,
        Guid accumAccountId,
        DateTime periodStart,
        DateTime periodEnd,
        Guid tenantId,
        Guid userId)
    {
        var journalId = await _accounting!.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "Adjustment",
            JournalDate = periodEnd,
            Description = $"Depreciation {asset.AssetCode} {periodStart:yyyy-MM-dd}–{periodEnd:yyyy-MM-dd}",
            ReferenceNo = $"DEP-{asset.AssetCode}-{periodEnd:yyyyMM}"
        }, tenantId, userId);
        if (journalId == Guid.Empty) return Guid.Empty;

        var debit = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = expenseAccountId,
            Debit = amount,
            Credit = 0,
            Narration = $"Depreciation expense — {asset.AssetName}"
        }, journalId, tenantId, userId);
        var credit = await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
        {
            AccountId = accumAccountId,
            Debit = 0,
            Credit = amount,
            Narration = $"Accumulated depreciation — {asset.AssetName}"
        }, journalId, tenantId, userId);
        if (debit == Guid.Empty || credit == Guid.Empty) return Guid.Empty;

        var posted = await _accounting.PostJournalAsync(journalId, tenantId, userId, allowPostWithoutApprove: true);
        return posted == journalId ? journalId : Guid.Empty;
    }

    public Task<IEnumerable<string>> GetAvailableDepreciationMethodsAsync()
        => Task.FromResult<IEnumerable<string>>(Methods);

    public async Task<decimal> CalculateDepreciationAsync(
        Guid assetId, DateTime periodStart, DateTime periodEnd, Guid tenantId)
    {
        var asset = await LoadAsync(assetId, tenantId);
        if (asset == null) return 0m;
        const int usefulLifeMonths = 60;
        return await CalculateStraightLineDepreciationAsync(
            asset.PurchaseCost, 0m, usefulLifeMonths, asset.PurchaseDate, periodStart, periodEnd);
    }

    public Task<decimal> CalculateStraightLineDepreciationAsync(
        decimal cost, decimal salvageValue, int usefulLifeMonths,
        DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd)
    {
        if (usefulLifeMonths <= 0 || cost <= salvageValue) return Task.FromResult(0m);
        if (periodEnd.Date < acquisitionDate.Date || periodEnd.Date < periodStart.Date)
            return Task.FromResult(0m);

        var monthly = (cost - salvageValue) / usefulLifeMonths;
        var months = MonthsOverlapping(acquisitionDate.Date, periodStart.Date, periodEnd.Date);
        return Task.FromResult(Math.Round(monthly * months, 2));
    }

    public Task<decimal> CalculateReducingBalanceDepreciationAsync(
        decimal cost, decimal rate, decimal accumulatedDepreciation,
        DateTime acquisitionDate, DateTime periodStart, DateTime periodEnd)
    {
        if (rate <= 0 || cost <= 0) return Task.FromResult(0m);
        if (periodEnd.Date < acquisitionDate.Date || periodEnd.Date < periodStart.Date)
            return Task.FromResult(0m);

        var book = Math.Max(0, cost - accumulatedDepreciation);
        var months = MonthsOverlapping(acquisitionDate.Date, periodStart.Date, periodEnd.Date);
        var amount = book * (rate / 100m) * (months / 12m);
        return Task.FromResult(Math.Round(Math.Min(amount, book), 2));
    }

    public async Task<decimal> GetAssetNetBookValueAsync(Guid assetId, DateTime asOfDate, Guid tenantId)
    {
        var asset = await LoadAsync(assetId, tenantId);
        if (asset == null) return 0m;
        var dep = await _db.DepreciationEntries
            .Where(d => d.AssetId == assetId
                        && d.TenantId == tenantId
                        && d.PeriodEnd <= asOfDate.Date)
            .SumAsync(d => (decimal?)d.DepreciationAmount) ?? 0m;
        return Math.Round(Math.Max(0, asset.PurchaseCost - dep), 2);
    }

    public async Task<object> GetAssetValuationReportAsync(Guid tenantId, DateTime asOfDate)
    {
        if (tenantId == Guid.Empty)
            return new { AsOfDate = asOfDate.Date, TotalPurchaseCost = 0m, TotalNetBookValue = 0m, Assets = Array.Empty<object>() };
        var assets = await _db.FixedAssets.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderBy(a => a.AssetCode)
            .ToListAsync();
        var lines = new List<object>();
        decimal totalCost = 0, totalNbv = 0;
        foreach (var a in assets)
        {
            var nbv = await GetAssetNetBookValueAsync(a.AssetId, asOfDate, tenantId);
            totalCost += a.PurchaseCost;
            totalNbv += nbv;
            lines.Add(new
            {
                a.AssetId,
                a.AssetCode,
                a.AssetName,
                a.AssetCategory,
                a.Status,
                a.PurchaseCost,
                NetBookValue = nbv,
                AccumulatedDepreciation = Math.Round(a.PurchaseCost - nbv, 2)
            });
        }
        return new
        {
            AsOfDate = asOfDate.Date,
            TotalPurchaseCost = Math.Round(totalCost, 2),
            TotalNetBookValue = Math.Round(totalNbv, 2),
            Assets = lines
        };
    }

    public async Task<object> GetDepreciationScheduleAsync(Guid assetId, Guid tenantId)
    {
        var asset = await LoadAsync(assetId, tenantId);
        if (asset == null) return new { };
        var entries = await GetDepreciationEntriesAsync(assetId, tenantId);
        return new
        {
            asset.AssetId,
            asset.AssetCode,
            asset.AssetName,
            asset.PurchaseCost,
            NetBookValue = asset.CurrentValue,
            Entries = entries
        };
    }

    public async Task<IEnumerable<string>> GetAssetCategoriesAsync()
        => await _db.FixedAssets.AsNoTracking()
            .Select(a => a.AssetCategory)
            .Where(c => c != null && c != "")
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    public async Task<bool> ValidateAssetCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return false;
        return await _db.FixedAssets.AnyAsync(a => a.AssetCategory == category.Trim());
    }

    private async Task<FixedAsset?> LoadAsync(Guid assetId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.FixedAssets.FirstOrDefaultAsync(a =>
            a.AssetId == assetId && a.TenantId == tenantId);
    }

    private async Task<decimal> AccumulatedAsync(Guid assetId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return 0m;
        return await _db.DepreciationEntries
            .Where(d => d.AssetId == assetId && d.TenantId == tenantId)
            .SumAsync(d => (decimal?)d.DepreciationAmount) ?? 0m;
    }

    private async Task<FixedAssetDto> MapAsync(FixedAsset e)
    {
        var accumulated = await AccumulatedAsync(e.AssetId, e.TenantId);
        var account = await _db.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountId == e.AccountId);
        return new FixedAssetDto
        {
            Id = e.AssetId,
            AssetId = e.AssetId,
            TenantId = e.TenantId,
            AssetCode = e.AssetCode,
            AssetName = e.AssetName,
            AssetType = e.AssetCategory,
            GLAccountId = e.AccountId,
            GLAccountName = account?.AccountName,
            GLAccountCode = account?.AccountCode,
            PurchasePrice = e.PurchaseCost,
            AcquisitionCost = e.PurchaseCost,
            PurchaseDate = e.PurchaseDate,
            AcquisitionDate = e.PurchaseDate,
            Status = e.Status,
            AccumulatedDepreciation = Math.Round(accumulated, 2),
            BookValue = e.CurrentValue,
            NetBookValue = e.CurrentValue,
            DisposedAt = e.DisposalDate,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            CreatedBy = e.CreatedBy,
            UpdatedBy = e.UpdatedBy
        };
    }

    private static DepreciationEntryDto MapDep(DepreciationEntry d, FixedAsset? asset, decimal accumulated) => new()
    {
        Id = d.DepreciationId,
        TenantId = d.TenantId,
        FixedAssetId = d.AssetId,
        AssetName = asset?.AssetName ?? string.Empty,
        AssetCode = asset?.AssetCode ?? string.Empty,
        DepreciationDate = d.PeriodEnd,
        DepreciationAmount = d.DepreciationAmount,
        AccumulatedDepreciation = accumulated,
        BookValue = asset == null ? 0 : Math.Round(asset.PurchaseCost - accumulated, 2),
        Method = "StraightLine",
        Status = d.PostedJournalId.HasValue ? "Posted" : "Recorded",
        PostedAt = d.PostedJournalId.HasValue ? d.CreatedAt : null,
        CreatedAt = d.CreatedAt,
        CreatedBy = d.CreatedBy?.ToString()
    };

    private static int MonthsOverlapping(DateTime acquisition, DateTime periodStart, DateTime periodEnd)
    {
        var start = periodStart < acquisition ? acquisition : periodStart;
        if (periodEnd < start) return 0;
        var months = ((periodEnd.Year - start.Year) * 12) + periodEnd.Month - start.Month + 1;
        return Math.Max(0, months);
    }
}
