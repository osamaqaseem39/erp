using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IFinanceGapService
{
    Task<BudgetLineDto> AddBudgetLineAsync(CreateBudgetLineDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<BudgetLineDto>> ListBudgetLinesAsync(Guid budgetId, Guid tenantId);
    Task<TaxGroupDto> CreateTaxGroupAsync(CreateTaxGroupDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<TaxGroupDto>> ListTaxGroupsAsync(Guid tenantId);
    Task<TaxGroupDto?> GetTaxGroupAsync(Guid id, Guid tenantId);
    Task<AssetCategoryDto> CreateAssetCategoryAsync(CreateAssetCategoryDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<AssetCategoryDto>> ListAssetCategoriesAsync(Guid tenantId);
    Task<AssetDisposalDto> CreateAssetDisposalAsync(CreateAssetDisposalDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<AssetDisposalDto>> ListAssetDisposalsAsync(Guid tenantId);
    Task<AssetDisposalDto?> GetAssetDisposalAsync(Guid id, Guid tenantId);
    Task<AssetTransferDto> CreateAssetTransferAsync(CreateAssetTransferDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<AssetTransferDto>> ListAssetTransfersAsync(Guid tenantId);
    Task<AssetTransferDto?> GetAssetTransferAsync(Guid id, Guid tenantId);
    Task<FundTransferDto> CreateFundTransferAsync(CreateFundTransferDto dto, Guid tenantId, Guid userId);
    Task<FundTransferDto> PostFundTransferAsync(Guid id, Guid tenantId, Guid userId);
    Task<IReadOnlyList<FundTransferDto>> ListFundTransfersAsync(Guid tenantId);
    Task<FundTransferDto?> GetFundTransferAsync(Guid id, Guid tenantId);
    Task<CreditMemoDto> CreateCreditMemoAsync(CreateCreditMemoDto dto, Guid tenantId, Guid userId);
    Task<CreditMemoDto> PostCreditMemoAsync(Guid id, Guid tenantId, Guid userId);
    Task<IReadOnlyList<CreditMemoDto>> ListCreditMemosAsync(Guid tenantId);
    Task<CreditMemoDto?> GetCreditMemoAsync(Guid id, Guid tenantId);
    Task<DebitMemoDto> CreateDebitMemoAsync(CreateDebitMemoDto dto, Guid tenantId, Guid userId);
    Task<DebitMemoDto> PostDebitMemoAsync(Guid id, Guid tenantId, Guid userId);
    Task<IReadOnlyList<DebitMemoDto>> ListDebitMemosAsync(Guid tenantId);
    Task<DebitMemoDto?> GetDebitMemoAsync(Guid id, Guid tenantId);
    Task<CustomerAdvanceDto> CreateCustomerAdvanceAsync(CreateCustomerAdvanceDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<CustomerAdvanceDto>> ListCustomerAdvancesAsync(Guid tenantId);
    Task<CustomerAdvanceDto?> GetCustomerAdvanceAsync(Guid id, Guid tenantId);
    Task<VendorAdvanceDto> CreateVendorAdvanceAsync(CreateVendorAdvanceDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<VendorAdvanceDto>> ListVendorAdvancesAsync(Guid tenantId);
    Task<VendorAdvanceDto?> GetVendorAdvanceAsync(Guid id, Guid tenantId);
    Task<Guid> ApplyAdvanceAsync(CreateAdvanceApplicationDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<AdvanceApplicationDto>> ListAdvanceApplicationsAsync(Guid tenantId);
    Task<WithholdingTaxLineDto> AddWithholdingAsync(CreateWithholdingTaxLineDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<WithholdingTaxLineDto>> ListWithholdingTaxLinesAsync(Guid tenantId);
    Task<RecurringJournalDto> CreateRecurringJournalAsync(CreateRecurringJournalDto dto, Guid tenantId, Guid userId);
    Task<Guid> GenerateRecurringJournalAsync(Guid id, Guid tenantId, Guid userId);
    Task<IReadOnlyList<RecurringJournalDto>> ListRecurringJournalsAsync(Guid tenantId);
    Task<RecurringJournalDto?> GetRecurringJournalAsync(Guid id, Guid tenantId);
    Task<FxRevaluationRunDto> CreateFxRunAsync(CreateFxRevaluationRunDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<FxRevaluationRunDto>> ListFxRunsAsync(Guid tenantId);
    Task<FxRevaluationRunDto?> GetFxRunAsync(Guid id, Guid tenantId);
    Task<IntercompanySettlementDto> CreateIcSettlementAsync(CreateIntercompanySettlementDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<IntercompanySettlementDto>> ListIcSettlementsAsync(Guid tenantId);
    Task<IntercompanySettlementDto?> GetIcSettlementAsync(Guid id, Guid tenantId);
    Task<IntercompanyTransactionDto> CreateIcTransactionAsync(CreateIntercompanyTransactionDto dto, Guid tenantId, Guid userId);
    Task<IReadOnlyList<IntercompanyTransactionDto>> ListIcTransactionsAsync(Guid tenantId, bool unsettledOnly = false);
    Task<IntercompanyTransactionDto?> GetIcTransactionAsync(Guid id, Guid tenantId);
}

public class FinanceGapService : IFinanceGapService
{
    private readonly ERPDbContext _db;

    public FinanceGapService(ERPDbContext db) => _db = db;

    public async Task<BudgetLineDto> AddBudgetLineAsync(CreateBudgetLineDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var budget = await _db.Budgets.FirstOrDefaultAsync(b =>
                (b.Id == dto.BudgetId || b.BudgetId == dto.BudgetId) && b.TenantId == tenantId)
            ?? throw new NotFoundException("Budget not found.");
        var line = new BudgetLine
        {
            TenantId = tenantId,
            BudgetId = budget.Id != Guid.Empty ? budget.Id : budget.BudgetId,
            PeriodStart = dto.PeriodStart.Date,
            PeriodEnd = dto.PeriodEnd.Date,
            Amount = dto.Amount,
            CreatedBy = userId.ToString()
        };
        _db.BudgetLines.Add(line);
        budget.BudgetAmount = await _db.BudgetLines.Where(l => l.BudgetId == line.BudgetId).SumAsync(l => l.Amount) + dto.Amount;
        await _db.SaveChangesAsync();
        return MapBudgetLine(line);
    }

    public async Task<IReadOnlyList<BudgetLineDto>> ListBudgetLinesAsync(Guid budgetId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<BudgetLineDto>();
        return (await _db.BudgetLines.AsNoTracking()
            .Where(l => l.BudgetId == budgetId && l.TenantId == tenantId && l.IsActive)
            .OrderBy(l => l.PeriodStart)
            .ToListAsync()).Select(MapBudgetLine).ToList();
    }

    public async Task<TaxGroupDto> CreateTaxGroupAsync(CreateTaxGroupDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Code and name are required.");
        var entity = new TaxGroup
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            TaxType = string.IsNullOrWhiteSpace(dto.TaxType) ? "Sales" : dto.TaxType.Trim(),
            CreatedBy = userId.ToString()
        };
        var seq = 1;
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new TaxGroupLine
            {
                TenantId = tenantId,
                TaxCodeId = line.TaxCodeId,
                Sequence = line.Sequence > 0 ? line.Sequence : seq++,
                IsCompound = line.IsCompound,
                CreatedBy = userId.ToString()
            });
        }
        _db.TaxGroups.Add(entity);
        await _db.SaveChangesAsync();
        return MapTaxGroup(entity);
    }

    public async Task<IReadOnlyList<TaxGroupDto>> ListTaxGroupsAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<TaxGroupDto>();
        return (await _db.TaxGroups.AsNoTracking().Include(g => g.Lines)
            .Where(g => g.TenantId == tenantId && g.IsActive)
            .OrderBy(g => g.Code)
            .ToListAsync()).Select(MapTaxGroup).ToList();
    }

    public async Task<TaxGroupDto?> GetTaxGroupAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.TaxGroups.AsNoTracking().Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id && g.TenantId == tenantId);
        return e == null ? null : MapTaxGroup(e);
    }

    public async Task<AssetCategoryDto> CreateAssetCategoryAsync(CreateAssetCategoryDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new AssetCategory
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            UsefulLifeMonths = dto.UsefulLifeMonths <= 0 ? 60 : dto.UsefulLifeMonths,
            DepreciationMethod = string.IsNullOrWhiteSpace(dto.DepreciationMethod) ? "StraightLine" : dto.DepreciationMethod,
            AssetAccountId = dto.AssetAccountId,
            AccumDepAccountId = dto.AccumDepAccountId,
            DepExpenseAccountId = dto.DepExpenseAccountId,
            CreatedBy = userId.ToString()
        };
        _db.AssetCategories.Add(entity);
        await _db.SaveChangesAsync();
        return MapAssetCategory(entity);
    }

    public async Task<IReadOnlyList<AssetCategoryDto>> ListAssetCategoriesAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<AssetCategoryDto>();
        return (await _db.AssetCategories.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync()).Select(MapAssetCategory).ToList();
    }

    public async Task<AssetDisposalDto> CreateAssetDisposalAsync(CreateAssetDisposalDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var asset = await _db.FixedAssets.FirstOrDefaultAsync(a =>
            (a.AssetId == dto.AssetId || a.Id == dto.AssetId) && a.TenantId == tenantId)
            ?? throw new NotFoundException("Fixed asset not found.");
        var book = asset.CurrentValue;
        var disposal = new AssetDisposal
        {
            TenantId = tenantId,
            AssetId = asset.AssetId,
            DisposalDate = dto.DisposalDate.Date,
            Proceeds = dto.Proceeds,
            BookValue = book,
            GainLoss = dto.Proceeds - book,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        _db.AssetDisposals.Add(disposal);
        asset.Status = "Disposed";
        asset.DisposalDate = disposal.DisposalDate;
        asset.UpdatedAt = DateTime.UtcNow;
        asset.UpdatedBy = userId.ToString();
        await _db.SaveChangesAsync();
        return MapDisposal(disposal);
    }

    public async Task<IReadOnlyList<AssetDisposalDto>> ListAssetDisposalsAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.AssetDisposals.AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.DisposalDate)
            .ToListAsync()).Select(MapDisposal).ToList();
    }

    public async Task<AssetDisposalDto?> GetAssetDisposalAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.AssetDisposals.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);
        return row == null ? null : MapDisposal(row);
    }

    public async Task<AssetTransferDto> CreateAssetTransferAsync(CreateAssetTransferDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var asset = await _db.FixedAssets.FirstOrDefaultAsync(a =>
            (a.AssetId == dto.AssetId || a.Id == dto.AssetId) && a.TenantId == tenantId)
            ?? throw new NotFoundException("Fixed asset not found.");
        var xfer = new AssetTransfer
        {
            TenantId = tenantId,
            AssetId = asset.AssetId,
            TransferDate = dto.TransferDate.Date,
            FromCostCenterId = dto.FromCostCenterId,
            ToCostCenterId = dto.ToCostCenterId,
            FromBranchId = dto.FromBranchId,
            ToBranchId = dto.ToBranchId,
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        _db.AssetTransfers.Add(xfer);
        await _db.SaveChangesAsync();
        return MapTransfer(xfer);
    }

    public async Task<IReadOnlyList<AssetTransferDto>> ListAssetTransfersAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.AssetTransfers.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync()).Select(MapTransfer).ToList();
    }

    public async Task<AssetTransferDto?> GetAssetTransferAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.AssetTransfers.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        return row == null ? null : MapTransfer(row);
    }

    public async Task<FundTransferDto> CreateFundTransferAsync(CreateFundTransferDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (dto.Amount <= 0) throw new ValidationException("Amount must be positive.");
        var entity = new FundTransfer
        {
            TenantId = tenantId,
            TransferNumber = string.IsNullOrWhiteSpace(dto.TransferNumber)
                ? $"FT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
                : dto.TransferNumber.Trim(),
            TransferDate = dto.TransferDate == default ? DateTime.UtcNow.Date : dto.TransferDate.Date,
            FromBankAccountId = dto.FromBankAccountId,
            ToBankAccountId = dto.ToBankAccountId,
            FromAccountId = dto.FromAccountId,
            ToAccountId = dto.ToAccountId,
            Amount = dto.Amount,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        _db.FundTransfers.Add(entity);
        await _db.SaveChangesAsync();
        return MapFundTransfer(entity);
    }

    public async Task<IReadOnlyList<FundTransferDto>> ListFundTransfersAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.FundTransfers.AsNoTracking()
            .Where(f => f.TenantId == tenantId)
            .OrderByDescending(f => f.TransferDate)
            .ToListAsync()).Select(MapFundTransfer).ToList();
    }

    public async Task<FundTransferDto?> GetFundTransferAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.FundTransfers.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId);
        return row == null ? null : MapFundTransfer(row);
    }

    public async Task<FundTransferDto> PostFundTransferAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _db.FundTransfers.FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId)
            ?? throw new NotFoundException("Fund transfer not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft fund transfers can be posted.");
        entity.Status = "Posted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapFundTransfer(entity);
    }

    public async Task<CreditMemoDto> CreateCreditMemoAsync(CreateCreditMemoDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new CreditMemo
        {
            TenantId = tenantId,
            MemoNumber = string.IsNullOrWhiteSpace(dto.MemoNumber)
                ? $"CM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpperInvariant()
                : dto.MemoNumber.Trim(),
            CustomerId = dto.CustomerId,
            MemoDate = dto.MemoDate == default ? DateTime.UtcNow.Date : dto.MemoDate.Date,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        var n = 1;
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new CreditMemoLine
            {
                TenantId = tenantId,
                LineNumber = n++,
                Description = line.Description,
                Amount = line.Amount,
                TaxAmount = line.TaxAmount,
                AccountId = line.AccountId,
                CreatedBy = userId.ToString()
            });
        }
        entity.SubTotal = entity.Lines.Sum(l => l.Amount);
        entity.TaxTotal = entity.Lines.Sum(l => l.TaxAmount);
        entity.TotalAmount = entity.SubTotal + entity.TaxTotal;
        entity.Balance = entity.TotalAmount;
        _db.CreditMemos.Add(entity);
        await _db.SaveChangesAsync();
        return MapCreditMemo(entity);
    }

    public async Task<IReadOnlyList<CreditMemoDto>> ListCreditMemosAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.CreditMemos.AsNoTracking().Include(e => e.Lines)
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.MemoDate)
            .ToListAsync()).Select(MapCreditMemo).ToList();
    }

    public async Task<CreditMemoDto?> GetCreditMemoAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.CreditMemos.AsNoTracking().Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        return row == null ? null : MapCreditMemo(row);
    }

    public async Task<CreditMemoDto> PostCreditMemoAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _db.CreditMemos.Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId)
            ?? throw new NotFoundException("Credit memo not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft credit memos can be posted.");
        entity.Status = "Posted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapCreditMemo(entity);
    }

    public async Task<DebitMemoDto> CreateDebitMemoAsync(CreateDebitMemoDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new DebitMemo
        {
            TenantId = tenantId,
            MemoNumber = string.IsNullOrWhiteSpace(dto.MemoNumber)
                ? $"DM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpperInvariant()
                : dto.MemoNumber.Trim(),
            VendorId = dto.VendorId,
            MemoDate = dto.MemoDate == default ? DateTime.UtcNow.Date : dto.MemoDate.Date,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        var n = 1;
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new DebitMemoLine
            {
                TenantId = tenantId,
                LineNumber = n++,
                Description = line.Description,
                Amount = line.Amount,
                TaxAmount = line.TaxAmount,
                AccountId = line.AccountId,
                CreatedBy = userId.ToString()
            });
        }
        entity.SubTotal = entity.Lines.Sum(l => l.Amount);
        entity.TaxTotal = entity.Lines.Sum(l => l.TaxAmount);
        entity.TotalAmount = entity.SubTotal + entity.TaxTotal;
        entity.Balance = entity.TotalAmount;
        _db.DebitMemos.Add(entity);
        await _db.SaveChangesAsync();
        return MapDebitMemo(entity);
    }

    public async Task<IReadOnlyList<DebitMemoDto>> ListDebitMemosAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.DebitMemos.AsNoTracking().Include(e => e.Lines)
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.MemoDate)
            .ToListAsync()).Select(MapDebitMemo).ToList();
    }

    public async Task<DebitMemoDto?> GetDebitMemoAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.DebitMemos.AsNoTracking().Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        return row == null ? null : MapDebitMemo(row);
    }

    public async Task<DebitMemoDto> PostDebitMemoAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _db.DebitMemos.Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId)
            ?? throw new NotFoundException("Debit memo not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft debit memos can be posted.");
        entity.Status = "Posted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapDebitMemo(entity);
    }

    public async Task<CustomerAdvanceDto> CreateCustomerAdvanceAsync(CreateCustomerAdvanceDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new CustomerAdvance
        {
            TenantId = tenantId,
            AdvanceNumber = string.IsNullOrWhiteSpace(dto.AdvanceNumber)
                ? $"CA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpperInvariant()
                : dto.AdvanceNumber.Trim(),
            CustomerId = dto.CustomerId,
            AdvanceDate = dto.AdvanceDate == default ? DateTime.UtcNow.Date : dto.AdvanceDate.Date,
            Amount = dto.Amount,
            Balance = dto.Amount,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode,
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        _db.CustomerAdvances.Add(entity);
        await _db.SaveChangesAsync();
        return MapCustomerAdvance(entity);
    }

    public async Task<IReadOnlyList<CustomerAdvanceDto>> ListCustomerAdvancesAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.CustomerAdvances.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.AdvanceDate)
            .ToListAsync()).Select(MapCustomerAdvance).ToList();
    }

    public async Task<CustomerAdvanceDto?> GetCustomerAdvanceAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.CustomerAdvances.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        return row == null ? null : MapCustomerAdvance(row);
    }

    public async Task<VendorAdvanceDto> CreateVendorAdvanceAsync(CreateVendorAdvanceDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new VendorAdvance
        {
            TenantId = tenantId,
            AdvanceNumber = string.IsNullOrWhiteSpace(dto.AdvanceNumber)
                ? $"VA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpperInvariant()
                : dto.AdvanceNumber.Trim(),
            VendorId = dto.VendorId,
            AdvanceDate = dto.AdvanceDate == default ? DateTime.UtcNow.Date : dto.AdvanceDate.Date,
            Amount = dto.Amount,
            Balance = dto.Amount,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode,
            Status = "Open",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        _db.VendorAdvances.Add(entity);
        await _db.SaveChangesAsync();
        return MapVendorAdvance(entity);
    }

    public async Task<IReadOnlyList<VendorAdvanceDto>> ListVendorAdvancesAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.VendorAdvances.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.AdvanceDate)
            .ToListAsync()).Select(MapVendorAdvance).ToList();
    }

    public async Task<VendorAdvanceDto?> GetVendorAdvanceAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.VendorAdvances.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        return row == null ? null : MapVendorAdvance(row);
    }

    public async Task<Guid> ApplyAdvanceAsync(CreateAdvanceApplicationDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (dto.AppliedAmount <= 0) throw new ValidationException("Applied amount must be positive.");
        var hasCust = dto.CustomerAdvanceId.HasValue && dto.CustomerAdvanceId != Guid.Empty;
        var hasVend = dto.VendorAdvanceId.HasValue && dto.VendorAdvanceId != Guid.Empty;
        if (hasCust == hasVend)
            throw new ValidationException("Exactly one of CustomerAdvanceId or VendorAdvanceId is required.");
        var hasInv = dto.InvoiceId.HasValue && dto.InvoiceId != Guid.Empty;
        var hasBill = dto.BillId.HasValue && dto.BillId != Guid.Empty;
        if (hasInv == hasBill)
            throw new ValidationException("Exactly one of InvoiceId or BillId is required.");

        if (hasCust)
        {
            var adv = await _db.CustomerAdvances.FirstOrDefaultAsync(a => a.Id == dto.CustomerAdvanceId && a.TenantId == tenantId)
                ?? throw new NotFoundException("Customer advance not found.");
            if (dto.AppliedAmount > adv.Balance) throw new ValidationException("Applied amount exceeds advance balance.");
            adv.Balance -= dto.AppliedAmount;
            if (adv.Balance <= 0) adv.Status = "FullyApplied";
        }
        else
        {
            var adv = await _db.VendorAdvances.FirstOrDefaultAsync(a => a.Id == dto.VendorAdvanceId && a.TenantId == tenantId)
                ?? throw new NotFoundException("Vendor advance not found.");
            if (dto.AppliedAmount > adv.Balance) throw new ValidationException("Applied amount exceeds advance balance.");
            adv.Balance -= dto.AppliedAmount;
            if (adv.Balance <= 0) adv.Status = "FullyApplied";
        }

        if (hasInv)
        {
            var inv = await _db.Invoices.FirstOrDefaultAsync(i =>
                (i.Id == dto.InvoiceId || i.InvoiceId == dto.InvoiceId) && i.TenantId == tenantId)
                ?? throw new NotFoundException("Invoice not found.");
            var apply = Math.Min(dto.AppliedAmount, inv.Balance);
            inv.Balance -= apply;
            if (inv.Balance <= 0) inv.Status = "Paid";
            else if (inv.Balance < inv.TotalAmount) inv.Status = "PartiallyPaid";
        }
        else
        {
            var bill = await _db.Bills.FirstOrDefaultAsync(b =>
                (b.Id == dto.BillId || b.BillId == dto.BillId) && b.TenantId == tenantId)
                ?? throw new NotFoundException("Bill not found.");
            var apply = Math.Min(dto.AppliedAmount, bill.Balance);
            bill.Balance -= apply;
            if (bill.Balance <= 0) bill.Status = "Paid";
            else if (bill.Balance < bill.TotalAmount) bill.Status = "PartiallyPaid";
        }

        var app = new AdvanceApplication
        {
            TenantId = tenantId,
            CustomerAdvanceId = dto.CustomerAdvanceId,
            VendorAdvanceId = dto.VendorAdvanceId,
            InvoiceId = dto.InvoiceId,
            BillId = dto.BillId,
            AppliedAmount = dto.AppliedAmount,
            AppliedDate = DateTime.UtcNow.Date,
            Status = "Applied",
            CreatedBy = userId.ToString()
        };
        _db.AdvanceApplications.Add(app);
        await _db.SaveChangesAsync();
        return app.Id;
    }

    public async Task<IReadOnlyList<AdvanceApplicationDto>> ListAdvanceApplicationsAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.AdvanceApplications.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync()).Select(a => new AdvanceApplicationDto
        {
            Id = a.Id,
            CustomerAdvanceId = a.CustomerAdvanceId,
            VendorAdvanceId = a.VendorAdvanceId,
            InvoiceId = a.InvoiceId,
            BillId = a.BillId,
            AppliedAmount = a.AppliedAmount,
            AppliedDate = a.AppliedDate,
            Status = a.Status
        }).ToList();
    }

    public async Task<WithholdingTaxLineDto> AddWithholdingAsync(CreateWithholdingTaxLineDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        _ = await _db.Payments.FirstOrDefaultAsync(p => p.Id == dto.PaymentId && p.TenantId == tenantId)
            ?? throw new NotFoundException("Payment not found.");
        var line = new WithholdingTaxLine
        {
            TenantId = tenantId,
            PaymentId = dto.PaymentId,
            TaxCodeId = dto.TaxCodeId,
            BaseAmount = dto.BaseAmount,
            WithholdingAmount = dto.WithholdingAmount,
            PayableAccountId = dto.PayableAccountId,
            CreatedBy = userId.ToString()
        };
        _db.WithholdingTaxLines.Add(line);
        await _db.SaveChangesAsync();
        return new WithholdingTaxLineDto
        {
            Id = line.Id,
            PaymentId = line.PaymentId,
            TaxCodeId = line.TaxCodeId,
            BaseAmount = line.BaseAmount,
            WithholdingAmount = line.WithholdingAmount,
            PayableAccountId = line.PayableAccountId
        };
    }

    public async Task<IReadOnlyList<WithholdingTaxLineDto>> ListWithholdingTaxLinesAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.WithholdingTaxLines.AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync()).Select(MapWithholding).ToList();
    }

    public async Task<RecurringJournalDto> CreateRecurringJournalAsync(CreateRecurringJournalDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new RecurringJournal
        {
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            JournalType = string.IsNullOrWhiteSpace(dto.JournalType) ? "Journal" : dto.JournalType,
            Frequency = string.IsNullOrWhiteSpace(dto.Frequency) ? "Monthly" : dto.Frequency,
            NextRunDate = dto.NextRunDate.Date,
            EndDate = dto.EndDate?.Date,
            CreatedBy = userId.ToString()
        };
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new RecurringJournalLine
            {
                TenantId = tenantId,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId,
                Debit = line.Debit,
                Credit = line.Credit,
                Narration = line.Narration,
                CreatedBy = userId.ToString()
            });
        }
        _db.RecurringJournals.Add(entity);
        await _db.SaveChangesAsync();
        return MapRecurring(entity);
    }

    public async Task<IReadOnlyList<RecurringJournalDto>> ListRecurringJournalsAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.RecurringJournals.AsNoTracking().Include(e => e.Lines)
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.Name)
            .ToListAsync()).Select(MapRecurring).ToList();
    }

    public async Task<RecurringJournalDto?> GetRecurringJournalAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.RecurringJournals.AsNoTracking().Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        return row == null ? null : MapRecurring(row);
    }

    public async Task<Guid> GenerateRecurringJournalAsync(Guid id, Guid tenantId, Guid userId)
    {
        var template = await _db.RecurringJournals.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId && r.IsActive)
            ?? throw new NotFoundException("Recurring journal not found.");
        if (template.EndDate.HasValue && template.NextRunDate > template.EndDate.Value)
            throw new InvalidOperationException("Recurring journal has ended.");

        var jId = Guid.NewGuid();
        var journal = new Journal
        {
            Id = jId,
            JournalId = jId,
            TenantId = tenantId,
            JournalType = template.JournalType,
            JournalDate = template.NextRunDate,
            Description = $"Recurring: {template.Name}",
            Status = "Draft",
            JournalNumber = $"JV-R-{DateTime.UtcNow:yyyyMMdd}-{jId:N}"[..22].ToUpperInvariant(),
            TotalDebit = template.Lines.Sum(l => l.Debit),
            TotalCredit = template.Lines.Sum(l => l.Credit),
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Journals.Add(journal);
        foreach (var line in template.Lines)
        {
            _db.JournalEntries.Add(new JournalEntry
            {
                Id = Guid.NewGuid(),
                EntryId = Guid.NewGuid(),
                JournalId = jId,
                TenantId = tenantId,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId,
                Debit = line.Debit,
                Credit = line.Credit,
                Narration = line.Narration ?? template.Name,
                EntryDate = template.NextRunDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }

        template.NextRunDate = template.Frequency.Equals("Weekly", StringComparison.OrdinalIgnoreCase)
            ? template.NextRunDate.AddDays(7)
            : template.Frequency.Equals("Yearly", StringComparison.OrdinalIgnoreCase)
                ? template.NextRunDate.AddYears(1)
                : template.NextRunDate.AddMonths(1);
        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedBy = userId.ToString();
        await _db.SaveChangesAsync();
        return jId;
    }

    public async Task<IReadOnlyList<FxRevaluationRunDto>> ListFxRunsAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.FxRevaluationRuns.AsNoTracking().Include(e => e.Lines)
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.RunDate)
            .ToListAsync()).Select(MapFxRun).ToList();
    }

    public async Task<FxRevaluationRunDto?> GetFxRunAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.FxRevaluationRuns.AsNoTracking().Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        return row == null ? null : MapFxRun(row);
    }

    public async Task<FxRevaluationRunDto> CreateFxRunAsync(CreateFxRevaluationRunDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var run = new FxRevaluationRun
        {
            TenantId = tenantId,
            RunDate = dto.RunDate == default ? DateTime.UtcNow.Date : dto.RunDate.Date,
            RateSource = dto.RateSource,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        foreach (var line in dto.Lines)
        {
            run.Lines.Add(new FxRevaluationLine
            {
                TenantId = tenantId,
                AccountId = line.AccountId,
                PartyType = line.PartyType,
                PartyId = line.PartyId,
                CurrencyCode = line.CurrencyCode,
                ForeignBalance = line.ForeignBalance,
                BookBalance = line.BookBalance,
                RevaluedBalance = line.RevaluedBalance,
                UnrealizedGainLoss = line.UnrealizedGainLoss,
                CreatedBy = userId.ToString()
            });
        }
        _db.FxRevaluationRuns.Add(run);
        await _db.SaveChangesAsync();
        return MapFxRun(run);
    }

    public async Task<IReadOnlyList<IntercompanySettlementDto>> ListIcSettlementsAsync(Guid tenantId)
    {
        RequireTenant(tenantId);
        return (await _db.IntercompanySettlements.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.SettlementDate)
            .ToListAsync()).Select(e => MapIcSettlement(e, null)).ToList();
    }

    public async Task<IntercompanySettlementDto?> GetIcSettlementAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.IntercompanySettlements.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);
        if (row == null) return null;
        var txIds = await _db.IntercompanyTransactions.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.SettlementId == id)
            .Select(t => t.IntercompanyId)
            .ToListAsync();
        return MapIcSettlement(row, txIds);
    }

    public async Task<IntercompanySettlementDto> CreateIcSettlementAsync(CreateIntercompanySettlementDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        var entity = new IntercompanySettlement
        {
            TenantId = tenantId,
            SettlementNumber = string.IsNullOrWhiteSpace(dto.SettlementNumber)
                ? $"ICS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..18].ToUpperInvariant()
                : dto.SettlementNumber.Trim(),
            SettlementDate = dto.SettlementDate == default ? DateTime.UtcNow.Date : dto.SettlementDate.Date,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        _db.IntercompanySettlements.Add(entity);
        await _db.SaveChangesAsync();

        if (dto.TransactionIds.Count > 0)
        {
            var txs = await _db.IntercompanyTransactions
                .Where(t => t.TenantId == tenantId && dto.TransactionIds.Contains(t.IntercompanyId))
                .ToListAsync();
            foreach (var tx in txs)
                tx.SettlementId = entity.Id;
            await _db.SaveChangesAsync();
        }

        return new IntercompanySettlementDto
        {
            Id = entity.Id,
            SettlementNumber = entity.SettlementNumber,
            SettlementDate = entity.SettlementDate,
            Status = entity.Status,
            Notes = entity.Notes,
            TransactionIds = dto.TransactionIds
        };
    }

    public async Task<IntercompanyTransactionDto> CreateIcTransactionAsync(
        CreateIntercompanyTransactionDto dto, Guid tenantId, Guid userId)
    {
        RequireTenant(tenantId);
        if (dto.Amount <= 0) throw new ValidationException("Amount must be positive.");
        if (dto.FromCompanyId == Guid.Empty || dto.ToCompanyId == Guid.Empty)
            throw new ValidationException("From and to company are required.");
        if (dto.FromCompanyId == dto.ToCompanyId)
            throw new ValidationException("From and to company must differ.");

        var entity = new IntercompanyTransaction
        {
            IntercompanyId = Guid.NewGuid(),
            TenantId = tenantId,
            FromCompanyId = dto.FromCompanyId,
            ToCompanyId = dto.ToCompanyId,
            Amount = dto.Amount,
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode.Trim(),
            Description = dto.Description,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId
        };
        _db.IntercompanyTransactions.Add(entity);
        await _db.SaveChangesAsync();
        return MapIcTransaction(entity);
    }

    public async Task<IReadOnlyList<IntercompanyTransactionDto>> ListIcTransactionsAsync(
        Guid tenantId, bool unsettledOnly = false)
    {
        RequireTenant(tenantId);
        var q = _db.IntercompanyTransactions.AsNoTracking().Where(t => t.TenantId == tenantId);
        if (unsettledOnly) q = q.Where(t => t.SettlementId == null);
        return (await q.OrderByDescending(t => t.CreatedAt).ToListAsync()).Select(MapIcTransaction).ToList();
    }

    public async Task<IntercompanyTransactionDto?> GetIcTransactionAsync(Guid id, Guid tenantId)
    {
        RequireTenant(tenantId);
        var row = await _db.IntercompanyTransactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.IntercompanyId == id && t.TenantId == tenantId);
        return row == null ? null : MapIcTransaction(row);
    }

    private static void RequireTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
    }

    private static BudgetLineDto MapBudgetLine(BudgetLine l) => new()
    {
        Id = l.Id,
        BudgetId = l.BudgetId,
        PeriodStart = l.PeriodStart,
        PeriodEnd = l.PeriodEnd,
        Amount = l.Amount
    };

    private static TaxGroupDto MapTaxGroup(TaxGroup g) => new()
    {
        Id = g.Id,
        Code = g.Code,
        Name = g.Name,
        TaxType = g.TaxType,
        Lines = g.Lines.OrderBy(l => l.Sequence).Select(l => new TaxGroupLineDto
        {
            Id = l.Id,
            TaxCodeId = l.TaxCodeId,
            Sequence = l.Sequence,
            IsCompound = l.IsCompound
        }).ToList()
    };

    private static AssetCategoryDto MapAssetCategory(AssetCategory c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Name = c.Name,
        UsefulLifeMonths = c.UsefulLifeMonths,
        DepreciationMethod = c.DepreciationMethod,
        AssetAccountId = c.AssetAccountId,
        AccumDepAccountId = c.AccumDepAccountId,
        DepExpenseAccountId = c.DepExpenseAccountId
    };

    private static AssetDisposalDto MapDisposal(AssetDisposal d) => new()
    {
        Id = d.Id,
        AssetId = d.AssetId,
        DisposalDate = d.DisposalDate,
        Proceeds = d.Proceeds,
        BookValue = d.BookValue,
        GainLoss = d.GainLoss,
        Status = d.Status,
        PostedJournalId = d.PostedJournalId,
        Notes = d.Notes
    };

    private static AssetTransferDto MapTransfer(AssetTransfer t) => new()
    {
        Id = t.Id,
        AssetId = t.AssetId,
        TransferDate = t.TransferDate,
        FromCostCenterId = t.FromCostCenterId,
        ToCostCenterId = t.ToCostCenterId,
        FromBranchId = t.FromBranchId,
        ToBranchId = t.ToBranchId,
        Notes = t.Notes
    };

    private static FundTransferDto MapFundTransfer(FundTransfer f) => new()
    {
        Id = f.Id,
        TransferNumber = f.TransferNumber,
        TransferDate = f.TransferDate,
        FromBankAccountId = f.FromBankAccountId,
        ToBankAccountId = f.ToBankAccountId,
        FromAccountId = f.FromAccountId,
        ToAccountId = f.ToAccountId,
        Amount = f.Amount,
        CurrencyCode = f.CurrencyCode,
        Status = f.Status,
        PostedJournalId = f.PostedJournalId,
        Notes = f.Notes
    };

    private static CreditMemoDto MapCreditMemo(CreditMemo e) => new()
    {
        Id = e.Id,
        MemoNumber = e.MemoNumber,
        CustomerId = e.CustomerId,
        MemoDate = e.MemoDate,
        CurrencyCode = e.CurrencyCode,
        SubTotal = e.SubTotal,
        TaxTotal = e.TaxTotal,
        TotalAmount = e.TotalAmount,
        Balance = e.Balance,
        Status = e.Status,
        PostedJournalId = e.PostedJournalId,
        Notes = e.Notes,
        Lines = e.Lines.OrderBy(l => l.LineNumber).Select(l => new MemoLineDto
        {
            Id = l.Id,
            LineNumber = l.LineNumber,
            Description = l.Description,
            Amount = l.Amount,
            TaxAmount = l.TaxAmount,
            AccountId = l.AccountId
        }).ToList()
    };

    private static DebitMemoDto MapDebitMemo(DebitMemo e) => new()
    {
        Id = e.Id,
        MemoNumber = e.MemoNumber,
        VendorId = e.VendorId,
        MemoDate = e.MemoDate,
        CurrencyCode = e.CurrencyCode,
        SubTotal = e.SubTotal,
        TaxTotal = e.TaxTotal,
        TotalAmount = e.TotalAmount,
        Balance = e.Balance,
        Status = e.Status,
        PostedJournalId = e.PostedJournalId,
        Notes = e.Notes,
        Lines = e.Lines.OrderBy(l => l.LineNumber).Select(l => new MemoLineDto
        {
            Id = l.Id,
            LineNumber = l.LineNumber,
            Description = l.Description,
            Amount = l.Amount,
            TaxAmount = l.TaxAmount,
            AccountId = l.AccountId
        }).ToList()
    };

    private static CustomerAdvanceDto MapCustomerAdvance(CustomerAdvance e) => new()
    {
        Id = e.Id,
        AdvanceNumber = e.AdvanceNumber,
        CustomerId = e.CustomerId,
        AdvanceDate = e.AdvanceDate,
        Amount = e.Amount,
        Balance = e.Balance,
        CurrencyCode = e.CurrencyCode,
        Status = e.Status,
        Notes = e.Notes
    };

    private static VendorAdvanceDto MapVendorAdvance(VendorAdvance e) => new()
    {
        Id = e.Id,
        AdvanceNumber = e.AdvanceNumber,
        VendorId = e.VendorId,
        AdvanceDate = e.AdvanceDate,
        Amount = e.Amount,
        Balance = e.Balance,
        CurrencyCode = e.CurrencyCode,
        Status = e.Status,
        Notes = e.Notes
    };

    private static RecurringJournalDto MapRecurring(RecurringJournal e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        JournalType = e.JournalType,
        Frequency = e.Frequency,
        NextRunDate = e.NextRunDate,
        EndDate = e.EndDate,
        IsActive = e.IsActive,
        Lines = e.Lines.Select(l => new RecurringJournalLineDto
        {
            Id = l.Id,
            AccountId = l.AccountId,
            CostCenterId = l.CostCenterId,
            Debit = l.Debit,
            Credit = l.Credit,
            Narration = l.Narration
        }).ToList()
    };

    private static FxRevaluationRunDto MapFxRun(FxRevaluationRun e) => new()
    {
        Id = e.Id,
        RunDate = e.RunDate,
        RateSource = e.RateSource,
        Status = e.Status,
        PostedJournalId = e.PostedJournalId,
        Notes = e.Notes,
        Lines = e.Lines.Select(l => new FxRevaluationLineDto
        {
            Id = l.Id,
            AccountId = l.AccountId,
            PartyType = l.PartyType,
            PartyId = l.PartyId,
            CurrencyCode = l.CurrencyCode,
            ForeignBalance = l.ForeignBalance,
            BookBalance = l.BookBalance,
            RevaluedBalance = l.RevaluedBalance,
            UnrealizedGainLoss = l.UnrealizedGainLoss
        }).ToList()
    };

    private static IntercompanySettlementDto MapIcSettlement(IntercompanySettlement e, IEnumerable<Guid>? transactionIds = null) => new()
    {
        Id = e.Id,
        SettlementNumber = e.SettlementNumber,
        SettlementDate = e.SettlementDate,
        Status = e.Status,
        PostedJournalId = e.PostedJournalId,
        Notes = e.Notes,
        TransactionIds = transactionIds?.ToList() ?? new List<Guid>()
    };

    private static WithholdingTaxLineDto MapWithholding(WithholdingTaxLine w) => new()
    {
        Id = w.Id,
        PaymentId = w.PaymentId,
        TaxCodeId = w.TaxCodeId,
        BaseAmount = w.BaseAmount,
        WithholdingAmount = w.WithholdingAmount,
        PayableAccountId = w.PayableAccountId
    };

    private static IntercompanyTransactionDto MapIcTransaction(IntercompanyTransaction t) => new()
    {
        Id = t.IntercompanyId,
        FromCompanyId = t.FromCompanyId,
        ToCompanyId = t.ToCompanyId,
        Amount = t.Amount,
        CurrencyCode = t.CurrencyCode,
        Description = t.Description,
        Status = t.Status,
        SettlementId = t.SettlementId,
        CreatedAt = t.CreatedAt
    };
}
