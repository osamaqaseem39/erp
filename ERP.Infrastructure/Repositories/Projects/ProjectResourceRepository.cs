using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectResourceRepository : IProjectResourceRepository
{
    private readonly ERPDbContext _context;

    public ProjectResourceRepository(ERPDbContext context) => _context = context;

    private IQueryable<ProjectResource> Query(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return _context.ProjectResources.Where(_ => false);
        return _context.ProjectResources.Where(pr => pr.TenantId == tenantId);
    }

    private async Task LoadRelatedAsync(ProjectResource? entity)
    {
        if (entity == null) return;
        await _context.Entry(entity).Reference(pr => pr.Project).LoadAsync();
        await _context.Entry(entity).Reference(pr => pr.Employee).LoadAsync();
    }

    private async Task LoadRelatedAsync(IEnumerable<ProjectResource> entities)
    {
        foreach (var entity in entities)
            await LoadRelatedAsync(entity);
    }

    public async Task<ProjectResource?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await Query(tenantId).FirstOrDefaultAsync(pr => pr.Id == id);
        await LoadRelatedAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<ProjectResource>> GetAllAsync(Guid tenantId)
    {
        var list = await Query(tenantId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectResource>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pr => pr.ProjectId == projectId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectResource>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(pr => pr.EmployeeId == employeeId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(ProjectResource projectResource)
    {
        if (projectResource.Id == Guid.Empty) projectResource.Id = Guid.NewGuid();
        if (projectResource.CreatedAt == default) projectResource.CreatedAt = DateTime.UtcNow;
        _context.ProjectResources.Add(projectResource);
        await _context.SaveChangesAsync();
        return projectResource.Id;
    }

    public async Task<bool> UpdateAsync(ProjectResource projectResource)
    {
        projectResource.UpdatedAt = DateTime.UtcNow;
        _context.ProjectResources.Update(projectResource);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var projectResource = await GetByIdAsync(id, tenantId);
        if (projectResource == null) return false;
        _context.ProjectResources.Remove(projectResource);
        return await _context.SaveChangesAsync() > 0;
    }
}
