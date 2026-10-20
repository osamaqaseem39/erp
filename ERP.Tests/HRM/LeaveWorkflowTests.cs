using ERP.Application.HRM;
using ERP.Domain.HRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Adapters;
using ERP.Infrastructure.Repositories.HRM;
using ERP.Shared.DTOs.HRM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.HRM;

public class LeaveWorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid OtherTenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();
    private static readonly Guid EmployeeId = Guid.NewGuid();
    private static readonly Guid LeaveTypeId = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static LeaveService Svc(ERPDbContext db) =>
        new(new LeaveRequestRepositoryAdapter(new LeaveRequestRepository(db)));

    private static async Task SeedAsync(ERPDbContext db)
    {
        db.LeaveTypes.Add(new LeaveType
        {
            Id = LeaveTypeId,
            TenantId = Tenant,
            Name = "Annual",
            Code = "ANNUAL",
            IsActive = true
        });
        db.Employees.Add(new Employee
        {
            Id = EmployeeId,
            TenantId = Tenant,
            EmployeeCode = "E-1",
            FirstName = "Ada",
            LastName = "Lovelace",
            WorkEmail = "ada@example.com",
            DateHired = new DateTime(2025, 1, 1),
            Status = "Active",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private static async Task<LeaveRequestDto> CreatePendingAsync(LeaveService svc, string? createdBy = null)
    {
        return await svc.AddLeaveRequestAsync(new CreateLeaveRequestDto
        {
            TenantId = Tenant,
            EmployeeId = EmployeeId,
            LeaveTypeId = LeaveTypeId,
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 4, 3),
            DaysRequested = 3,
            Reason = "Vacation",
            AppliedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        });
    }

    [Fact]
    public async Task Approve_Reject_Cancel_Only_From_Pending()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);

        var created = await CreatePendingAsync(svc);
        Assert.Equal("Pending", created.Status);
        var createdId = created.Id!.Value;

        var approved = await svc.ApproveLeaveAsync(
            new LeaveApprovalDto { LeaveRequestId = createdId, Notes = "OK" }, Tenant, Approver);
        Assert.Equal("Approved", approved.Status);
        Assert.Equal(Approver, approved.ApprovedBy);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.RejectLeaveAsync(new RejectLeaveRequestDto { LeaveRequestId = createdId, Reason = "late" }, Tenant, Approver));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CancelLeaveAsync(createdId, "nope", Tenant, Approver));

        var pending = await CreatePendingAsync(svc);
        var rejected = await svc.RejectLeaveAsync(
            new RejectLeaveRequestDto { LeaveRequestId = pending.Id!.Value, Reason = "Staffing" }, Tenant, Approver);
        Assert.Equal("Rejected", rejected.Status);
        Assert.Equal("Staffing", rejected.RejectionReason);

        var toCancel = await CreatePendingAsync(svc);
        var done = await svc.CancelLeaveAsync(toCancel.Id!.Value, "Changed plans", Tenant, Approver);
        Assert.Equal("Cancelled", done.Status);
    }

    [Fact]
    public async Task Tenant_FailClosed_And_Creator_Cannot_Approve()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);

        var created = await CreatePendingAsync(svc, Creator.ToString());
        var id = created.Id!.Value;

        Assert.Null(await svc.GetLeaveRequestByIdAsync(id, OtherTenant));
        Assert.Null(await svc.GetLeaveRequestByIdAsync(id, Guid.Empty));
        Assert.Empty(await svc.GetPendingLeaveRequestsAsync(Guid.Empty));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.ApproveLeaveAsync(new LeaveApprovalDto { LeaveRequestId = id }, Tenant, Creator));

        var approved = await svc.ApproveLeaveAsync(
            new LeaveApprovalDto { LeaveRequestId = id }, Tenant, Approver);
        Assert.Equal("Approved", approved.Status);
        Assert.False(await svc.DeleteLeaveRequestAsync(id, Tenant));
    }

    [Fact]
    public async Task Pending_Only_Editable_And_Repo_Isolates()
    {
        await using var db = Db();
        await SeedAsync(db);
        var svc = Svc(db);
        var repo = new LeaveRequestRepository(db);

        var created = await CreatePendingAsync(svc);
        var id = created.Id!.Value;

        Assert.NotNull(await repo.GetByIdAsync(id, Tenant));
        Assert.Null(await repo.GetByIdAsync(id, OtherTenant));
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));

        await svc.ApproveLeaveAsync(new LeaveApprovalDto { LeaveRequestId = id }, Tenant, Approver);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateLeaveRequestAsync(new UpdateLeaveRequestDto
            {
                Id = id,
                LeaveTypeId = LeaveTypeId,
                StartDate = new DateTime(2026, 4, 1),
                EndDate = new DateTime(2026, 4, 5),
                DaysRequested = 5,
                AppliedAt = DateTime.UtcNow
            }, Tenant));
    }
}
