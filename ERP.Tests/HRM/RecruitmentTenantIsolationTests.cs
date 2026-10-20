using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class RecruitmentTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static RecruitmentService Svc(ERPDbContext db) => new(
        new JobOpeningRepositoryAdapter(db),
        new JobApplicationRepositoryAdapter(db),
        new CandidateRepositoryAdapter(db),
        new InterviewRepositoryAdapter(db));

    [Fact]
    public async Task Opening_And_Candidate_FailClosed_EmptyTenant_And_Isolate()
    {
        await using var db = Db();
        var svc = Svc(db);

        var openingId = await svc.CreateJobOpeningAsync(new CreateJobOpeningDto
        {
            Title = "Engineer"
        }, TenantA, User);
        var candidateId = await svc.CreateCandidateAsync(new CreateCandidateDto
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com"
        }, TenantA, User);

        Assert.NotNull(await svc.GetJobOpeningByIdAsync(openingId, TenantA));
        Assert.Null(await svc.GetJobOpeningByIdAsync(openingId, TenantB));
        Assert.Null(await svc.GetJobOpeningByIdAsync(openingId, Guid.Empty));
        Assert.Empty(await svc.GetAllJobOpeningsAsync(Guid.Empty));

        Assert.NotNull(await svc.GetCandidateByIdAsync(candidateId, TenantA));
        Assert.Null(await svc.GetCandidateByIdAsync(candidateId, TenantB));
        Assert.Null(await svc.GetCandidateByIdAsync(candidateId, Guid.Empty));
        Assert.Empty(await svc.GetAllCandidatesAsync(Guid.Empty));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateJobOpeningAsync(new CreateJobOpeningDto { Title = "X" }, Guid.Empty, User));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateCandidateAsync(new CreateCandidateDto
            {
                FirstName = "X",
                LastName = "Y",
                Email = "x@y.com"
            }, Guid.Empty, User));
    }

    [Fact]
    public async Task Application_CrossTenant_Blocked_And_EmptyTenant_Rejected()
    {
        await using var db = Db();
        var svc = Svc(db);

        var openingId = await svc.CreateJobOpeningAsync(new CreateJobOpeningDto
        {
            Title = "Designer"
        }, TenantA, User);
        var candidateId = await svc.CreateCandidateAsync(new CreateCandidateDto
        {
            FirstName = "Grace",
            LastName = "Hopper",
            Email = "grace@example.com"
        }, TenantA, User);

        var appId = await svc.CreateJobApplicationAsync(new CreateJobApplicationDto
        {
            JobOpeningId = openingId,
            CandidateId = candidateId
        }, TenantA, User);

        Assert.NotNull(await svc.GetJobApplicationByIdAsync(appId, TenantA));
        Assert.Null(await svc.GetJobApplicationByIdAsync(appId, TenantB));
        Assert.Empty(await svc.GetAllJobApplicationsAsync(Guid.Empty));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateJobApplicationAsync(new CreateJobApplicationDto
            {
                JobOpeningId = openingId,
                CandidateId = candidateId
            }, Guid.Empty, User));

        // Cross-tenant apply: opening not visible
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateJobApplicationAsync(new CreateJobApplicationDto
            {
                JobOpeningId = openingId,
                CandidateId = candidateId
            }, TenantB, User));
    }

    [Fact]
    public async Task Adapter_ForTenant_Empty_Returns_No_Rows()
    {
        await using var db = Db();
        db.JobOpenings.Add(new JobOpening
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            Title = "Ops",
            Status = "Open",
            IsActive = true
        });
        db.Candidates.Add(new Candidate
        {
            Id = Guid.NewGuid(),
            TenantId = TenantB,
            FirstName = "B",
            LastName = "Tenant",
            Email = "b@example.com",
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var openings = new JobOpeningRepositoryAdapter(db);
        var candidates = new CandidateRepositoryAdapter(db);

        Assert.Empty(await openings.GetAllAsync(Guid.Empty));
        Assert.Empty(await candidates.GetAllAsync(Guid.Empty));
        Assert.Single(await openings.GetAllAsync(TenantA));
        Assert.Empty(await openings.GetAllAsync(TenantB));
        Assert.Single(await candidates.GetAllAsync(TenantB));
        Assert.Empty(await candidates.GetAllAsync(TenantA));
    }
}
