using ERP.Application.Interfaces.Projects;
using ERP.Shared.DTOs.Projects;
using HotChocolate;

namespace ERP.Api.GraphQL.Projects;

[ExtendObjectType("Query")]
public class ProjectsQueries
{
    // Project Queries
    public async Task<ProjectDto?> GetProject(
        [Service] IProjectService service,
        Guid projectId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(projectId, tenantId);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjects(
        [Service] IProjectService service,
        Guid tenantId,
        string? status = null,
        Guid? managerId = null,
        Guid? customerId = null)
    {
        return await service.GetAllAsync(tenantId, status, managerId, customerId);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByStatus(
        [Service] IProjectService service,
        string status)
    {
        return await service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByManager(
        [Service] IProjectService service,
        Guid managerId)
    {
        return await service.GetByManagerAsync(managerId);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByClient(
        [Service] IProjectService service,
        Guid clientId)
    {
        return await service.GetByClientAsync(clientId);
    }

    // Project Task Queries
    public async Task<ProjectTaskDto?> GetProjectTask(
        [Service] IProjectTaskService service,
        Guid taskId)
    {
        return await service.GetByIdAsync(taskId);
    }

    public async Task<IEnumerable<ProjectTaskDto>> GetProjectTasks(
        [Service] IProjectTaskService service,
        Guid? projectId = null,
        Guid? assignedToId = null,
        string? status = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value);
        if (assignedToId.HasValue)
            return await service.GetByAssignedToAsync(assignedToId.Value);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status);
        return await service.GetAllAsync();
    }

    // Project Resource Queries
    public async Task<ProjectResourceDto?> GetProjectResource(
        [Service] IProjectResourceService service,
        Guid resourceId)
    {
        return await service.GetByIdAsync(resourceId);
    }

    public async Task<IEnumerable<ProjectResourceDto>> GetProjectResources(
        [Service] IProjectResourceService service,
        Guid? projectId = null,
        Guid? employeeId = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value);
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value);
        return await service.GetAllAsync();
    }

    // Project Milestone Queries
    public async Task<ProjectMilestoneDto?> GetProjectMilestone(
        [Service] IProjectMilestoneService service,
        Guid milestoneId)
    {
        return await service.GetByIdAsync(milestoneId);
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetProjectMilestones(
        [Service] IProjectMilestoneService service,
        Guid? projectId = null,
        string? status = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status);
        return await service.GetAllAsync();
    }
}

