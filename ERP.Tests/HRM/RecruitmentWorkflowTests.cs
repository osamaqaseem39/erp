using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class RecruitmentWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid InterviewerId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static RecruitmentService Svc(ERPDbContext db) => new(
        new JobOpeningRepositoryAdapter(db),
        new JobApplicationRepositoryAdapter(db),
        new CandidateRepositoryAdapter(db),
        new InterviewRepositoryAdapter(db));

    private static async Task SeedInterviewerAsync(ERPDbContext db)
    {
        db.Employees.Add(new Employee
        {
            Id = InterviewerId,
            TenantId = Tenant,
            EmployeeCode = "E-INT",
            FirstName = "Inter",
            LastName = "Viewer",
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Opening_Apply_Interview_Hire_Fills_Opening()
    {
        await using var db = Db();
        await SeedInterviewerAsync(db);
        var svc = Svc(db);

        var openingId = await svc.CreateJobOpeningAsync(new CreateJobOpeningDto
        {
            Title = "Backend Engineer",
            Description = "APIs",
            Department = "Engineering",
            Location = "Remote"
        }, Tenant, User);

        Assert.Equal("Open", (await svc.GetJobOpeningByIdAsync(openingId, Tenant))!.Status);
        Assert.Null(await svc.GetJobOpeningByIdAsync(openingId, OtherTenant));

        var candidateId = await svc.CreateCandidateAsync(new CreateCandidateDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com"
        }, Tenant, User);

        var appId = await svc.CreateJobApplicationAsync(new CreateJobApplicationDto
        {
            JobOpeningId = openingId,
            CandidateId = candidateId,
            CoverLetter = "Hello"
        }, Tenant, User);

        Assert.Equal("Applied", (await svc.GetJobApplicationByIdAsync(appId, Tenant))!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateJobApplicationAsync(new CreateJobApplicationDto
            {
                JobOpeningId = openingId,
                CandidateId = candidateId
            }, Tenant, User));

        Assert.True(await svc.ShortlistApplicationAsync(appId, Tenant, User));

        var interviewId = await svc.CreateInterviewAsync(new CreateInterviewDto
        {
            JobApplicationId = appId,
            InterviewerId = InterviewerId,
            InterviewType = "Video",
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            Duration = 45
        }, Tenant, User);

        Assert.True(await svc.CompleteInterviewAsync(interviewId, 5, "Strong", Tenant, User));
        Assert.Equal("Interviewed", (await svc.GetJobApplicationByIdAsync(appId, Tenant))!.Status);
        Assert.Equal("Completed", (await svc.GetInterviewByIdAsync(interviewId, Tenant))!.Status);
        Assert.Null(await svc.GetInterviewByIdAsync(interviewId, OtherTenant));

        Assert.True(await svc.HireApplicationAsync(appId, Tenant, User));
        Assert.Equal("Hired", (await svc.GetJobApplicationByIdAsync(appId, Tenant))!.Status);
        Assert.Equal("Hired", (await svc.GetCandidateByIdAsync(candidateId, Tenant))!.Status);
        Assert.Equal("Filled", (await svc.GetJobOpeningByIdAsync(openingId, Tenant))!.Status);
    }

    [Fact]
    public async Task Close_And_Reject_Block_Further_Apply()
    {
        await using var db = Db();
        var svc = Svc(db);

        var openingId = await svc.CreateJobOpeningAsync(new CreateJobOpeningDto
        {
            Title = "Analyst",
            Description = "Data"
        }, Tenant, User);

        var candidateId = await svc.CreateCandidateAsync(new CreateCandidateDto
        {
            FirstName = "Ben",
            LastName = "Bit",
            Email = "ben@example.com"
        }, Tenant, User);

        var appId = await svc.CreateJobApplicationAsync(new CreateJobApplicationDto
        {
            JobOpeningId = openingId,
            CandidateId = candidateId
        }, Tenant, User);

        Assert.True(await svc.RejectApplicationAsync(appId, Tenant, User));
        Assert.Equal("Rejected", (await svc.GetJobApplicationByIdAsync(appId, Tenant))!.Status);

        Assert.True(await svc.CloseJobOpeningAsync(openingId, Tenant, User));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateJobApplicationAsync(new CreateJobApplicationDto
            {
                JobOpeningId = openingId,
                CandidateId = candidateId
            }, Tenant, User));

        Assert.True(await svc.ReopenJobOpeningAsync(openingId, Tenant, User));
        Assert.Equal("Open", (await svc.GetJobOpeningByIdAsync(openingId, Tenant))!.Status);
    }
}
