using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectDocumentRepository : IProjectDocumentRepository
{
    private readonly ERPDbContext _context;

    public ProjectDocumentRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDocument?> GetByIdAsync(Guid id)
    {
        return await _context.ProjectDocuments
            .Include(pd => pd.Project)
            .FirstOrDefaultAsync(pd => pd.Id == id);
    }

    public async Task<IEnumerable<ProjectDocument>> GetAllAsync()
    {
        return await _context.ProjectDocuments
            .Include(pd => pd.Project)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectDocument>> GetByProjectAsync(Guid projectId)
    {
        return await _context.ProjectDocuments
            .Include(pd => pd.Project)
            .Where(pd => pd.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectDocument>> GetByTypeAsync(string documentType)
    {
        return await _context.ProjectDocuments
            .Include(pd => pd.Project)
            .Where(pd => pd.DocumentType == documentType)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(ProjectDocument projectDocument)
    {
        _context.ProjectDocuments.Add(projectDocument);
        await _context.SaveChangesAsync();
        return projectDocument.Id;
    }

    public async Task<bool> UpdateAsync(ProjectDocument projectDocument)
    {
        _context.ProjectDocuments.Update(projectDocument);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var projectDocument = await _context.ProjectDocuments.FindAsync(id);
        if (projectDocument == null) return false;

        _context.ProjectDocuments.Remove(projectDocument);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}