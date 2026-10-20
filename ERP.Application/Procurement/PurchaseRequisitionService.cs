using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;

namespace ERP.Application.Procurement;

public class PurchaseRequisitionService : IPurchaseRequisitionService
{
    private readonly IPurchaseRequisitionRepository _repository;
    private readonly IPurchaseRequisitionDetailRepository _detailRepository;

    public PurchaseRequisitionService(
        IPurchaseRequisitionRepository repository,
        IPurchaseRequisitionDetailRepository detailRepository)
    {
        _repository = repository;
        _detailRepository = detailRepository;
    }

    public async Task<PurchaseRequisitionDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return null;
        var details = (await _detailRepository.GetByRequisitionIdAsync(id, tenantId)).ToList();
        entity.Details = details;
        return MapToDto(entity, true);
    }

    public async Task<IEnumerable<PurchaseRequisitionDto>> GetAllAsync(Guid tenantId) =>
        (await _repository.GetAllAsync(tenantId)).Select(e => MapToDto(e, false));

    public async Task<Guid> CreateAsync(CreatePurchaseRequisitionDto dto, Guid tenantId, Guid userId)
    {
        var requester = dto.RequestedById == Guid.Empty ? userId : dto.RequestedById;
        var entity = new PurchaseRequisition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CompanyId = dto.CompanyId ?? Guid.Empty,
            RequestedByUserId = requester,
            RequestorId = requester,
            DepartmentId = dto.DepartmentId,
            RequestDate = dto.RequestDate ?? DateTime.UtcNow,
            RequiredByDate = dto.RequiredDate ?? DateTime.UtcNow.AddDays(14),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status,
            Remarks = dto.Remarks ?? dto.Notes ?? dto.Justification ?? string.Empty,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        ApplyTotals(entity, dto.Details);
        var id = await _repository.CreateAsync(entity);
        await SaveDetailsAsync(id, tenantId, userId, dto.Details);
        return id;
    }

    public async Task<bool> UpdateAsync(UpdatePurchaseRequisitionDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null || IsImmutable(entity.Status)) return false;

        if (dto.CompanyId.HasValue) entity.CompanyId = dto.CompanyId.Value;
        if (dto.RequestedById.HasValue)
        {
            entity.RequestedByUserId = dto.RequestedById.Value;
            entity.RequestorId = dto.RequestedById.Value;
        }
        if (dto.DepartmentId.HasValue) entity.DepartmentId = dto.DepartmentId;
        if (dto.RequestDate.HasValue) entity.RequestDate = dto.RequestDate.Value;
        if (dto.RequiredDate.HasValue) entity.RequiredByDate = dto.RequiredDate.Value;
        if (!string.IsNullOrWhiteSpace(dto.Status)) entity.Status = dto.Status;
        if (dto.Remarks != null || dto.Notes != null || dto.Justification != null)
            entity.Remarks = dto.Remarks ?? dto.Notes ?? dto.Justification ?? entity.Remarks;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;

        ApplyTotals(entity, dto.Details);
        await _detailRepository.DeleteByRequisitionIdAsync(entity.Id, tenantId);
        await SaveDetailsAsync(entity.Id, tenantId, userId, dto.Details);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null || IsImmutable(entity.Status)) return false;
        await _detailRepository.DeleteByRequisitionIdAsync(id, tenantId);
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<PurchaseRequisitionDto>> GetByRequesterAsync(Guid requesterId, Guid tenantId) =>
        (await _repository.GetByRequestorAsync(requesterId, tenantId)).Select(e => MapToDto(e, false));

    public async Task<IEnumerable<PurchaseRequisitionDto>> GetByStatusAsync(string status, Guid tenantId) =>
        (await _repository.GetByStatusAsync(status, tenantId)).Select(e => MapToDto(e, false));

    public async Task<IEnumerable<PurchaseRequisitionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId) =>
        (await _repository.GetByDateRangeAsync(startDate, endDate, tenantId)).Select(e => MapToDto(e, false));

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(entity.Status, "Submitted", StringComparison.OrdinalIgnoreCase))
            return false;
        entity.Status = "Approved";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null || string.Equals(entity.Status, "ConvertedToPO", StringComparison.OrdinalIgnoreCase))
            return false;
        entity.Status = "Rejected";
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task SaveDetailsAsync(Guid prId, Guid tenantId, Guid userId, IEnumerable<CreatePurchaseRequisitionDetailDto> details)
    {
        foreach (var line in details)
        {
            await _detailRepository.CreateAsync(new PurchaseRequisitionDetail
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PRId = prId,
                ItemId = line.ItemId,
                QuantityRequested = line.Quantity,
                UnitOfMeasureId = line.UnitOfMeasureId ?? Guid.Empty,
                EstimatedUnitPrice = line.EstimatedUnitPrice ?? 0,
                Remarks = line.Notes ?? line.Specifications ?? string.Empty,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static void ApplyTotals(PurchaseRequisition entity, IEnumerable<CreatePurchaseRequisitionDetailDto> details)
    {
        // Header has no TotalAmount field; totals live on DTO only.
        _ = entity;
        _ = details;
    }

    private static bool IsImmutable(string status) =>
        string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "ConvertedToPO", StringComparison.OrdinalIgnoreCase);

    private static PurchaseRequisitionDto MapToDto(PurchaseRequisition entity, bool includeDetails)
    {
        var details = includeDetails && entity.Details != null
            ? entity.Details.Select(d => new PurchaseRequisitionDetailDto
            {
                Id = d.Id,
                TenantId = d.TenantId,
                PRId = d.PRId,
                RequisitionId = d.PRId,
                ItemId = d.ItemId,
                Quantity = d.QuantityRequested,
                QuantityRequested = d.QuantityRequested,
                EstimatedUnitPrice = d.EstimatedUnitPrice,
                EstimatedTotalPrice = d.QuantityRequested * d.EstimatedUnitPrice,
                UnitOfMeasureId = d.UnitOfMeasureId,
                Remarks = d.Remarks,
                Notes = d.Remarks,
                CreatedAt = d.CreatedAt,
                CreatedBy = d.CreatedBy
            }).ToList()
            : null;

        var total = entity.Details?.Sum(d => d.QuantityRequested * d.EstimatedUnitPrice) ?? 0;

        return new PurchaseRequisitionDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            RequestedById = entity.RequestorId,
            RequestedByUserId = entity.RequestedByUserId,
            DepartmentId = entity.DepartmentId,
            RequestDate = entity.RequestDate,
            RequiredDate = entity.RequiredByDate,
            RequiredByDate = entity.RequiredByDate,
            TotalAmount = Math.Round(total, 2),
            Status = entity.Status,
            Remarks = entity.Remarks,
            Notes = entity.Remarks,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive,
            Details = details,
            RequisitionDetails = details
        };
    }
}
