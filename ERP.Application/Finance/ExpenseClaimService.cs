using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

public interface IExpenseClaimService
{
    Task<IReadOnlyList<ExpenseClaimDto>> ListAsync(Guid tenantId, string? status = null);
    Task<ExpenseClaimDto?> GetAsync(Guid id, Guid tenantId);
    Task<ExpenseClaimDto> CreateAsync(CreateExpenseClaimDto dto, Guid tenantId, Guid userId);
    Task<ExpenseClaimDto> SubmitAsync(Guid id, Guid tenantId, Guid userId);
    Task<ExpenseClaimDto> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<ExpenseClaimDto> MarkPaidAsync(Guid id, Guid tenantId, Guid userId);
}

public class ExpenseClaimService : IExpenseClaimService
{
    private readonly ERPDbContext _db;
    private readonly ERP.Application.Projects.IProjectCostService? _projectCosts;

    public ExpenseClaimService(
        ERPDbContext db,
        ERP.Application.Projects.IProjectCostService? projectCosts = null)
    {
        _db = db;
        _projectCosts = projectCosts;
    }

    public async Task<IReadOnlyList<ExpenseClaimDto>> ListAsync(Guid tenantId, string? status = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ExpenseClaimDto>();
        var q = _db.ExpenseClaims.AsNoTracking()
            .Include(c => c.Lines)
            .Where(c => c.TenantId == tenantId && c.IsActive);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(c => c.Status == status);
        return (await q.OrderByDescending(c => c.ClaimDate).ToListAsync()).Select(Map).ToList();
    }

    public async Task<ExpenseClaimDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.ExpenseClaims.AsNoTracking()
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<ExpenseClaimDto> CreateAsync(CreateExpenseClaimDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        var claimNo = string.IsNullOrWhiteSpace(dto.ClaimNumber)
            ? $"EC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
            : dto.ClaimNumber.Trim();

        var entity = new ExpenseClaim
        {
            TenantId = tenantId,
            ClaimNumber = claimNo,
            EmployeeId = dto.EmployeeId,
            ProjectId = dto.ProjectId,
            ClaimDate = dto.ClaimDate == default ? DateTime.UtcNow.Date : dto.ClaimDate.Date,
            Status = "Draft",
            CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "USD" : dto.CurrencyCode.Trim(),
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        var n = 1;
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new ExpenseClaimLine
            {
                TenantId = tenantId,
                LineNumber = line.LineNumber > 0 ? line.LineNumber : n++,
                ExpenseDate = line.ExpenseDate == default ? entity.ClaimDate : line.ExpenseDate.Date,
                Category = line.Category,
                Description = line.Description,
                Amount = line.Amount,
                AccountId = line.AccountId,
                CreatedBy = userId.ToString()
            });
        }
        entity.TotalAmount = Math.Round(entity.Lines.Sum(l => l.Amount), 2);

        _db.ExpenseClaims.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<ExpenseClaimDto> SubmitAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, userId, "Draft", "Submitted", e => e.SubmittedAt = DateTime.UtcNow);

    public async Task<ExpenseClaimDto> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var result = await TransitionAsync(id, tenantId, userId, "Submitted", "Approved", e =>
        {
            e.ApprovedAt = DateTime.UtcNow;
            e.ApprovedBy = userId;
        });
        if (_projectCosts != null && result.ProjectId.HasValue && result.ProjectId != Guid.Empty)
            await _projectCosts.RecalculateActualCostAsync(result.ProjectId.Value, tenantId);
        return result;
    }

    public async Task<ExpenseClaimDto> MarkPaidAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, userId, "Approved", "Paid", e => e.PaidAt = DateTime.UtcNow);

    private async Task<ExpenseClaimDto> TransitionAsync(
        Guid id, Guid tenantId, Guid userId, string from, string to, Action<ExpenseClaim> apply)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        var entity = await _db.ExpenseClaims.Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId)
            ?? throw new NotFoundException("Expense claim not found.");
        if (!string.Equals(entity.Status, from, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Claim must be in {from} status.");
        entity.Status = to;
        apply(entity);
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static ExpenseClaimDto Map(ExpenseClaim e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ClaimNumber = e.ClaimNumber,
        EmployeeId = e.EmployeeId,
        ProjectId = e.ProjectId,
        ClaimDate = e.ClaimDate,
        Status = e.Status,
        TotalAmount = e.TotalAmount,
        CurrencyCode = e.CurrencyCode,
        Notes = e.Notes,
        Lines = e.Lines.OrderBy(l => l.LineNumber).Select(l => new ExpenseClaimLineDto
        {
            Id = l.Id,
            LineNumber = l.LineNumber,
            ExpenseDate = l.ExpenseDate,
            Category = l.Category,
            Description = l.Description,
            Amount = l.Amount,
            AccountId = l.AccountId
        }).ToList()
    };
}
