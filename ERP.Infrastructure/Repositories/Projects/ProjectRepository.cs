using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectRepository : IProjectRepository
{
    private readonly ERPDbContext _context;

    public ProjectRepository(ERPDbContext context)
    {
        _context = context;
    }

    private IQueryable<Project> Query(Guid tenantId) =>
        _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .Where(p => p.TenantId == tenantId);

    public async Task<Project?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Project>();
        return await Query(tenantId).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Project>();
        return await Query(tenantId).Where(p => p.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByManagerAsync(Guid managerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Project>();
        return await Query(tenantId).Where(p => p.ProjectManagerId == managerId).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByClientAsync(Guid clientId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Project>();
        return await Query(tenantId).Where(p => p.ClientId == clientId).ToListAsync();
    }

    public async Task<Project?> GetByCodeAsync(string projectCode, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await Query(tenantId).FirstOrDefaultAsync(p => p.ProjectCode == projectCode);
    }

    public async Task<Guid> CreateAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project.Id;
    }

    public async Task<bool> UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var project = await GetByIdAsync(id, tenantId);
        if (project == null) return false;

        _context.Projects.Remove(project);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
