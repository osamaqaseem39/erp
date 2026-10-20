using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectMilestoneRepository : IProjectMilestoneRepository
{
    private readonly ERPDbContext _context;

    public ProjectMilestoneRepository(ERPDbContext context) => _context = context;

    private IQueryable<ProjectMilestone> Query(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return _context.ProjectMilestones.Where(_ => false);
        return _context.ProjectMilestones.Where(pm => pm.TenantId == tenantId);
    }

    private async Task LoadRelatedAsync(ProjectMilestone? entity)
    {
        if (entity == null) return;
        await _context.Entry(entity).Reference(pm => pm.Project).LoadAsync();
    }

    private async Task LoadRelatedAsync(IEnumerable<ProjectMilestone> entities)
    {
        foreach (var entity in entities)
            await LoadRelatedAsync(entity);
    }

    public async Task<ProjectMilestone?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await Query(tenantId).FirstOrDefaultAsync(pm => pm.Id == id);
        await LoadRelatedAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<ProjectMilestone>> GetAllAsync(Guid tenantId)
    {
        var list = await Query(tenantId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectMilestone>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pm => pm.ProjectId == projectId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectMilestone>> GetByStatusAsync(string status, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pm => pm.Status == status).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(ProjectMilestone projectMilestone)
    {
        if (projectMilestone.Id == Guid.Empty) projectMilestone.Id = Guid.NewGuid();
        if (projectMilestone.CreatedAt == default) projectMilestone.CreatedAt = DateTime.UtcNow;
        _context.ProjectMilestones.Add(projectMilestone);
        await _context.SaveChangesAsync();
        return projectMilestone.Id;
    }

    public async Task<bool> UpdateAsync(ProjectMilestone projectMilestone)
    {
        projectMilestone.UpdatedAt = DateTime.UtcNow;
        _context.ProjectMilestones.Update(projectMilestone);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var projectMilestone = await GetByIdAsync(id, tenantId);
        if (projectMilestone == null) return false;
        _context.ProjectMilestones.Remove(projectMilestone);
        return await _context.SaveChangesAsync() > 0;
    }
}
