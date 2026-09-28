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
        CreateProjectTaskDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create project task");
    }

    public async Task<ProjectTaskDto> UpdateProjectTask(
        [Service] IProjectTaskService service,
        UpdateProjectTaskDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update project task");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("Project task not found");
    }

    public async Task<bool> DeleteProjectTask(
        [Service] IProjectTaskService service,
        Guid taskId)
    {
        return await service.DeleteAsync(taskId);
    }

    public async Task<bool> CompleteProjectTask(
        [Service] IProjectTaskService service,
        Guid taskId)
    {
        return await service.CompleteAsync(taskId);
    }
}

