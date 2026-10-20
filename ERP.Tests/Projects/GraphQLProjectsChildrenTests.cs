using ERP.Api.GraphQL.Projects;
using ERP.Application.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Infrastructure.Storage;
using ERP.Shared.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Projects;

public class GraphQLProjectsChildrenTests
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
            ProjectCode = "P-GQL",
            ProjectName = "GraphQL Demo",
            Status = "InProgress",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public void Surface_ExposesMilestoneResourceDocumentParity()
    {
        var queries = typeof(ProjectsQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProjectsQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(ProjectsMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProjectsMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetProjectDocument", queries);
        Assert.Contains("GetProjectDocuments", queries);

        Assert.Contains("UpdateProjectMilestone", mutations);
        Assert.Contains("DeleteProjectMilestone", mutations);
        Assert.Contains("UpdateProjectResource", mutations);
        Assert.Contains("AddProjectDocument", mutations);
        Assert.Contains("UpdateProjectDocument", mutations);
        Assert.Contains("DeleteProjectDocument", mutations);
        Assert.Contains("UploadProjectDocument", mutations);
    }

    [Fact]
    public async Task GraphQL_Milestone_Update_Delete_And_Resource_Update()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db);
        var projects = new ProjectRepository(db);
        var milestones = new ProjectMilestoneService(new ProjectMilestoneRepository(db), projects);
        var resources = new ProjectResourceService(new ProjectResourceRepository(db), projects);
        var mutations = new ProjectsMutations();
        var queries = new ProjectsQueries();

        var ms = await mutations.AddProjectMilestone(milestones, new CreateProjectMilestoneDto
        {
            TenantId = Tenant,
            ProjectId = projectId,
            MilestoneName = "Beta",
            TargetDate = DateTime.UtcNow.Date.AddDays(7)
        }, Tenant, User);
        Assert.False(string.IsNullOrWhiteSpace(ms.Status));
        Assert.NotEqual("Completed", ms.Status);

        var updated = await mutations.UpdateProjectMilestone(milestones, new UpdateProjectMilestoneDto
        {
            Id = ms.Id,
            MilestoneName = "Beta Updated"
        }, Tenant, User);
        Assert.Equal("Beta Updated", updated.MilestoneName);

        var listed = (await queries.GetProjectMilestones(milestones, Tenant, projectId: projectId)).ToList();
        Assert.Contains(listed, m => m.Id == ms.Id);

        Assert.True(await mutations.DeleteProjectMilestone(milestones, ms.Id, Tenant));
        Assert.Null(await queries.GetProjectMilestone(milestones, ms.Id, Tenant));

        var res = await mutations.AddProjectResource(resources, new CreateProjectResourceDto
        {
            TenantId = Tenant,
            ProjectId = projectId,
            ResourceId = Guid.NewGuid(),
            ResourceType = "Employee",
            Role = "Dev",
            AllocatedHours = 20,
            HourlyRate = 40
        }, Tenant, User);
        var hoursUpdated = await mutations.UpdateProjectResource(resources, new UpdateProjectResourceDto
        {
            Id = res.Id,
            AllocatedHours = 30
        }, Tenant, User);
        Assert.Equal(30m, hoursUpdated.AllocatedHours);
    }

    [Fact]
    public async Task GraphQL_Document_Create_List_Delete()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db);
        var root = Path.Combine(Path.GetTempPath(), "erp-gql-docs", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var docs = new ProjectDocumentService(
                new ProjectDocumentRepository(db),
                new ProjectRepository(db),
                new LocalProjectDocumentStorage(root));
            var mutations = new ProjectsMutations();
            var queries = new ProjectsQueries();

            var created = await mutations.AddProjectDocument(docs, new CreateProjectDocumentDto
            {
                TenantId = Tenant,
                CreatedBy = User.ToString(),
                ProjectId = projectId,
                DocumentName = "Spec",
                DocumentType = "Specification",
                FileName = "spec.txt",
                ContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("hello"))
            });
            Assert.NotEqual(Guid.Empty, created.Id);

            var byProject = (await queries.GetProjectDocuments(docs, Tenant, projectId: projectId)).ToList();
            Assert.Contains(byProject, d => d.Id == created.Id);

            Assert.True(await mutations.DeleteProjectDocument(docs, created.Id, Tenant));
            Assert.Null(await queries.GetProjectDocument(docs, created.Id, Tenant));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
