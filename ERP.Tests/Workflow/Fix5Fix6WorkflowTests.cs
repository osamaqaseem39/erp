using ERP.Application.Finance;
using ERP.Application.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Finance;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Workflow;

/// <summary>
/// ERP-FIX-5 / ERP-FIX-6: status free-set guards and same-actor approval blocks.
/// </summary>
public class Fix5Fix6WorkflowTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Creator = Guid.NewGuid();
    private static readonly Guid Approver = Guid.NewGuid();

    private static ERPDbContext Db() => new(
        new DbContextOptionsBuilder<ERPDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    // ────────────────────────────────────────────────────────────────
    // PurchaseRequisition — FIX-5 + FIX-6
    // ────────────────────────────────────────────────────────────────

    private static PurchaseRequisitionService PRService(ERPDbContext db) =>
        new(new PurchaseRequisitionRepository(db), new PurchaseRequisitionDetailRepository(db));

    [Fact]
    public async Task PurchaseRequisition_Create_ForcesDraft_IgnoresClientStatus()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto
        {
            Status = "Approved",  // should be ignored
            Details = []
        }, Tenant, Creator);

        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task PurchaseRequisition_Update_DoesNotFreeSetStatus()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto { Details = [] }, Tenant, Creator);

        // Attempt to push Status via Update — should be silently ignored
        var ok = await svc.UpdateAsync(new UpdatePurchaseRequisitionDto
        {
            Id = id,
            Status = "Approved",
            Details = []
        }, Tenant, Creator);

        Assert.True(ok); // update itself succeeds
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status); // but status unchanged
    }

    [Fact]
    public async Task PurchaseRequisition_Approve_SameActor_Rejected_FIX6()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto { Details = [] }, Tenant, Creator);

        // FIX-6: creator must not be able to approve their own PR
        Assert.False(await svc.ApproveAsync(id, Tenant, Creator));
        Assert.Equal("Draft", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task PurchaseRequisition_Approve_DifferentActor_Succeeds()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto { Details = [] }, Tenant, Creator);

        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task PurchaseRequisition_Reject_AfterApprove_IllegalTransition()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto { Details = [] }, Tenant, Creator);
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));

        // Approved is terminal — reject must fail
        Assert.False(await svc.RejectAsync(id, Tenant, Approver));
        Assert.Equal("Approved", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task PurchaseRequisition_Reject_FromDraft_Succeeds()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto { Details = [] }, Tenant, Creator);

        Assert.True(await svc.RejectAsync(id, Tenant, Approver));
        Assert.Equal("Rejected", (await svc.GetByIdAsync(id, Tenant))!.Status);
    }

    [Fact]
    public async Task PurchaseRequisition_Update_AfterApprove_Rejected()
    {
        await using var db = Db();
        var svc = PRService(db);

        var id = await svc.CreateAsync(new CreatePurchaseRequisitionDto { Details = [] }, Tenant, Creator);
        Assert.True(await svc.ApproveAsync(id, Tenant, Approver));

        // Approved is immutable
        var ok = await svc.UpdateAsync(new UpdatePurchaseRequisitionDto { Id = id, Details = [] }, Tenant, Approver);
        Assert.False(ok);
    }

    // ────────────────────────────────────────────────────────────────
    // InvoiceService (AR) — FIX-5: Update must NOT free-set Status
    // ────────────────────────────────────────────────────────────────

    private static InvoiceService InvService(ERPDbContext db) =>
        new(new ApplicationInvoiceRepositoryAdapter(db));

    [Fact]
    public async Task InvoiceService_Update_DoesNotFreeSetStatus()
    {
        await using var db = Db();
        var svc = InvService(db);

        var id = await svc.AddInvoiceAsync(new InvoiceCreateDto
        {
            InvoiceNumber = "AR-FIX5-001",
            InvoiceDate   = DateTime.UtcNow.Date,
            DueDate       = DateTime.UtcNow.Date.AddDays(30),
            TotalAmount   = 100
        }, Tenant, Creator);

        // Invoice starts Draft
        Assert.Equal("Draft", (await svc.GetInvoiceByIdAsync(id, Tenant))!.Status);

        // Attempt to push status via Update
        await svc.UpdateInvoiceAsync(new InvoiceUpdateDto
        {
            Id           = id,
            InvoiceNumber = "AR-FIX5-001",
            Status       = "Posted",   // must be ignored
            TotalAmount  = 100
        }, Tenant, Creator);

        Assert.Equal("Draft", (await svc.GetInvoiceByIdAsync(id, Tenant))!.Status);
    }
}
