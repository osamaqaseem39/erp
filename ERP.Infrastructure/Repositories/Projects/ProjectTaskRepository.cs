using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectTaskRepository : IProjectTaskRepository
{
    private readonly ERPDbContext _context;

    public ProjectTaskRepository(ERPDbContext context) => _context = context;

    private IQueryable<ProjectTask> Query(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return _context.ProjectTasks.Where(_ => false);
        return _context.ProjectTasks.Where(pt => pt.TenantId == tenantId);
    }

    private async Task LoadRelatedAsync(ProjectTask? entity)
    {
        if (entity == null) return;
        await _context.Entry(entity).Reference(pt => pt.Project).LoadAsync();
        await _context.Entry(entity).Reference(pt => pt.AssignedTo).LoadAsync();
    }

    private async Task LoadRelatedAsync(IEnumerable<ProjectTask> entities)
    {
        foreach (var entity in entities)
            await LoadRelatedAsync(entity);
    }

    public async Task<ProjectTask?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await Query(tenantId).FirstOrDefaultAsync(pt => pt.Id == id);
        await LoadRelatedAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<ProjectTask>> GetAllAsync(Guid tenantId)
    {
        var list = await Query(tenantId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectTask>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pt => pt.ProjectId == projectId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectTask>> GetByAssignedToAsync(Guid assignedToId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pt => pt.AssignedToId == assignedToId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectTask>> GetByStatusAsync(string status, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pt => pt.Status == status).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(ProjectTask projectTask)
    {
        if (projectTask.Id == Guid.Empty) projectTask.Id = Guid.NewGuid();
        if (projectTask.CreatedAt == default) projectTask.CreatedAt = DateTime.UtcNow;
        _context.ProjectTasks.Add(projectTask);
        await _context.SaveChangesAsync();
        return projectTask.Id;
    }

    public async Task<bool> UpdateAsync(ProjectTask projectTask)
    {
        projectTask.UpdatedAt = DateTime.UtcNow;
        _context.ProjectTasks.Update(projectTask);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var projectTask = await GetByIdAsync(id, tenantId);
        if (projectTask == null) return false;
        _context.ProjectTasks.Remove(projectTask);
        return await _context.SaveChangesAsync() > 0;
    }
}
