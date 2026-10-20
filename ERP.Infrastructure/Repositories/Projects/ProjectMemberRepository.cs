using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Projects;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly ERPDbContext _context;

    public ProjectMemberRepository(ERPDbContext context) => _context = context;

    private IQueryable<ProjectMember> Query(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return _context.ProjectMembers.Where(_ => false);
        return _context.ProjectMembers.Where(m => m.TenantId == tenantId && m.IsActive);
    }

    private async Task LoadRelatedAsync(ProjectMember? entity)
    {
        if (entity == null) return;
        await _context.Entry(entity).Reference(m => m.Project).LoadAsync();
        await _context.Entry(entity).Reference(m => m.Employee).LoadAsync();
    }

    private async Task LoadRelatedAsync(IEnumerable<ProjectMember> entities)
    {
        foreach (var entity in entities)
            await LoadRelatedAsync(entity);
    }

    public async Task<ProjectMember?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await Query(tenantId).FirstOrDefaultAsync(m => m.Id == id);
        await LoadRelatedAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<ProjectMember>> GetAllAsync(Guid tenantId)
    {
        var list = await Query(tenantId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectMember>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(m => m.ProjectId == projectId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<IEnumerable<ProjectMember>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        var list = await Query(tenantId).Where(m => m.EmployeeId == employeeId).ToListAsync();
        await LoadRelatedAsync(list);
        return list;
    }

    public async Task<Guid> CreateAsync(ProjectMember member)
    {
        if (member.Id == Guid.Empty) member.Id = Guid.NewGuid();
        if (member.CreatedAt == default) member.CreatedAt = DateTime.UtcNow;
        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();
        return member.Id;
    }

    public async Task<bool> UpdateAsync(ProjectMember member)
    {
        member.UpdatedAt = DateTime.UtcNow;
        _context.ProjectMembers.Update(member);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var member = await GetByIdAsync(id, tenantId);
        if (member == null) return false;
        member.IsActive = false;
        member.Status = "Inactive";
        member.UpdatedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
}
