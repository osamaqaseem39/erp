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

    private IQueryable<ProjectDocument> Active(Guid tenantId) =>
        _context.ProjectDocuments
            .Include(pd => pd.Project)
            .Where(pd => pd.TenantId == tenantId && pd.IsActive);

    public async Task<ProjectDocument?> GetByIdAsync(Guid id, Guid tenantId)
        => await Active(tenantId).FirstOrDefaultAsync(pd => pd.Id == id);

    public async Task<IEnumerable<ProjectDocument>> GetAllAsync(Guid tenantId)
        => await Active(tenantId).OrderByDescending(pd => pd.CreatedAt).ToListAsync();

    public async Task<IEnumerable<ProjectDocument>> GetByProjectAsync(Guid projectId, Guid tenantId)
        => await Active(tenantId)
            .Where(pd => pd.ProjectId == projectId)
            .OrderByDescending(pd => pd.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<ProjectDocument>> GetByTypeAsync(string documentType, Guid tenantId)
        => await Active(tenantId)
            .Where(pd => pd.DocumentType == documentType)
            .OrderByDescending(pd => pd.CreatedAt)
            .ToListAsync();

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

    public async Task<bool> SoftDeleteAsync(Guid id, Guid tenantId)
    {
        var projectDocument = await _context.ProjectDocuments
            .FirstOrDefaultAsync(pd => pd.Id == id && pd.TenantId == tenantId && pd.IsActive);
        if (projectDocument == null) return false;

        projectDocument.IsActive = false;
        projectDocument.UpdatedAt = DateTime.UtcNow;
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}
