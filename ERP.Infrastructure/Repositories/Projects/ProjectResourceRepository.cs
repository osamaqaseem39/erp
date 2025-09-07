using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectResourceRepository : IProjectResourceRepository
{
    private readonly ERPDbContext _context;

    public ProjectResourceRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectResource?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectResources
            .Include(pr => pr.Project)
            .Include(pr => pr.Employee)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<IEnumerable<ProjectResource>> GetAllAsync()
    {
        return await _context.ProjectResources
            .Include(pr => pr.Project)
            .Include(pr => pr.Employee)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectResource>> GetByProjectAsync(Guid projectId)
    {
        return await _context.ProjectResources
            .Include(pr => pr.Project)
            .Include(pr => pr.Employee)
            .Where(pr => pr.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectResource>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _context.ProjectResources
            .Include(pr => pr.Project)
            .Include(pr => pr.Employee)
            .Where(pr => pr.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProjectResource projectResource)
    {
        _context.ProjectResources.Add(projectResource);
        await _context.SaveChangesAsync();
        return projectResource.Id;
    }

    public async Task<bool> UpdateAsync(ProjectResource projectResource)
    {
        _context.ProjectResources.Update(projectResource);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var projectResource = await _context.ProjectResources.FindAsync(id);
        if (projectResource == null) return false;

        _context.ProjectResources.Remove(projectResource);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}