using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IOpeningBalanceService
{
    Task<IReadOnlyList<OpeningBalanceDto>> ListAsync(Guid tenantId, Guid? fiscalYearId = null);
    Task<OpeningBalanceDto?> GetAsync(Guid id, Guid tenantId);
    Task<OpeningBalanceDto> CreateAsync(CreateOpeningBalanceDto dto, Guid tenantId, Guid userId);
    Task<OpeningBalanceDto> PostAsync(Guid id, Guid tenantId, Guid userId);
}

public class OpeningBalanceService : IOpeningBalanceService
{
    private readonly ERPDbContext _db;
    private readonly IAccountingService _accounting;

    public OpeningBalanceService(ERPDbContext db, IAccountingService accounting)
    {
        _db = db;
        _accounting = accounting;
    }

    public async Task<IReadOnlyList<OpeningBalanceDto>> ListAsync(Guid tenantId, Guid? fiscalYearId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<OpeningBalanceDto>();
        var q = _db.OpeningBalances.AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.TenantId == tenantId && o.IsActive);
        if (fiscalYearId.HasValue && fiscalYearId != Guid.Empty)
            q = q.Where(o => o.FiscalYearId == fiscalYearId);
        return (await q.OrderByDescending(o => o.AsOfDate).ToListAsync()).Select(Map).ToList();
    }

    public async Task<OpeningBalanceDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.OpeningBalances.AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<OpeningBalanceDto> CreateAsync(CreateOpeningBalanceDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.FiscalYearId == Guid.Empty) throw new ValidationException("Fiscal year is required.");
        if (dto.Lines.Count == 0) throw new ValidationException("At least one line is required.");

        var fy = await _db.FiscalYears.AsNoTracking()
            .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.FiscalYearId == dto.FiscalYearId);
        if (fy == null) throw new NotFoundException("Fiscal year not found.");

        var entity = new OpeningBalance
        {
            TenantId = tenantId,
            FiscalYearId = dto.FiscalYearId,
            ReferenceNo = dto.ReferenceNo?.Trim(),
            AsOfDate = dto.AsOfDate == default ? DateTime.UtcNow.Date : dto.AsOfDate.Date,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        var n = 1;
        foreach (var line in dto.Lines.OrderBy(l => l.LineNumber))
        {
            entity.Lines.Add(new OpeningBalanceLine
            {
                TenantId = tenantId,
                LineNumber = line.LineNumber > 0 ? line.LineNumber : n++,
                AccountId = line.AccountId,
                Debit = line.Debit,
                Credit = line.Credit,
                CostCenterId = line.CostCenterId,
                ProjectId = line.ProjectId,
                PartyType = line.PartyType,
                PartyId = line.PartyId,
                Narration = line.Narration,
                CreatedBy = userId.ToString()
            });
        }

        _db.OpeningBalances.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<OpeningBalanceDto> PostAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        var entity = await _db.OpeningBalances
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId)
            ?? throw new NotFoundException("Opening balance not found.");
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only draft opening balances can be posted.");

        var debit = Math.Round(entity.Lines.Sum(l => l.Debit), 2);
        var credit = Math.Round(entity.Lines.Sum(l => l.Credit), 2);
        if (debit != credit) throw new ValidationException("Opening balance lines must be balanced.");

        var journalId = await _accounting.AddJournalAsync(new JournalCreateDto
        {
            JournalType = "OpeningBalance",
            ReferenceNo = entity.ReferenceNo,
            JournalDate = entity.AsOfDate,
            Description = entity.Notes ?? $"Opening balance FY {entity.FiscalYearId}",
            EntityReferenceTable = "OpeningBalances",
            EntityReferenceId = entity.Id
        }, tenantId, userId);

        if (journalId != Guid.Empty)
        {
            foreach (var line in entity.Lines.OrderBy(l => l.LineNumber))
            {
                await _accounting.AddJournalEntryAsync(new CreateJournalEntryDto
                {
                    AccountId = line.AccountId,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    CostCenterId = line.CostCenterId,
                    ProjectId = line.ProjectId,
                    Narration = line.Narration
                }, journalId, tenantId, userId);
            }

            var posted = await _accounting.PostJournalAsync(journalId, tenantId, userId, allowPostWithoutApprove: true);
            if (posted != Guid.Empty) entity.JournalId = posted;
        }

        entity.Status = "Posted";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static OpeningBalanceDto Map(OpeningBalance e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        FiscalYearId = e.FiscalYearId,
        ReferenceNo = e.ReferenceNo,
        AsOfDate = e.AsOfDate,
        Status = e.Status,
        JournalId = e.JournalId,
        Notes = e.Notes,
        Lines = e.Lines.OrderBy(l => l.LineNumber).Select(l => new OpeningBalanceLineDto
        {
            Id = l.Id,
            LineNumber = l.LineNumber,
            AccountId = l.AccountId,
            Debit = l.Debit,
            Credit = l.Credit,
            CostCenterId = l.CostCenterId,
            ProjectId = l.ProjectId,
            PartyType = l.PartyType,
            PartyId = l.PartyId,
            Narration = l.Narration
        }).ToList()
    };
}
