using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using IRFQDetailRepository = ERP.Infrastructure.Interfaces.Procurement.IRFQDetailRepository;
using IRFQVendorResponseRepository = ERP.Infrastructure.Interfaces.Procurement.IRFQVendorResponseRepository;

namespace ERP.Application.Procurement;

public class RequestForQuotationService : IRequestForQuotationService
{
    private readonly IRequestForQuotationRepository _repository;
    private readonly IRFQDetailRepository _details;
    private readonly IRFQVendorResponseRepository _responses;

    public RequestForQuotationService(
        IRequestForQuotationRepository repository,
        IRFQDetailRepository details,
        IRFQVendorResponseRepository responses)
    {
        _repository = repository;
        _details = details;
        _responses = responses;
    }

    public async Task<RequestForQuotationDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity == null ? null : Map(entity, includeDetails: true, includeResponses: true);
    }

    public async Task<IEnumerable<RequestForQuotationDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(e => Map(e, false, false));

    public async Task<IEnumerable<RequestForQuotationDto>> GetByStatusAsync(string status, Guid tenantId) =>
        (await _repository.GetByStatusAsync(status, tenantId)).Select(e => Map(e, false, false));

    public async Task<Guid> CreateAsync(CreateRequestForQuotationDto dto, Guid tenantId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        if (dto.Details == null || dto.Details.Count == 0)
            throw new ArgumentException("RFQ requires at least one line.");
        if (dto.Details.Any(d => d.ItemId == Guid.Empty || d.Quantity <= 0))
            throw new ArgumentException("Each line needs ItemId and Quantity > 0.");

        var id = Guid.NewGuid();
        var entity = new RequestForQuotation
        {
            Id = id,
            TenantId = tenantId,
            PRId = dto.PRId,
            RequestorId = userId,
            RFQNumber = string.IsNullOrWhiteSpace(dto.RFQNumber)
                ? $"RFQ-{DateTime.UtcNow:yyyyMMdd}-{id.ToString("N")[..6]}"
                : dto.RFQNumber.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "Medium" : dto.Priority.Trim(),
            EstimatedValue = dto.EstimatedValue ?? 0,
            Terms = dto.Terms,
            Notes = dto.Notes,
            IssuedDate = dto.IssueDate?.ToUniversalTime() ?? DateTime.UtcNow,
            DueDate = dto.DueDate?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(14),
            Status = "Draft",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.CreateAsync(entity);
        await SaveDetailsAsync(id, tenantId, userId, dto.Details);
        return id;
    }

    public async Task<bool> UpdateAsync(UpdateRequestForQuotationDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft RFQs can be updated.");

        if (!string.IsNullOrWhiteSpace(dto.Title)) entity.Title = dto.Title.Trim();
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.IssueDate.HasValue) entity.IssuedDate = dto.IssueDate.Value.ToUniversalTime();
        if (dto.DueDate.HasValue) entity.DueDate = dto.DueDate.Value.ToUniversalTime();
        if (!string.IsNullOrWhiteSpace(dto.Priority)) entity.Priority = dto.Priority.Trim();
        if (dto.EstimatedValue.HasValue) entity.EstimatedValue = dto.EstimatedValue.Value;
        if (dto.Terms != null) entity.Terms = dto.Terms;
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (!string.IsNullOrWhiteSpace(dto.RFQNumber)) entity.RFQNumber = dto.RFQNumber.Trim();
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        if (dto.Details != null)
        {
            if (dto.Details.Count == 0)
                throw new ArgumentException("RFQ requires at least one line.");
            await _details.DeleteByRfqIdAsync(entity.Id, tenantId);
            await SaveDetailsAsync(entity.Id, tenantId, userId, dto.Details);
        }

        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only Draft RFQs can be deleted.");
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> SendAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        if (entity.Details == null || entity.Details.Count == 0)
            throw new InvalidOperationException("Cannot send an RFQ with no lines.");

        entity.Status = "Open";
        entity.IssuedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Awarded", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.Status = "Cancelled";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Open", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(entity.Status, "Awarded", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.Status = "Closed";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<Guid> SubmitVendorResponseAsync(CreateRFQVendorResponseDto dto, Guid tenantId, Guid userId)
    {
        if (dto.RFQId == Guid.Empty || dto.VendorId == Guid.Empty)
            throw new ArgumentException("RFQId and VendorId are required.");

        var rfq = await _repository.GetByIdAsync(dto.RFQId, tenantId)
            ?? throw new KeyNotFoundException("RFQ not found.");
        if (!string.Equals(rfq.Status, "Open", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Vendor responses are only accepted while the RFQ is Open.");

        var existing = (await _responses.GetByRfqIdAsync(dto.RFQId, tenantId))
            .FirstOrDefault(r => r.VendorId == dto.VendorId
                                 && !string.Equals(r.Status, "Rejected", StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            throw new InvalidOperationException("This vendor already submitted a response for this RFQ.");

        var id = Guid.NewGuid();
        var entity = new RFQVendorResponse
        {
            Id = id,
            TenantId = tenantId,
            RFQId = dto.RFQId,
            VendorId = dto.VendorId,
            ResponseDate = dto.ResponseDate?.ToUniversalTime() ?? DateTime.UtcNow,
            TotalQuotedAmount = dto.TotalAmount ?? 0,
            Status = "Submitted",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _responses.CreateAsync(entity);
        return id;
    }

    public async Task<bool> AwardAsync(Guid rfqId, Guid vendorResponseId, Guid tenantId, Guid userId)
    {
        var rfq = await _repository.GetByIdAsync(rfqId, tenantId);
        if (rfq == null) return false;
        if (!string.Equals(rfq.Status, "Open", StringComparison.OrdinalIgnoreCase))
            return false;

        var winner = await _responses.GetByIdAsync(vendorResponseId, tenantId);
        if (winner == null || winner.RFQId != rfqId) return false;

        var all = (await _responses.GetByRfqIdAsync(rfqId, tenantId)).ToList();
        foreach (var r in all)
        {
            // Get tracked entity for update
            var tracked = await _responses.GetByIdAsync(r.Id, tenantId);
            if (tracked == null) continue;
            tracked.Status = tracked.Id == vendorResponseId ? "Accepted" : "Rejected";
            tracked.UpdatedBy = userId.ToString();
            tracked.UpdatedAt = DateTime.UtcNow;
            await _responses.UpdateAsync(tracked);
        }

        rfq.Status = "Awarded";
        rfq.EstimatedValue = winner.TotalQuotedAmount > 0 ? winner.TotalQuotedAmount : rfq.EstimatedValue;
        rfq.UpdatedBy = userId.ToString();
        rfq.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(rfq);
    }

    private async Task SaveDetailsAsync(Guid rfqId, Guid tenantId, Guid userId, IEnumerable<CreateRFQDetailDto> lines)
    {
        foreach (var line in lines)
        {
            await _details.CreateAsync(new RFQDetail
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RFQId = rfqId,
                ItemId = line.ItemId,
                QuantityRequested = line.Quantity,
                Specifications = line.Specifications,
                Remarks = line.Notes ?? string.Empty,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
    }

    private static RequestForQuotationDto Map(RequestForQuotation e, bool includeDetails, bool includeResponses)
    {
        List<RFQDetailDto>? details = null;
        if (includeDetails && e.Details != null)
        {
            details = e.Details.Select(d => new RFQDetailDto
            {
                Id = d.Id,
                RFQId = d.RFQId,
                ItemId = d.ItemId,
                ItemName = d.Item?.ItemName,
                ItemCode = d.Item?.ItemCode,
                Quantity = d.QuantityRequested,
                Specifications = d.Specifications,
                Notes = d.Remarks
            }).ToList();
        }

        List<RFQVendorResponseDto>? responses = null;
        if (includeResponses && e.VendorResponses != null)
        {
            responses = e.VendorResponses.Select(r => new RFQVendorResponseDto
            {
                Id = r.Id,
                RFQId = r.RFQId,
                VendorId = r.VendorId,
                ResponseDate = r.ResponseDate,
                TotalAmount = r.TotalQuotedAmount,
                Status = r.Status
            }).ToList();
        }

        return new RequestForQuotationDto
        {
            Id = e.Id,
            TenantId = e.TenantId,
            RFQNumber = e.RFQNumber,
            Title = e.Title,
            Description = e.Description,
            IssueDate = e.IssuedDate,
            DueDate = e.DueDate,
            Status = e.Status,
            Priority = e.Priority,
            EstimatedValue = e.EstimatedValue,
            Terms = e.Terms,
            Notes = e.Notes,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            CreatedBy = e.CreatedBy,
            UpdatedBy = e.UpdatedBy,
            IsActive = e.IsActive,
            RFQDetails = details,
            VendorResponses = responses
        };
    }
}
