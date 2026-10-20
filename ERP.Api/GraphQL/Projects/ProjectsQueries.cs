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
        string status,
        Guid tenantId)
    {
        return await service.GetByStatusAsync(status, tenantId);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByManager(
        [Service] IProjectService service,
        Guid managerId,
        Guid tenantId)
    {
        return await service.GetByManagerAsync(managerId, tenantId);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByClient(
        [Service] IProjectService service,
        Guid clientId,
        Guid tenantId)
    {
        return await service.GetByClientAsync(clientId, tenantId);
    }

    // Project Task Queries
    public async Task<ProjectTaskDto?> GetProjectTask(
        [Service] IProjectTaskService service,
        Guid taskId,
        Guid tenantId)
        => await service.GetByIdAsync(taskId, tenantId);

    public async Task<IEnumerable<ProjectTaskDto>> GetProjectTasks(
        [Service] IProjectTaskService service,
        Guid tenantId,
        Guid? projectId = null,
        Guid? assignedToId = null,
        string? status = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value, tenantId);
        if (assignedToId.HasValue)
            return await service.GetByAssignedToAsync(assignedToId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Project Resource Queries
    public async Task<ProjectResourceDto?> GetProjectResource(
        [Service] IProjectResourceService service,
        Guid resourceId,
        Guid tenantId)
        => await service.GetByIdAsync(resourceId, tenantId);

    public async Task<IEnumerable<ProjectResourceDto>> GetProjectResources(
        [Service] IProjectResourceService service,
        Guid tenantId,
        Guid? projectId = null,
        Guid? employeeId = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value, tenantId);
        if (employeeId.HasValue)
            return await service.GetByEmployeeAsync(employeeId.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Project Milestone Queries
    public async Task<ProjectMilestoneDto?> GetProjectMilestone(
        [Service] IProjectMilestoneService service,
        Guid milestoneId,
        Guid tenantId)
        => await service.GetByIdAsync(milestoneId, tenantId);

    public async Task<IEnumerable<ProjectMilestoneDto>> GetProjectMilestones(
        [Service] IProjectMilestoneService service,
        Guid tenantId,
        Guid? projectId = null,
        string? status = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    // Project Document Queries
    public async Task<ProjectDocumentDto?> GetProjectDocument(
        [Service] IProjectDocumentService service,
        Guid documentId,
        Guid tenantId)
        => await service.GetByIdAsync(documentId, tenantId);

    public async Task<IEnumerable<ProjectDocumentDto>> GetProjectDocuments(
        [Service] IProjectDocumentService service,
        Guid tenantId,
        Guid? projectId = null,
        string? documentType = null)
    {
        if (projectId.HasValue)
            return await service.GetByProjectAsync(projectId.Value, tenantId);
        if (!string.IsNullOrEmpty(documentType))
            return await service.GetByTypeAsync(documentType, tenantId);
        return await service.GetAllAsync(tenantId);
    }
}
