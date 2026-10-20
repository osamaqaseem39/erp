using ERP.Application.Interfaces.Projects;
using ERP.Shared.DTOs.Projects;
using HotChocolate;

namespace ERP.Api.GraphQL.Projects;

[ExtendObjectType("Mutation")]
public class ProjectsMutations
{
    // Project Mutations
    public async Task<ProjectCreateResult> AddProject(
        [Service] IProjectService service,
        CreateProjectDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<bool> UpdateProject(
        [Service] IProjectService service,
        UpdateProjectDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteProject(
        [Service] IProjectService service,
        Guid projectId,
        Guid tenantId)
    {
        return await service.DeleteAsync(projectId, tenantId);
    }

    public async Task<bool> StartProject(
        [Service] IProjectService service,
        Guid projectId,
        Guid tenantId,
        Guid userId)
    {
        return await service.StartAsync(projectId, tenantId, userId);
    }

    public async Task<bool> CompleteProject(
        [Service] IProjectService service,
        Guid projectId,
        CompleteProjectDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CompleteAsync(projectId, input, tenantId, userId);
    }

    public async Task<bool> PauseProject(
        [Service] IProjectService service,
        Guid projectId,
        PauseProjectDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.PauseAsync(projectId, input, tenantId, userId);
    }

    public async Task<bool> CancelProject(
        [Service] IProjectService service,
        Guid projectId,
        CancelProjectDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CancelAsync(projectId, input, tenantId, userId);
    }

    // Project Task Mutations
    public async Task<ProjectTaskDto> AddProjectTask(
        [Service] IProjectTaskService service,
        CreateProjectTaskDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create project task");
    }

    public async Task<ProjectTaskDto> UpdateProjectTask(
        [Service] IProjectTaskService service,
        UpdateProjectTaskDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update project task");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Project task not found");
    }

    public async Task<bool> DeleteProjectTask(
        [Service] IProjectTaskService service,
        Guid taskId,
        Guid tenantId)
        => await service.DeleteAsync(taskId, tenantId);

    public async Task<bool> CompleteProjectTask(
        [Service] IProjectTaskService service,
        Guid taskId,
        Guid tenantId,
        Guid userId)
        => await service.CompleteAsync(taskId, tenantId, userId);

    public async Task<ProjectMilestoneDto> AddProjectMilestone(
        [Service] IProjectMilestoneService service,
        CreateProjectMilestoneDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create project milestone");
    }

    public async Task<ProjectMilestoneDto> UpdateProjectMilestone(
        [Service] IProjectMilestoneService service,
        UpdateProjectMilestoneDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update project milestone");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Project milestone not found");
    }

    public async Task<bool> DeleteProjectMilestone(
        [Service] IProjectMilestoneService service,
        Guid milestoneId,
        Guid tenantId)
        => await service.DeleteAsync(milestoneId, tenantId);

    public async Task<bool> CompleteProjectMilestone(
        [Service] IProjectMilestoneService service,
        Guid milestoneId,
        Guid tenantId,
        Guid userId)
        => await service.CompleteAsync(milestoneId, tenantId, userId);

    public async Task<ProjectResourceDto> AddProjectResource(
        [Service] IProjectResourceService service,
        CreateProjectResourceDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create project resource");
    }

    public async Task<ProjectResourceDto> UpdateProjectResource(
        [Service] IProjectResourceService service,
        UpdateProjectResourceDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update project resource");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Project resource not found");
    }

    public async Task<bool> DeleteProjectResource(
        [Service] IProjectResourceService service,
        Guid resourceId,
        Guid tenantId)
        => await service.DeleteAsync(resourceId, tenantId);

    // Project Document Mutations
    public async Task<ProjectDocumentDto> AddProjectDocument(
        [Service] IProjectDocumentService service,
        CreateProjectDocumentDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id, input.TenantId)
            ?? throw new Exception("Failed to create project document");
    }

    public async Task<ProjectDocumentDto> UpdateProjectDocument(
        [Service] IProjectDocumentService service,
        UpdateProjectDocumentDto input,
        Guid tenantId)
    {
        var success = await service.UpdateAsync(input, tenantId);
        if (!success)
            throw new Exception("Failed to update project document");
        return await service.GetByIdAsync(input.Id, tenantId)
            ?? throw new Exception("Project document not found");
    }

    public async Task<bool> DeleteProjectDocument(
        [Service] IProjectDocumentService service,
        Guid documentId,
        Guid tenantId)
        => await service.DeleteAsync(documentId, tenantId);

    public async Task<bool> UploadProjectDocument(
        [Service] IProjectDocumentService service,
        Guid documentId,
        Guid tenantId,
        string fileName,
        byte[] content,
        string? updatedBy = null)
        => await service.UploadAsync(documentId, tenantId, fileName, content, updatedBy);
}
