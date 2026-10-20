using ERP.Application.Projects;
using ERP.Domain.Projects;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Infrastructure.Storage;
using ERP.Shared.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Projects;

public class ProjectDocumentTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static (ProjectDocumentService Svc, string Root) Svc(ERPDbContext db)
    {
        var root = Path.Combine(Path.GetTempPath(), "erp-doc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var storage = new LocalProjectDocumentStorage(root);
        var svc = new ProjectDocumentService(
            new ProjectDocumentRepository(db),
            new ProjectRepository(db),
            storage);
        return (svc, root);
    }

    private static async Task<Guid> SeedProjectAsync(ERPDbContext db, Guid tenantId, string code = "P-1")
    {
        var id = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = id,
            TenantId = tenantId,
            ProjectCode = code,
            ProjectName = "Demo",
            Status = "InProgress",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Create_With_Content_Download_And_SoftDelete()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db, TenantA);
        var (svc, root) = Svc(db);
        try
        {
            var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("hello-spec"));
            var id = await svc.CreateAsync(new CreateProjectDocumentDto
            {
                TenantId = TenantA,
                CreatedBy = User.ToString(),
                ProjectId = projectId,
                DocumentName = "Spec v1",
                DocumentType = "Specification",
                FileName = "spec.txt",
                ContentBase64 = payload
            });
            Assert.NotEqual(Guid.Empty, id);

            var dto = await svc.GetByIdAsync(id, TenantA);
            Assert.NotNull(dto);
            Assert.Equal("Specification", dto!.DocumentType);
            Assert.True(dto.FileSize > 0);
            Assert.False(string.IsNullOrWhiteSpace(dto.FilePath));

            var download = await svc.DownloadAsync(id, TenantA);
            Assert.NotNull(download);
            Assert.Equal("hello-spec", System.Text.Encoding.UTF8.GetString(download!.Content));
            Assert.Equal("spec.txt", download.FileName);

            Assert.True(await svc.DeleteAsync(id, TenantA));
            Assert.Null(await svc.GetByIdAsync(id, TenantA));
            Assert.Null(await svc.DownloadAsync(id, TenantA));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task Other_Tenant_Cannot_Access_Or_Attach_To_Foreign_Project()
    {
        await using var db = Db();
        var projectB = await SeedProjectAsync(db, TenantB, "B-1");
        var (svc, root) = Svc(db);
        try
        {
            Assert.Equal(Guid.Empty, await svc.CreateAsync(new CreateProjectDocumentDto
            {
                TenantId = TenantA,
                ProjectId = projectB,
                DocumentName = "Hijack",
                DocumentType = "Contract"
            }));

            var id = await svc.CreateAsync(new CreateProjectDocumentDto
            {
                TenantId = TenantB,
                ProjectId = projectB,
                DocumentName = "Secret",
                DocumentType = "Contract",
                ContentBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 })
            });
            Assert.NotEqual(Guid.Empty, id);

            Assert.Null(await svc.GetByIdAsync(id, TenantA));
            Assert.Null(await svc.DownloadAsync(id, TenantA));
            Assert.False(await svc.DeleteAsync(id, TenantA));
            Assert.Empty(await svc.GetByProjectAsync(projectB, TenantA));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task Upload_Replaces_File_And_Metadata_Only_Create_Has_No_Download()
    {
        await using var db = Db();
        var projectId = await SeedProjectAsync(db, TenantA);
        var (svc, root) = Svc(db);
        try
        {
            var id = await svc.CreateAsync(new CreateProjectDocumentDto
            {
                TenantId = TenantA,
                ProjectId = projectId,
                DocumentName = "Empty meta",
                DocumentType = "Report"
            });
            Assert.NotEqual(Guid.Empty, id);
            Assert.Null(await svc.DownloadAsync(id, TenantA));

            Assert.True(await svc.UploadAsync(id, TenantA, "report.pdf", new byte[] { 9, 9, 9 }, User.ToString()));
            var dl = await svc.DownloadAsync(id, TenantA);
            Assert.NotNull(dl);
            Assert.Equal(new byte[] { 9, 9, 9 }, dl!.Content);
            Assert.Equal("report.pdf", dl.FileName);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
