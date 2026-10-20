using ERP.Application.Interfaces.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;

namespace ERP.Application.Projects;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _repository;
    private readonly IProjectRepository _projects;

    public ProjectMemberService(IProjectMemberRepository repository, IProjectRepository projects)
    {
        _repository = repository;
        _projects = projects;
    }

    public async Task<ProjectMemberDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectMemberDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetByProjectAsync(Guid projectId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectMemberDto>();
        return (await _repository.GetByProjectAsync(projectId, tenantId)).Select(Map);
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetByEmployeeAsync(Guid employeeId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Enumerable.Empty<ProjectMemberDto>();
        return (await _repository.GetByEmployeeAsync(employeeId, tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateProjectMemberDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.ProjectId == Guid.Empty || dto.EmployeeId == Guid.Empty)
            return Guid.Empty;
        var project = await _projects.GetByIdAsync(dto.ProjectId, tenantId);
        if (project == null) return Guid.Empty;

        var entity = new ProjectMember
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = dto.ProjectId,
            EmployeeId = dto.EmployeeId,
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Member" : dto.Role.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status!,
            Notes = dto.Notes,
            CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateProjectMemberDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (dto.Role != null) e.Role = dto.Role;
        if (dto.StartDate.HasValue) e.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) e.EndDate = dto.EndDate;
        if (dto.Status != null) e.Status = dto.Status;
        if (dto.Notes != null) e.Notes = dto.Notes;
        e.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    private static ProjectMemberDto Map(ProjectMember e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ProjectId = e.ProjectId,
        ProjectName = e.Project?.ProjectName,
        EmployeeId = e.EmployeeId,
        EmployeeName = e.Employee != null
            ? $"{e.Employee.FirstName} {e.Employee.LastName}".Trim()
            : null,
        Role = e.Role,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Status = e.Status,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
