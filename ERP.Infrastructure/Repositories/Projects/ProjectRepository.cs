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

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(string status)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByManagerAsync(Guid managerId)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .Where(p => p.ProjectManagerId == managerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByClientAsync(Guid clientId)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .Where(p => p.ClientId == clientId)
            .ToListAsync();
    }

    public async Task<Project?> GetByCodeAsync(string projectCode)
    {
        return await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.ProjectManager)
            .Include(p => p.Tasks)
            .Include(p => p.Resources)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.ProjectCode == projectCode);
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        _context.Projects.Remove(project);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}