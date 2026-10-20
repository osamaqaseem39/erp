using ERP.Application.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Projects;

public class ProjectTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Create_Stamps_Caller_Tenant()
    {
        await using var db = Db();
        var svc = new ProjectService(new ProjectRepository(db));

        var created = await svc.CreateAsync(new CreateProjectDto
        {
            ProjectCode = "A-1",
            ProjectName = "Alpha",
            Priority = "Medium"
        }, TenantA, UserA);

        var row = await db.Projects.SingleAsync(p => p.Id == created.ProjectId);
        Assert.Equal(TenantA, row.TenantId);
    }

    [Fact]
    public async Task GetAll_Only_Callers_Tenant()
    {
        await using var db = Db();
        db.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), TenantId = TenantA, ProjectCode = "A", ProjectName = "A", Status = "Planning", CreatedAt = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), TenantId = TenantB, ProjectCode = "B", ProjectName = "B", Status = "Planning", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc = new ProjectService(new ProjectRepository(db));
        var all = (await svc.GetAllAsync(TenantA)).ToList();
        Assert.Single(all);
        Assert.Equal("A", all[0].ProjectCode);
    }

    [Fact]
    public async Task Other_Tenant_Cannot_Get_Update_Delete_Or_Start()
    {
        await using var db = Db();
        var projectId = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = projectId,
            TenantId = TenantB,
            ProjectCode = "B-1",
            ProjectName = "Secret",
            Status = "Planning",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = new ProjectService(new ProjectRepository(db));

        Assert.Null(await svc.GetByIdAsync(projectId, TenantA));
        Assert.False(await svc.UpdateAsync(new UpdateProjectDto
        {
            Id = projectId,
            ProjectCode = "Hijack",
            ProjectName = "Hijack",
            Status = "Planning",
            Priority = "High"
        }, TenantA, UserA));
        Assert.False(await svc.DeleteAsync(projectId, TenantA));
        Assert.False(await svc.StartAsync(projectId, TenantA, UserA));

        var untouched = await db.Projects.SingleAsync(p => p.Id == projectId);
        Assert.Equal("Secret", untouched.ProjectName);
        Assert.Equal("Planning", untouched.Status);
    }

    [Fact]
    public async Task Empty_Tenant_Fails_Closed_On_Reads()
    {
        await using var db = Db();
        db.Projects.Add(new Project
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            ProjectCode = "A",
            ProjectName = "A",
            Status = "Planning",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repo = new ProjectRepository(db);
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));
        Assert.Null(await repo.GetByIdAsync(db.Projects.First().Id, Guid.Empty));
    }

    [Fact]
    public async Task Child_Create_Rejects_Other_Tenants_Project()
    {
        await using var db = Db();
        var projectId = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = projectId,
            TenantId = TenantB,
            ProjectCode = "B",
            ProjectName = "B",
            Status = "InProgress",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var projects = new ProjectRepository(db);
        var tasks = new ProjectTaskService(new ProjectTaskRepository(db), projects);

        var id = await tasks.CreateAsync(new CreateProjectTaskDto
        {
            ProjectId = projectId,
            TaskName = "Leak"
        }, TenantA, UserA);
        Assert.Equal(Guid.Empty, id);
    }

    [Fact]
    public async Task Child_Other_Tenant_Cannot_Read_Complete_Or_Delete()
    {
        await using var db = Db();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = projectId,
            TenantId = TenantB,
            ProjectCode = "B",
            ProjectName = "Secret",
            Status = "InProgress",
            CreatedAt = DateTime.UtcNow
        });
        db.ProjectTasks.Add(new ProjectTask
        {
            Id = taskId,
            TenantId = TenantB,
            ProjectId = projectId,
            TaskName = "Secret task",
            Status = "NotStarted",
            Priority = "Medium",
            CreatedAt = DateTime.UtcNow
        });
        db.ProjectMilestones.Add(new ProjectMilestone
        {
            Id = milestoneId,
            TenantId = TenantB,
            ProjectId = projectId,
            MilestoneName = "Secret MS",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        });
        db.ProjectResources.Add(new ProjectResource
        {
            Id = resourceId,
            TenantId = TenantB,
            ProjectId = projectId,
            ResourceName = "Secret res",
            ResourceType = "Employee",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var projects = new ProjectRepository(db);
        var tasks = new ProjectTaskService(new ProjectTaskRepository(db), projects);
        var milestones = new ProjectMilestoneService(new ProjectMilestoneRepository(db), projects);
        var resources = new ProjectResourceService(new ProjectResourceRepository(db), projects);

        Assert.Null(await tasks.GetByIdAsync(taskId, TenantA));
        Assert.Empty(await tasks.GetAllAsync(TenantA));
        Assert.False(await tasks.CompleteAsync(taskId, TenantA, UserA));
        Assert.False(await tasks.DeleteAsync(taskId, TenantA));

        Assert.Null(await milestones.GetByIdAsync(milestoneId, TenantA));
        Assert.False(await milestones.CompleteAsync(milestoneId, TenantA, UserA));
        Assert.False(await milestones.DeleteAsync(milestoneId, TenantA));

        Assert.Null(await resources.GetByIdAsync(resourceId, TenantA));
        Assert.False(await resources.DeleteAsync(resourceId, TenantA));

        Assert.Empty(await tasks.GetAllAsync(Guid.Empty));
        Assert.Empty(await milestones.GetAllAsync(Guid.Empty));
        Assert.Empty(await resources.GetAllAsync(Guid.Empty));
    }

    [Fact]
    public async Task Update_Ignores_Status_Start_Transitions()
    {
        await using var db = Db();
        var svc = new ProjectService(new ProjectRepository(db));
        var created = await svc.CreateAsync(new CreateProjectDto
        {
            ProjectCode = "P-W",
            ProjectName = "Workflow",
            Priority = "Medium",
            Status = "Completed"
        }, TenantA, UserA);

        Assert.Equal("Planning", (await svc.GetByIdAsync(created.ProjectId, TenantA))!.Status);

        Assert.True(await svc.UpdateAsync(new UpdateProjectDto
        {
            Id = created.ProjectId,
            ProjectCode = "P-W",
            ProjectName = "Workflow",
            Priority = "High",
            Status = "Completed"
        }, TenantA, UserA));
        Assert.Equal("Planning", (await svc.GetByIdAsync(created.ProjectId, TenantA))!.Status);

        Assert.True(await svc.StartAsync(created.ProjectId, TenantA, UserA));
        Assert.Equal("In Progress", (await svc.GetByIdAsync(created.ProjectId, TenantA))!.Status);
    }
}

