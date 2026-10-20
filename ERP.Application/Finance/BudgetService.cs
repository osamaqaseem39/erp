using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IBudgetService
{
    Task<IReadOnlyList<BudgetDto>> ListAsync(Guid tenantId);
    Task<BudgetDto?> GetAsync(Guid id, Guid tenantId);
    Task<BudgetDto> CreateAsync(CreateBudgetDto dto, Guid tenantId, Guid userId);
    Task<BudgetDto> UpdateAsync(UpdateBudgetDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ActivateAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId);
}

public class BudgetService : IBudgetService
{
    private readonly ERPDbContext _db;

    public BudgetService(ERPDbContext db) => _db = db;

    public async Task<IReadOnlyList<BudgetDto>> ListAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var rows = await _db.Budgets.AsNoTracking()
            .Include(b => b.Account)
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
        return rows.Select(Map).ToList();
    }

    public async Task<BudgetDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.Budgets.AsNoTracking()
            .Include(b => b.Account)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && (b.Id == id || b.BudgetId == id));
        return e == null ? null : Map(e);
    }

    public async Task<BudgetDto> CreateAsync(CreateBudgetDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (string.IsNullOrWhiteSpace(dto.BudgetName)) throw new ValidationException("Budget name is required.");
        var accountId = dto.GLAccountId ?? Guid.Empty;
        if (accountId == Guid.Empty) throw new ValidationException("Account is required.");
        if (dto.BudgetAmount is null or <= 0) throw new ValidationException("Budget amount must be positive.");

        var (start, end) = ResolvePeriod(dto);
        var id = Guid.NewGuid();
        var entity = new Budget
        {
            Id = id,
            BudgetId = id,
            TenantId = tenantId,
            BudgetName = dto.BudgetName.Trim(),
            AccountId = accountId,
            StartDate = start,
            EndDate = end,
            BudgetPeriodStart = start,
            BudgetPeriodEnd = end,
            BudgetAmount = dto.BudgetAmount.Value,
            ActualAmount = 0,
            Status = "Draft",
            CurrencyCode = "PKR",
            CreatedBy = userId.ToString()
        };
        _db.Budgets.Add(entity);
        await _db.SaveChangesAsync();
        return (await GetAsync(id, tenantId))!;
    }

    public async Task<BudgetDto> UpdateAsync(UpdateBudgetDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        var entity = await _db.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && (b.Id == dto.Id || b.BudgetId == dto.Id))
            ?? throw new NotFoundException("Budget not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Only draft budgets can be edited.");

        if (!string.IsNullOrWhiteSpace(dto.BudgetName)) entity.BudgetName = dto.BudgetName.Trim();
        if (dto.GLAccountId.HasValue && dto.GLAccountId != Guid.Empty) entity.AccountId = dto.GLAccountId.Value;
        if (dto.BudgetAmount.HasValue) entity.BudgetAmount = dto.BudgetAmount.Value;
        if (dto.FiscalYear.HasValue || dto.Quarter.HasValue || dto.Month.HasValue)
        {
            var (start, end) = ResolvePeriod(new CreateBudgetDto
            {
                FiscalYear = dto.FiscalYear ?? entity.StartDate.Year,
                Quarter = dto.Quarter,
                Month = dto.Month,
                BudgetType = dto.BudgetType
            });
            entity.StartDate = start;
            entity.EndDate = end;
            entity.BudgetPeriodStart = start;
            entity.BudgetPeriodEnd = end;
        }
        // Status is lifecycle-only — ignore dto.Status (FIX-5)
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (await GetAsync(entity.BudgetId, tenantId))!;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _db.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && (b.Id == id || b.BudgetId == id));
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        _db.Budgets.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await Load(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)) return false;
        entity.Status = "Active";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await Load(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase)) return false;
        entity.Status = "Closed";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<Budget?> Load(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && (b.Id == id || b.BudgetId == id));
    }

    private static (DateTime Start, DateTime End) ResolvePeriod(CreateBudgetDto dto)
    {
        var year = dto.FiscalYear is > 0 ? dto.FiscalYear.Value : DateTime.UtcNow.Year;
        if (dto.Month is >= 1 and <= 12)
        {
            var start = new DateTime(year, dto.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            return (start, start.AddMonths(1).AddDays(-1));
        }
        if (dto.Quarter is >= 1 and <= 4)
        {
            var startMonth = (dto.Quarter.Value - 1) * 3 + 1;
            var start = new DateTime(year, startMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            return (start, start.AddMonths(3).AddDays(-1));
        }
        var yStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (yStart, new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc));
    }

    private static BudgetDto Map(Budget b) => new()
    {
        Id = b.BudgetId != Guid.Empty ? b.BudgetId : b.Id,
        TenantId = b.TenantId,
        BudgetName = b.BudgetName,
        BudgetType = PeriodLabel(b),
        GLAccountId = b.AccountId,
        GLAccountName = b.Account?.AccountName,
        FiscalYear = b.StartDate.Year,
        Quarter = null,
        Month = null,
        BudgetAmount = b.BudgetAmount,
        ActualAmount = b.ActualAmount,
        Variance = b.ActualAmount - b.BudgetAmount,
        Status = b.Status,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt,
        CreatedBy = b.CreatedBy,
        UpdatedBy = b.UpdatedBy,
        IsActive = b.IsActive
    };

    private static string PeriodLabel(Budget b)
    {
        if (b.StartDate.Month == 1 && b.EndDate.Month == 12) return "Annual";
        if (b.EndDate.Month - b.StartDate.Month == 2) return "Quarterly";
        return "Monthly";
    }
}
