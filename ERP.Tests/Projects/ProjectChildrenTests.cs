using ERP.Application.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Projects;

public class ProjectChildrenTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<Guid> SeedProjectAsync(ERPDbContext db)
    {
        var id = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = id,
            TenantId = Tenant,
            ProjectCode = "P-1",
            ProjectName = "Demo",
            Status = "InProgress",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Task_Create_Complete_And_SoftDelete()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db);
        var projects = new ProjectRepository(db);
        var tasks = new ProjectTaskService(new ProjectTaskRepository(db), projects);

        var id = await tasks.CreateAsync(new CreateProjectTaskDto
        {
            TenantId = Tenant,
            CreatedBy = User.ToString(),
            ProjectId = projectId,
            TaskName = "Build API",
            Priority = "High",
            EstimatedHours = 8
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var loaded = await tasks.GetByIdAsync(id, Tenant);
        Assert.Equal("NotStarted", loaded!.Status);
        Assert.Equal("High", loaded.Priority);

        Assert.True(await tasks.CompleteAsync(id, Tenant, User));
        loaded = await tasks.GetByIdAsync(id, Tenant);
        Assert.Equal("Completed", loaded!.Status);
        Assert.Equal(100m, loaded.Progress);
        Assert.NotNull(loaded.CompletedDate);

        Assert.False(await tasks.DeleteAsync(id, Tenant)); // completed cannot delete
        Assert.False(await tasks.UpdateAsync(new UpdateProjectTaskDto
        {
            Id = id,
            TaskName = "Nope"
        }, Tenant, User));
    }

    [Fact]
    public async Task Milestone_Create_Complete_And_Overdue_Status()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db);
        var projects = new ProjectRepository(db);
        var milestones = new ProjectMilestoneService(new ProjectMilestoneRepository(db), projects);

        var id = await milestones.CreateAsync(new CreateProjectMilestoneDto
        {
            TenantId = Tenant,
            ProjectId = projectId,
            MilestoneName = "Alpha",
            TargetDate = DateTime.UtcNow.Date.AddDays(-3)
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var open = await milestones.GetByIdAsync(id, Tenant);
        Assert.Equal("Overdue", open!.Status);

        Assert.True(await milestones.CompleteAsync(id, Tenant, User));
        var done = await milestones.GetByIdAsync(id, Tenant);
        Assert.Equal("Completed", done!.Status);
        Assert.NotNull(done.CompletedDate);

        var list = (await milestones.GetByProjectAsync(projectId, Tenant)).ToList();
        Assert.Contains(list, m => m.Id == id);
    }

    [Fact]
    public async Task Resource_Create_Update_Hours()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db);
        var projects = new ProjectRepository(db);
        var resources = new ProjectResourceService(new ProjectResourceRepository(db), projects);
        var employeeId = Guid.NewGuid();

        var id = await resources.CreateAsync(new CreateProjectResourceDto
        {
            TenantId = Tenant,
            ProjectId = projectId,
            ResourceId = employeeId,
            ResourceType = "Employee",
            Role = "Developer",
            AllocatedHours = 40,
            HourlyRate = 50
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        Assert.True(await resources.UpdateAsync(new UpdateProjectResourceDto
        {
            Id = id,
            ActualHours = 12
        }, Tenant, User));

        var dto = await resources.GetByIdAsync(id, Tenant);
        Assert.Equal("Developer", dto!.Role);
        Assert.Equal(12, dto.ActualHours);
        Assert.Equal(employeeId, dto.ResourceId);
        Assert.Single(await resources.GetByProjectAsync(projectId, Tenant));
    }
}
