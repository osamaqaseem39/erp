using ERP.Application.Interfaces.Finance;
using ERP.Application.Interfaces.Sales;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Projects;
using ERP.Shared.DTOs.Finance;
using ERP.Shared.DTOs.Projects;
using ERP.Shared.DTOs.Sales;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Projects;

public interface IProjectMilestoneBillingService
{
    Task<MilestoneBillingResultDto> CreateDraftInvoiceFromMilestoneAsync(
        Guid milestoneId, MilestoneBillingDto dto, Guid tenantId, Guid userId);
}

public class ProjectMilestoneBillingService : IProjectMilestoneBillingService
{
    private readonly ERPDbContext _db;
    private readonly IProjectMilestoneRepository _milestones;
    private readonly IInvoiceService? _financeInvoices;
    private readonly ISalesInvoiceService? _salesInvoices;

    public ProjectMilestoneBillingService(
        ERPDbContext db,
        IProjectMilestoneRepository milestones,
        IInvoiceService? financeInvoices = null,
        ISalesInvoiceService? salesInvoices = null)
    {
        _db = db;
        _milestones = milestones;
        _financeInvoices = financeInvoices;
        _salesInvoices = salesInvoices;
    }

    public async Task<MilestoneBillingResultDto> CreateDraftInvoiceFromMilestoneAsync(
        Guid milestoneId, MilestoneBillingDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) throw new ValidationException("Tenant is required.");

        var milestone = await _milestones.GetByIdAsync(milestoneId, tenantId)
            ?? throw new NotFoundException("Milestone not found.");

        if (milestone.SalesInvoiceId.HasValue || milestone.FinanceInvoiceId.HasValue)
            throw new InvalidOperationException("Milestone has already been billed.");

        var amount = dto.Amount > 0 ? dto.Amount : milestone.Amount;
        if (amount <= 0)
            throw new ValidationException("Amount must be positive (set dto.Amount or milestone.Amount).");

        if (!milestone.IsBillable && dto.Amount <= 0)
            throw new ValidationException("Milestone is not billable; pass an explicit Amount to override.");

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == milestone.ProjectId && p.TenantId == tenantId)
            ?? throw new NotFoundException("Project not found.");

        var invoiceDate = dto.InvoiceDate?.Date ?? DateTime.UtcNow.Date;
        var dueDate = dto.DueDate?.Date ?? invoiceDate.AddDays(30);
        var desc = dto.Description ?? $"Milestone: {milestone.MilestoneName}";

        if (_salesInvoices != null && project.ClientId.HasValue && project.ClientId != Guid.Empty)
        {
            var salesId = await _salesInvoices.CreateAsync(new CreateSalesInvoiceDto
            {
                CustomerId = project.ClientId.Value,
                InvoiceDate = invoiceDate,
                DueDate = dueDate,
                SubTotal = amount,
                TaxAmount = 0,
                TotalAmount = amount,
                Status = "Draft",
                Notes = $"ProjectMilestone:{milestone.Id} — {desc}",
                ProjectId = project.Id,
                ProjectMilestoneId = milestone.Id,
                Lines =
                [
                    new CreateSalesInvoiceLineDto
                    {
                        Description = desc,
                        Quantity = 1,
                        UnitPrice = amount
                    }
                ]
            }, tenantId, userId);
            if (salesId != Guid.Empty)
            {
                milestone.SalesInvoiceId = salesId;
                milestone.BilledAt = DateTime.UtcNow;
                milestone.Amount = amount;
                milestone.IsBillable = true;
                milestone.UpdatedAt = DateTime.UtcNow;
                milestone.UpdatedBy = userId.ToString();
                await _milestones.UpdateAsync(milestone);

                return new MilestoneBillingResultDto
                {
                    MilestoneId = milestoneId,
                    SalesInvoiceId = salesId,
                    Target = "sales"
                };
            }
        }

        if (_financeInvoices != null && project.ClientId.HasValue && project.ClientId != Guid.Empty)
        {
            var financeId = await _financeInvoices.AddInvoiceAsync(new InvoiceCreateDto
            {
                CustomerId = project.ClientId.Value,
                InvoiceDate = invoiceDate,
                DueDate = dueDate,
                SubTotal = amount,
                TaxAmount = 0,
                TotalAmount = amount,
                Status = "Draft",
                Notes = $"ProjectMilestone:{milestone.Id} — {desc}",
                ProjectId = project.Id,
                ProjectMilestoneId = milestone.Id
            }, tenantId, userId);
            if (financeId != Guid.Empty)
            {
                milestone.FinanceInvoiceId = financeId;
                milestone.BilledAt = DateTime.UtcNow;
                milestone.Amount = amount;
                milestone.IsBillable = true;
                milestone.UpdatedAt = DateTime.UtcNow;
                milestone.UpdatedBy = userId.ToString();
                await _milestones.UpdateAsync(milestone);

                return new MilestoneBillingResultDto
                {
                    MilestoneId = milestoneId,
                    FinanceInvoiceId = financeId,
                    Target = "finance"
                };
            }
        }

        throw new InvalidOperationException("No invoice service available or project has no client.");
    }
}
