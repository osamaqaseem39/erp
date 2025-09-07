using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectMilestoneRepository : IProjectMilestoneRepository
{
    private readonly ERPDbContext _context;

    public ProjectMilestoneRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMilestone?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectMilestones
            .Include(pm => pm.Project)
            .FirstOrDefaultAsync(pm => pm.Id == id);
    }

    public async Task<IEnumerable<ProjectMilestone>> GetAllAsync()
    {
        return await _context.ProjectMilestones
            .Include(pm => pm.Project)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectMilestone>> GetByProjectAsync(Guid projectId)
    {
        return await _context.ProjectMilestones
            .Include(pm => pm.Project)
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectMilestone>> GetByStatusAsync(string status)
    {
        return await _context.ProjectMilestones
            .Include(pm => pm.Project)
            .Where(pm => pm.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProjectMilestone projectMilestone)
    {
        _context.ProjectMilestones.Add(projectMilestone);
        await _context.SaveChangesAsync();
        return projectMilestone.Id;
    }

    public async Task<bool> UpdateAsync(ProjectMilestone projectMilestone)
    {
        _context.ProjectMilestones.Update(projectMilestone);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var projectMilestone = await _context.ProjectMilestones.FindAsync(id);
        if (projectMilestone == null) return false;

        _context.ProjectMilestones.Remove(projectMilestone);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}