using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectTaskRepository : IProjectTaskRepository
{
    private readonly ERPDbContext _context;

    public ProjectTaskRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectTask?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectTasks
            .Include(pt => pt.Project)
            .Include(pt => pt.AssignedTo)
            .Include(pt => pt.ParentTask)
            .Include(pt => pt.Subtasks)
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async Task<IEnumerable<ProjectTask>> GetAllAsync()
    {
        return await _context.ProjectTasks
            .Include(pt => pt.Project)
            .Include(pt => pt.AssignedTo)
            .Include(pt => pt.ParentTask)
            .Include(pt => pt.Subtasks)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetByProjectAsync(Guid projectId)
    {
        return await _context.ProjectTasks
            .Include(pt => pt.Project)
            .Include(pt => pt.AssignedTo)
            .Include(pt => pt.ParentTask)
            .Include(pt => pt.Subtasks)
            .Where(pt => pt.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetByAssignedToAsync(Guid assignedToId)
    {
        return await _context.ProjectTasks
            .Include(pt => pt.Project)
            .Include(pt => pt.AssignedTo)
            .Include(pt => pt.ParentTask)
            .Include(pt => pt.Subtasks)
            .Where(pt => pt.AssignedToId == assignedToId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTask>> GetByStatusAsync(string status)
    {
        return await _context.ProjectTasks
            .Include(pt => pt.Project)
            .Include(pt => pt.AssignedTo)
            .Include(pt => pt.ParentTask)
            .Include(pt => pt.Subtasks)
            .Where(pt => pt.Status == status)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProjectTask projectTask)
    {
        _context.ProjectTasks.Add(projectTask);
        await _context.SaveChangesAsync();
        return projectTask.Id;
    }

    public async Task<bool> UpdateAsync(ProjectTask projectTask)
    {
        _context.ProjectTasks.Update(projectTask);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var projectTask = await _context.ProjectTasks.FindAsync(id);
        if (projectTask == null) return false;

        _context.ProjectTasks.Remove(projectTask);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}