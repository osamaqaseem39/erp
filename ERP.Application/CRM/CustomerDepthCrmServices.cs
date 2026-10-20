using ERP.Application.Common;
using ERP.Domain.CRM;
using ERP.Infrastructure.Numbering;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public interface IPouchTypeService
{
    Task<PouchTypeDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PouchTypeDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePouchTypeDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePouchTypeDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IProductionProcessService
{
    Task<ProductionProcessDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ProductionProcessDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateProductionProcessDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateProductionProcessDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IFOHTypeService
{
    Task<FOHTypeDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<FOHTypeDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateFOHTypeDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateFOHTypeDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ISaleEnquiryService
{
    Task<SaleEnquiryDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SaleEnquiryDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSaleEnquiryDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSaleEnquiryDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
}

public interface ISalePreCostService
{
    Task<SalePreCostDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalePreCostDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSalePreCostDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSalePreCostDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId);
}

public class PouchTypeService : IPouchTypeService
{
    private readonly IPouchTypeRepository _repo;
    public PouchTypeService(IPouchTypeRepository repo) => _repo = repo;

    public async Task<PouchTypeDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<PouchTypeDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repo.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreatePouchTypeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return Guid.Empty;
        return await _repo.CreateAsync(new PouchType
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            CreatedBy = userId.ToString()
        });
    }

    public async Task<bool> UpdateAsync(UpdatePouchTypeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        e.Code = dto.Code.Trim();
        e.Name = dto.Name.Trim();
        e.Description = dto.Description;
        e.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId) =>
        tenantId != Guid.Empty && await _repo.SoftDeleteAsync(id, tenantId);

    private static PouchTypeDto Map(PouchType e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

public class ProductionProcessService : IProductionProcessService
{
    private readonly IProductionProcessRepository _repo;
    public ProductionProcessService(IProductionProcessRepository repo) => _repo = repo;

    public async Task<ProductionProcessDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ProductionProcessDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repo.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateProductionProcessDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return Guid.Empty;
        return await _repo.CreateAsync(new ProductionProcess
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            CreatedBy = userId.ToString()
        });
    }

    public async Task<bool> UpdateAsync(UpdateProductionProcessDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        e.Code = dto.Code.Trim();
        e.Name = dto.Name.Trim();
        e.Description = dto.Description;
        e.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId) =>
        tenantId != Guid.Empty && await _repo.SoftDeleteAsync(id, tenantId);

    private static ProductionProcessDto Map(ProductionProcess e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

public class FOHTypeService : IFOHTypeService
{
    private readonly IFOHTypeRepository _repo;
    public FOHTypeService(IFOHTypeRepository repo) => _repo = repo;

    public async Task<FOHTypeDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<FOHTypeDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repo.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateFOHTypeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return Guid.Empty;
        return await _repo.CreateAsync(new FOHType
        {
            TenantId = tenantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Basis = dto.Basis,
            Rate = dto.Rate,
            Description = dto.Description,
            CreatedBy = userId.ToString()
        });
    }

    public async Task<bool> UpdateAsync(UpdateFOHTypeDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        e.Code = dto.Code.Trim();
        e.Name = dto.Name.Trim();
        e.Basis = dto.Basis;
        e.Rate = dto.Rate;
        e.Description = dto.Description;
        e.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId) =>
        tenantId != Guid.Empty && await _repo.SoftDeleteAsync(id, tenantId);

    private static FOHTypeDto Map(FOHType e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Code = e.Code,
        Name = e.Name,
        Basis = e.Basis,
        Rate = e.Rate,
        Description = e.Description,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

public class SaleEnquiryService : ISaleEnquiryService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Approved", "Rejected" },
            ["Approved"] = Array.Empty<string>(),
            ["Rejected"] = Array.Empty<string>()
        };

    private readonly ISaleEnquiryRepository _repo;
    private readonly IDocumentNumberService _numbers;

    public SaleEnquiryService(ISaleEnquiryRepository repo, IDocumentNumberService numbers)
    {
        _repo = repo;
        _numbers = numbers;
    }

    public async Task<SaleEnquiryDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repo.GetByIdAsync(id, tenantId, includeLines: true);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<SaleEnquiryDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var list = await _repo.GetAllAsync(tenantId);
        return list.Select(MapHeader);
    }

    public async Task<Guid> CreateAsync(CreateSaleEnquiryDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = new SaleEnquiry
        {
            TenantId = tenantId,
            EnquiryNumber = await _numbers.NextAsync("crm.saleEnquiry"),
            CustomerId = dto.CustomerId,
            EnquiryDate = dto.EnquiryDate?.Date ?? DateTime.UtcNow.Date,
            Status = "Draft",
            ReferenceNo = dto.ReferenceNo,
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            Lines = (dto.Lines ?? []).Select(l => ToLine(l, tenantId)).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateSaleEnquiryDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.CustomerId = dto.CustomerId;
        entity.EnquiryDate = dto.EnquiryDate?.Date ?? entity.EnquiryDate;
        entity.ReferenceNo = dto.ReferenceNo;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        if (!await _repo.UpdateAsync(entity)) return false;
        await _repo.ReplaceLinesAsync(entity.Id, tenantId, (dto.Lines ?? []).Select(l => ToLine(l, tenantId)));
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repo.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Approved", Transitions)) return false;
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId)) return false;
        entity.Status = "Approved";
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Rejected", Transitions)) return false;
        entity.Status = "Rejected";
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    private static SaleEnquiryLine ToLine(CreateSaleEnquiryLineDto dto, Guid tenantId) => new()
    {
        TenantId = tenantId,
        ItemId = dto.ItemId,
        ItemDescription = dto.ItemDescription ?? string.Empty,
        Quantity = dto.Quantity,
        Size = dto.Size,
        MaterialType = dto.MaterialType,
        PET = dto.PET,
        MPET = dto.MPET,
        LaminationMicron = dto.LaminationMicron,
        CutOffLength = dto.CutOffLength,
        FolderSize = dto.FolderSize,
        Ups = dto.Ups,
        PouchTypeId = dto.PouchTypeId,
        ProcessId = dto.ProcessId,
        FOHTypeId = dto.FOHTypeId,
        Notes = dto.Notes
    };

    private static SaleEnquiryDto Map(SaleEnquiry e)
    {
        var dto = MapHeader(e);
        dto.Lines = e.Lines.Select(l => new SaleEnquiryLineDto
        {
            Id = l.Id,
            ItemId = l.ItemId,
            ItemDescription = l.ItemDescription,
            Quantity = l.Quantity,
            Size = l.Size,
            MaterialType = l.MaterialType,
            PET = l.PET,
            MPET = l.MPET,
            LaminationMicron = l.LaminationMicron,
            CutOffLength = l.CutOffLength,
            FolderSize = l.FolderSize,
            Ups = l.Ups,
            PouchTypeId = l.PouchTypeId,
            ProcessId = l.ProcessId,
            FOHTypeId = l.FOHTypeId,
            Notes = l.Notes
        }).ToList();
        return dto;
    }

    private static SaleEnquiryDto MapHeader(SaleEnquiry e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        EnquiryNumber = e.EnquiryNumber,
        CustomerId = e.CustomerId,
        EnquiryDate = e.EnquiryDate,
        Status = e.Status,
        ReferenceNo = e.ReferenceNo,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

public class SalePreCostService : ISalePreCostService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Draft"] = new[] { "Approved", "Rejected" },
            ["Approved"] = Array.Empty<string>(),
            ["Rejected"] = Array.Empty<string>()
        };

    private readonly ISalePreCostRepository _repo;
    private readonly IDocumentNumberService _numbers;

    public SalePreCostService(ISalePreCostRepository repo, IDocumentNumberService numbers)
    {
        _repo = repo;
        _numbers = numbers;
    }

    public async Task<SalePreCostDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repo.GetByIdAsync(id, tenantId, includeLines: true);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<SalePreCostDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repo.GetAllAsync(tenantId)).Select(MapHeader);
    }

    public async Task<Guid> CreateAsync(CreateSalePreCostDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = new SalePreCost
        {
            TenantId = tenantId,
            PreCostNumber = await _numbers.NextAsync("crm.salePreCost"),
            SaleEnquiryId = dto.SaleEnquiryId,
            QuoteId = dto.QuoteId,
            CustomerId = dto.CustomerId,
            PreCostDate = dto.PreCostDate?.Date ?? DateTime.UtcNow.Date,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedBy = userId.ToString(),
            Lines = (dto.Lines ?? []).Select(l => ToLine(l, tenantId)).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateSalePreCostDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        entity.SaleEnquiryId = dto.SaleEnquiryId;
        entity.QuoteId = dto.QuoteId;
        entity.CustomerId = dto.CustomerId;
        entity.PreCostDate = dto.PreCostDate?.Date ?? entity.PreCostDate;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId.ToString();
        if (!await _repo.UpdateAsync(entity)) return false;
        await _repo.ReplaceLinesAsync(entity.Id, tenantId, (dto.Lines ?? []).Select(l => ToLine(l, tenantId)));
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(DocumentWorkflow.Normalize(entity.Status), "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repo.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Approved", Transitions)) return false;
        if (DocumentWorkflow.IsSameActor(entity.CreatedBy, userId)) return false;
        entity.Status = "Approved";
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> RejectAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || tenantId == Guid.Empty) return false;
        if (!DocumentWorkflow.CanTransition(entity.Status, "Rejected", Transitions)) return false;
        entity.Status = "Rejected";
        entity.UpdatedBy = userId.ToString();
        return await _repo.UpdateAsync(entity);
    }

    private static SalePreCostLine ToLine(CreateSalePreCostLineDto dto, Guid tenantId)
    {
        var baseCost = dto.MaterialCost + dto.ProcessCost + dto.FohCost;
        var unitRate = dto.UnitRate > 0
            ? dto.UnitRate
            : Math.Round(baseCost * (1 + dto.MarginPercent / 100m), 4);
        return new SalePreCostLine
        {
            TenantId = tenantId,
            ItemId = dto.ItemId,
            ItemDescription = dto.ItemDescription ?? string.Empty,
            Quantity = dto.Quantity,
            MaterialCost = dto.MaterialCost,
            ProcessCost = dto.ProcessCost,
            FohCost = dto.FohCost,
            MarginPercent = dto.MarginPercent,
            UnitRate = unitRate,
            Size = dto.Size,
            MaterialType = dto.MaterialType,
            PET = dto.PET,
            MPET = dto.MPET,
            LaminationMicron = dto.LaminationMicron,
            CutOffLength = dto.CutOffLength,
            FolderSize = dto.FolderSize,
            Ups = dto.Ups,
            PouchTypeId = dto.PouchTypeId,
            ProcessId = dto.ProcessId,
            FOHTypeId = dto.FOHTypeId,
            Notes = dto.Notes
        };
    }

    private static SalePreCostDto Map(SalePreCost e)
    {
        var dto = MapHeader(e);
        dto.Lines = e.Lines.Select(l => new SalePreCostLineDto
        {
            Id = l.Id,
            ItemId = l.ItemId,
            ItemDescription = l.ItemDescription,
            Quantity = l.Quantity,
            MaterialCost = l.MaterialCost,
            ProcessCost = l.ProcessCost,
            FohCost = l.FohCost,
            MarginPercent = l.MarginPercent,
            UnitRate = l.UnitRate,
            Size = l.Size,
            MaterialType = l.MaterialType,
            PET = l.PET,
            MPET = l.MPET,
            LaminationMicron = l.LaminationMicron,
            CutOffLength = l.CutOffLength,
            FolderSize = l.FolderSize,
            Ups = l.Ups,
            PouchTypeId = l.PouchTypeId,
            ProcessId = l.ProcessId,
            FOHTypeId = l.FOHTypeId,
            Notes = l.Notes
        }).ToList();
        return dto;
    }

    private static SalePreCostDto MapHeader(SalePreCost e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        PreCostNumber = e.PreCostNumber,
        SaleEnquiryId = e.SaleEnquiryId,
        QuoteId = e.QuoteId,
        CustomerId = e.CustomerId,
        PreCostDate = e.PreCostDate,
        Status = e.Status,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
