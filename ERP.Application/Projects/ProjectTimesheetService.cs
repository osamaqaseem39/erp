using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Projects;

public interface IProjectTimesheetService
{
    Task<IReadOnlyList<ProjectTimesheetDto>> ListAsync(Guid tenantId, Guid? projectId = null);
    Task<ProjectTimesheetDto?> GetAsync(Guid id, Guid tenantId);
    Task<ProjectTimesheetDto> CreateAsync(CreateProjectTimesheetDto dto, Guid tenantId, Guid userId);
    Task<ProjectTimesheetDto> SubmitAsync(Guid id, Guid tenantId, Guid userId);
    Task<ProjectTimesheetDto> ApproveAsync(Guid id, Guid tenantId, Guid userId);
}

public class ProjectTimesheetService : IProjectTimesheetService
{
    private readonly ERPDbContext _db;
    private readonly IProjectRepository _projects;
    private readonly IProjectCostService _costs;

    public ProjectTimesheetService(ERPDbContext db, IProjectRepository projects, IProjectCostService costs)
    {
        _db = db;
        _projects = projects;
        _costs = costs;
    }

    public async Task<IReadOnlyList<ProjectTimesheetDto>> ListAsync(Guid tenantId, Guid? projectId = null)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ProjectTimesheetDto>();
        var q = _db.ProjectTimesheets.AsNoTracking()
            .Include(t => t.Lines)
            .Where(t => t.TenantId == tenantId && t.IsActive);
        if (projectId.HasValue && projectId != Guid.Empty)
            q = q.Where(t => t.ProjectId == projectId);
        return (await q.OrderByDescending(t => t.PeriodStart).ToListAsync()).Select(Map).ToList();
    }

    public async Task<ProjectTimesheetDto?> GetAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _db.ProjectTimesheets.AsNoTracking()
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<ProjectTimesheetDto> CreateAsync(CreateProjectTimesheetDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");
        if (dto.ProjectId == Guid.Empty) throw new ValidationException("Project is required.");
        if (dto.EmployeeId == Guid.Empty) throw new ValidationException("Employee is required.");
        _ = await _projects.GetByIdAsync(dto.ProjectId, tenantId) ?? throw new NotFoundException("Project not found.");

        var defaultRate = await _db.ProjectResources.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.ProjectId == dto.ProjectId
                && r.EmployeeId == dto.EmployeeId && r.IsActive)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Select(r => (decimal?)r.HourlyRate)
            .FirstOrDefaultAsync() ?? 0m;

        var entity = new ProjectTimesheet
        {
            TenantId = tenantId,
            ProjectId = dto.ProjectId,
            EmployeeId = dto.EmployeeId,
            PeriodStart = dto.PeriodStart.Date,
            PeriodEnd = dto.PeriodEnd.Date,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString()
        };
        var n = 1;
        foreach (var line in dto.Lines)
        {
            entity.Lines.Add(new ProjectTimesheetLine
            {
                TenantId = tenantId,
                LineNumber = line.LineNumber > 0 ? line.LineNumber : n++,
                WorkDate = line.WorkDate.Date,
                ProjectTaskId = line.ProjectTaskId,
                Hours = line.Hours,
                Description = line.Description,
                IsBillable = line.IsBillable ?? true,
                HourlyRate = line.HourlyRate ?? defaultRate,
                CreatedBy = userId.ToString()
            });
        }
        entity.TotalHours = Math.Round(entity.Lines.Sum(l => l.Hours), 4);

        _db.ProjectTimesheets.Add(entity);
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<ProjectTimesheetDto> SubmitAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, userId, "Draft", "Submitted", t => t.SubmittedAt = DateTime.UtcNow);

    public async Task<ProjectTimesheetDto> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var result = await TransitionAsync(id, tenantId, userId, "Submitted", "Approved", t =>
        {
            t.ApprovedAt = DateTime.UtcNow;
            t.ApprovedBy = userId;
        });
        await _costs.RecalculateActualCostAsync(result.ProjectId, tenantId);
        return result;
    }

    private async Task<ProjectTimesheetDto> TransitionAsync(
        Guid id, Guid tenantId, Guid userId, string from, string to, Action<ProjectTimesheet> apply)
    {
        var entity = await _db.ProjectTimesheets.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId)
            ?? throw new NotFoundException("Timesheet not found.");
        if (!string.Equals(entity.Status, from, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Timesheet must be in {from} status.");
        entity.Status = to;
        apply(entity);
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(entity);
    }

    private static ProjectTimesheetDto Map(ProjectTimesheet e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProjectId = e.ProjectId,
        EmployeeId = e.EmployeeId,
        PeriodStart = e.PeriodStart,
        PeriodEnd = e.PeriodEnd,
        Status = e.Status,
        TotalHours = e.TotalHours,
        Notes = e.Notes,
        Lines = e.Lines.OrderBy(l => l.LineNumber).Select(l => new ProjectTimesheetLineDto
        {
            Id = l.Id,
            LineNumber = l.LineNumber,
            WorkDate = l.WorkDate,
            ProjectTaskId = l.ProjectTaskId,
            Hours = l.Hours,
            Description = l.Description,
            IsBillable = l.IsBillable,
            HourlyRate = l.HourlyRate
        }).ToList()
    };
}
