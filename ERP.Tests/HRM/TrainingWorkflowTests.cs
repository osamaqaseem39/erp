using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class TrainingWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid EmpA = Guid.NewGuid();
    private static readonly Guid EmpB = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static TrainingService Svc(ERPDbContext db) => new(new TrainingRepository(db));

    private static async Task SeedEmployeesAsync(ERPDbContext db)
    {
        db.Employees.AddRange(
            new Employee
            {
                Id = EmpA,
                TenantId = Tenant,
                EmployeeCode = "E-TA",
                FirstName = "Train",
                LastName = "Alpha",
                Status = "Active",
                IsActive = true
            },
            new Employee
            {
                Id = EmpB,
                TenantId = Tenant,
                EmployeeCode = "E-TB",
                FirstName = "Train",
                LastName = "Beta",
                Status = "Active",
                IsActive = true
            });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_Enroll_Complete_Capacity_And_TenantIsolation()
    {
        await using var db = Db();
        await SeedEmployeesAsync(db);
        var svc = Svc(db);

        var trainingId = await svc.CreateAsync(new CreateTrainingDto
        {
            Title = "Safety 101",
            Description = "Intro",
            TrainingType = "Compliance",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(2),
            MaxParticipants = 1,
            Status = "Scheduled",
            IsActive = true
        }, Tenant, User);

        var created = await svc.GetByIdAsync(trainingId, Tenant);
        Assert.NotNull(created);
        Assert.Equal("Scheduled", created!.Status);
        Assert.Null(await svc.GetByIdAsync(trainingId, OtherTenant));

        Assert.True(await svc.EnrollEmployeeAsync(trainingId, EmpA, Tenant, User));
        created = await svc.GetByIdAsync(trainingId, Tenant);
        Assert.Equal("In Progress", created!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.EnrollEmployeeAsync(trainingId, EmpA, Tenant, User));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.EnrollEmployeeAsync(trainingId, EmpB, Tenant, User));

        var forEmp = (await svc.GetByEmployeeAsync(EmpA, Tenant)).ToList();
        Assert.Single(forEmp);
        Assert.Equal(trainingId, forEmp[0].Id);

        Assert.True(await svc.CompleteTrainingAsync(trainingId, EmpA, 95m, Tenant, User));
        created = await svc.GetByIdAsync(trainingId, Tenant);
        Assert.Equal("Completed", created!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CompleteTrainingAsync(trainingId, EmpA, 90m, Tenant, User));

        Assert.True(await svc.DeleteAsync(trainingId, Tenant));
        Assert.Null(await svc.GetByIdAsync(trainingId, Tenant));
    }

    [Fact]
    public async Task Complete_Waits_Until_All_Enrolled_Are_Done()
    {
        await using var db = Db();
        await SeedEmployeesAsync(db);
        var svc = Svc(db);

        var trainingId = await svc.CreateAsync(new CreateTrainingDto
        {
            Title = "Soft Skills",
            TrainingType = "Soft Skills",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(5),
            MaxParticipants = 5,
            IsActive = true
        }, Tenant, User);

        Assert.True(await svc.EnrollEmployeeAsync(trainingId, EmpA, Tenant, User));
        Assert.True(await svc.EnrollEmployeeAsync(trainingId, EmpB, Tenant, User));

        Assert.True(await svc.CompleteTrainingAsync(trainingId, EmpA, 80m, Tenant, User));
        var mid = await svc.GetByIdAsync(trainingId, Tenant);
        Assert.Equal("In Progress", mid!.Status);

        Assert.True(await svc.CompleteTrainingAsync(trainingId, EmpB, 88m, Tenant, User));
        var done = await svc.GetByIdAsync(trainingId, Tenant);
        Assert.Equal("Completed", done!.Status);
    }
}
