using ERP.Application.Projects;
using ERP.Domain.Finance;
using ERP.Domain.HRM;
using ERP.Domain.Projects;
using ERP.Domain.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Projects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Projects;

public class ProjectEntityDepthTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<(Guid ProjectId, Guid EmployeeId)> SeedProjectAsync(ERPDbContext db, Guid? clientId = null)
    {
        var employeeId = Guid.NewGuid();
        db.Employees.Add(new Employee
        {
            Id = employeeId,
            TenantId = Tenant,
            FirstName = "Pat",
            LastName = "Lee",
            EmployeeCode = "E-1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        var projectId = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = projectId,
            TenantId = Tenant,
            ProjectCode = "P-DEPTH",
            ProjectName = "Depth",
            Status = "InProgress",
            ClientId = clientId,
            Budget = 10000m,
            ActualCost = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return (projectId, employeeId);
    }

    [Fact]
    public async Task Member_Create_List_And_TenantFailClosed()
    {
        await using var db = Db();
        var (projectId, employeeId) = await SeedProjectAsync(db);
        var projects = new ProjectRepository(db);
        var members = new ProjectMemberService(new ProjectMemberRepository(db), projects);

        var id = await members.CreateAsync(new CreateProjectMemberDto
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            Role = "Lead"
        }, Tenant, User);
        Assert.NotEqual(Guid.Empty, id);

        var list = (await members.GetByProjectAsync(projectId, Tenant)).ToList();
        Assert.Single(list);
        Assert.Equal("Lead", list[0].Role);

        Assert.Empty(await members.GetByProjectAsync(projectId, Guid.NewGuid()));
        Assert.Null(await members.GetByIdAsync(id, Guid.Empty));
    }

    [Fact]
    public async Task Timesheet_BillableLines_And_Approve_UpdatesActualCost()
    {
        await using var db = Db();
        var (projectId, employeeId) = await SeedProjectAsync(db);
        db.ProjectResources.Add(new ProjectResource
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ProjectId = projectId,
            EmployeeId = employeeId,
            ResourceType = "Employee",
            ResourceName = "Pat",
            HourlyRate = 50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var projects = new ProjectRepository(db);
        var costs = new ProjectCostService(db, projects);
        var timesheets = new ProjectTimesheetService(db, projects, costs);

        var created = await timesheets.CreateAsync(new CreateProjectTimesheetDto
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            PeriodStart = DateTime.UtcNow.Date.AddDays(-7),
            PeriodEnd = DateTime.UtcNow.Date,
            Lines =
            [
                new CreateProjectTimesheetLineDto
                {
                    LineNumber = 1,
                    WorkDate = DateTime.UtcNow.Date.AddDays(-1),
                    Hours = 4,
                    IsBillable = true
                }
            ]
        }, Tenant, User);

        Assert.True(created.Lines[0].IsBillable);
        Assert.Equal(50m, created.Lines[0].HourlyRate);

        await timesheets.SubmitAsync(created.Id, Tenant, User);
        await timesheets.ApproveAsync(created.Id, Tenant, User);

        var project = await db.Projects.AsNoTracking().FirstAsync(p => p.Id == projectId);
        Assert.Equal(200m, project.ActualCost);

        var summary = await costs.GetCostSummaryAsync(projectId, Tenant);
        Assert.NotNull(summary);
        Assert.Equal(200m, summary!.LaborCost);
        Assert.Equal(200m, summary.ActualCost);
    }

    [Fact]
    public async Task CostSummary_IncludesApprovedExpenses()
    {
        await using var db = Db();
        var (projectId, _) = await SeedProjectAsync(db);
        db.ExpenseClaims.Add(new ExpenseClaim
        {
            Id = Guid.NewGuid(),
            TenantId = Tenant,
            ClaimNumber = "EC-1",
            ProjectId = projectId,
            ClaimDate = DateTime.UtcNow.Date,
            Status = "Approved",
            TotalAmount = 125.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var costs = new ProjectCostService(db, new ProjectRepository(db));
        var summary = await costs.RecalculateActualCostAsync(projectId, Tenant);
        Assert.NotNull(summary);
        Assert.Equal(125.50m, summary!.ExpenseCost);
        Assert.Equal(125.50m, summary.ActualCost);
    }

    [Fact]
    public async Task MilestoneBilling_SetsInvoiceFks_And_BlocksDoubleBill()
    {
        await using var db = Db();
        var clientId = Guid.NewGuid();
        var (projectId, _) = await SeedProjectAsync(db, clientId);

        var milestoneId = Guid.NewGuid();
        db.ProjectMilestones.Add(new ProjectMilestone
        {
            Id = milestoneId,
            TenantId = Tenant,
            ProjectId = projectId,
            MilestoneName = "Phase 1",
            Status = "Completed",
            Amount = 1500m,
            IsBillable = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var sales = new StubSalesInvoiceService(db);
        var billing = new ProjectMilestoneBillingService(
            db,
            new ProjectMilestoneRepository(db),
            financeInvoices: null,
            salesInvoices: sales);

        var result = await billing.CreateDraftInvoiceFromMilestoneAsync(
            milestoneId,
            new MilestoneBillingDto { Amount = 0 },
            Tenant,
            User);

        Assert.Equal("sales", result.Target);
        Assert.NotNull(result.SalesInvoiceId);

        var milestone = await db.ProjectMilestones.AsNoTracking().FirstAsync(m => m.Id == milestoneId);
        Assert.Equal(result.SalesInvoiceId, milestone.SalesInvoiceId);
        Assert.NotNull(milestone.BilledAt);

        var invoice = await db.SalesInvoices.AsNoTracking().FirstAsync(i => i.Id == result.SalesInvoiceId);
        Assert.Equal(projectId, invoice.ProjectId);
        Assert.Equal(milestoneId, invoice.ProjectMilestoneId);
        Assert.Equal(1500m, invoice.TotalAmount);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            billing.CreateDraftInvoiceFromMilestoneAsync(milestoneId, new MilestoneBillingDto { Amount = 10 }, Tenant, User));
    }

    private sealed class StubSalesInvoiceService : ERP.Application.Interfaces.Sales.ISalesInvoiceService
    {
        private readonly ERPDbContext _db;
        public StubSalesInvoiceService(ERPDbContext db) => _db = db;

        public async Task<Guid> CreateAsync(ERP.Shared.DTOs.Sales.CreateSalesInvoiceDto dto, Guid tenantId, Guid userId)
        {
            var id = Guid.NewGuid();
            _db.SalesInvoices.Add(new SalesInvoice
            {
                Id = id,
                TenantId = tenantId,
                CustomerId = dto.CustomerId,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                SubTotal = dto.SubTotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                Status = "Draft",
                Notes = dto.Notes,
                ProjectId = dto.ProjectId,
                ProjectMilestoneId = dto.ProjectMilestoneId,
                InvoiceNumber = $"SI-{id:N}"[..16],
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            });
            await _db.SaveChangesAsync();
            return id;
        }

        public Task<bool> UpdateAsync(ERP.Shared.DTOs.Sales.UpdateSalesInvoiceDto dto, Guid tenantId, Guid userId) => Task.FromResult(false);
        public Task<bool> DeleteAsync(Guid id, Guid tenantId) => Task.FromResult(false);
        public Task<ERP.Shared.DTOs.Sales.SalesInvoiceDto?> GetByIdAsync(Guid id, Guid tenantId) => Task.FromResult<ERP.Shared.DTOs.Sales.SalesInvoiceDto?>(null);
        public Task<IEnumerable<ERP.Shared.DTOs.Sales.SalesInvoiceDto>> GetAllAsync(Guid tenantId) => Task.FromResult(Enumerable.Empty<ERP.Shared.DTOs.Sales.SalesInvoiceDto>());
        public Task<IEnumerable<ERP.Shared.DTOs.Sales.SalesInvoiceDto>> GetByCustomerAsync(Guid customerId, Guid tenantId) => Task.FromResult(Enumerable.Empty<ERP.Shared.DTOs.Sales.SalesInvoiceDto>());
        public Task<IEnumerable<ERP.Shared.DTOs.Sales.SalesInvoiceDto>> GetByStatusAsync(string status, Guid tenantId) => Task.FromResult(Enumerable.Empty<ERP.Shared.DTOs.Sales.SalesInvoiceDto>());
        public Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy) => Task.FromResult(false);
        public Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid tenantId, Guid voidedBy) => Task.FromResult(false);
        public Task<bool> UnpostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid userId) => Task.FromResult(false);
    }
}
