using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Projects;

public interface IProjectCostService
{
    Task<ProjectCostSummaryDto?> GetCostSummaryAsync(Guid projectId, Guid tenantId);
    Task<ProjectCostSummaryDto?> RecalculateActualCostAsync(Guid projectId, Guid tenantId);
}

public class ProjectCostService : IProjectCostService
{
    private readonly ERPDbContext _db;
    private readonly IProjectRepository _projects;

    public ProjectCostService(ERPDbContext db, IProjectRepository projects)
    {
        _db = db;
        _projects = projects;
    }

    public async Task<ProjectCostSummaryDto?> GetCostSummaryAsync(Guid projectId, Guid tenantId)
    {
        if (tenantId == Guid.Empty || projectId == Guid.Empty) return null;
        var project = await _projects.GetByIdAsync(projectId, tenantId);
        if (project == null) return null;
        var breakdown = await ComputeAsync(projectId, tenantId);
        return new ProjectCostSummaryDto
        {
            ProjectId = projectId,
            Budget = project.Budget,
            LaborCost = breakdown.Labor,
            ExpenseCost = breakdown.Expenses,
            GlDebitCost = breakdown.Gl,
            ActualCost = breakdown.Total,
            StoredActualCost = project.ActualCost
        };
    }

    public async Task<ProjectCostSummaryDto?> RecalculateActualCostAsync(Guid projectId, Guid tenantId)
    {
        if (tenantId == Guid.Empty || projectId == Guid.Empty) return null;
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.TenantId == tenantId);
        if (project == null) return null;

        var breakdown = await ComputeAsync(projectId, tenantId);
        project.ActualCost = breakdown.Total;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new ProjectCostSummaryDto
        {
            ProjectId = projectId,
            Budget = project.Budget,
            LaborCost = breakdown.Labor,
            ExpenseCost = breakdown.Expenses,
            GlDebitCost = breakdown.Gl,
            ActualCost = breakdown.Total,
            StoredActualCost = project.ActualCost
        };
    }

    private async Task<(decimal Labor, decimal Expenses, decimal Gl, decimal Total)> ComputeAsync(Guid projectId, Guid tenantId)
    {
        var labor = await _db.ProjectTimesheets.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.ProjectId == projectId && t.IsActive
                && t.Status == "Approved")
            .SelectMany(t => t.Lines)
            .Where(l => l.IsBillable && l.IsActive)
            .SumAsync(l => (decimal?)(l.Hours * l.HourlyRate)) ?? 0m;

        var expenses = await _db.ExpenseClaims.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.ProjectId == projectId && c.IsActive
                && (c.Status == "Approved" || c.Status == "Paid"))
            .SumAsync(c => (decimal?)c.TotalAmount) ?? 0m;

        // Posted journals only (Journal.Status == Posted) with ProjectId debits.
        var gl = await (
            from e in _db.JournalEntries.AsNoTracking()
            join j in _db.Journals.AsNoTracking() on e.JournalId equals j.Id
            where e.TenantId == tenantId && e.ProjectId == projectId && e.Debit > 0
                  && j.Status == "Posted"
            select e.Debit
        ).SumAsync(d => (decimal?)d) ?? 0m;

        labor = Math.Round(labor, 2);
        expenses = Math.Round(expenses, 2);
        gl = Math.Round(gl, 2);
        return (labor, expenses, gl, Math.Round(labor + expenses + gl, 2));
    }
}
