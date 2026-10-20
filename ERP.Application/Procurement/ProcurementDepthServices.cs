using ERP.Application.Interfaces.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Procurement;

public class LandedCostVoucherService : ILandedCostVoucherService
{
    private readonly ILandedCostVoucherRepository _repo;
    private readonly ERPDbContext _db;

    public LandedCostVoucherService(ILandedCostVoucherRepository repo, ERPDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public async Task<LandedCostVoucherDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapVoucher(e);
    }

    public async Task<IEnumerable<LandedCostVoucherDto>> GetAllAsync(Guid tenantId)
        => (await _repo.GetAllAsync(tenantId)).Select(MapVoucher);

    public async Task<Guid> CreateAsync(CreateLandedCostVoucherDto dto, Guid tenantId, Guid userId)
    {
        if (dto.Lines.Count == 0) return Guid.Empty;
        var id = Guid.NewGuid();
        var total = dto.Lines.Sum(l => l.AllocatedAmount);
        var entity = new LandedCostVoucher
        {
            Id = id,
            TenantId = tenantId,
            VendorId = dto.VendorId,
            VoucherDate = dto.VoucherDate == default ? DateTime.UtcNow.Date : dto.VoucherDate,
            VoucherNumber = string.IsNullOrWhiteSpace(dto.VoucherNumber)
                ? $"LCV-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.VoucherNumber.Trim(),
            Description = dto.Description,
            TotalAmount = total,
            Status = "Draft",
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Lines = dto.Lines.Select(l => new LandedCostVoucherLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                LandedCostVoucherId = id,
                ItemId = l.ItemId,
                GrnDetailId = l.GrnDetailId,
                PurchaseInvoiceDetailId = l.PurchaseInvoiceDetailId,
                AllocatedAmount = l.AllocatedAmount,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            }).ToList()
        };
        return await _repo.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateLandedCostVoucherDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null || !string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return false;
        if (dto.Lines.Count == 0) return false;
        entity.VendorId = dto.VendorId;
        entity.VoucherDate = dto.VoucherDate == default ? entity.VoucherDate : dto.VoucherDate;
        entity.Description = dto.Description;
        entity.TotalAmount = dto.Lines.Sum(l => l.AllocatedAmount);
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Lines.Clear();
        foreach (var l in dto.Lines)
        {
            entity.Lines.Add(new LandedCostVoucherLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                LandedCostVoucherId = entity.Id,
                ItemId = l.ItemId,
                GrnDetailId = l.GrnDetailId,
                PurchaseInvoiceDetailId = l.PurchaseInvoiceDetailId,
                AllocatedAmount = l.AllocatedAmount,
                Notes = l.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            });
        }
        return await _repo.UpdateAsync(entity);
    }

    public async Task<bool> PostAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId);
        if (entity == null || !string.Equals(entity.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            return false;

        foreach (var line in entity.Lines.Where(l => l.ItemId.HasValue && l.AllocatedAmount > 0))
        {
            var itemId = line.ItemId!.Value;
            var grnQty = 0m;
            if (line.GrnDetailId.HasValue)
            {
                var grnLine = await _db.GoodsReceiptNoteDetails
                    .FirstOrDefaultAsync(g => g.Id == line.GrnDetailId && g.TenantId == tenantId);
                if (grnLine != null)
                {
                    grnQty = grnLine.QuantityReceived;
                    if (!line.ItemId.HasValue) itemId = grnLine.ItemId;
                }
            }

            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.TenantId == tenantId);
            if (item == null) continue;

            if (grnQty > 0)
            {
                var addPerUnit = line.AllocatedAmount / grnQty;
                item.CostPrice = Math.Round(item.CostPrice + addPerUnit, 4);
            }
            else
            {
                item.CostPrice = Math.Round(item.CostPrice + line.AllocatedAmount, 4);
            }

            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId.ToString();
        }

        entity.Status = "Posted";
        entity.PostedBy = userId.ToString();
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static LandedCostVoucherDto MapVoucher(LandedCostVoucher e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        VoucherNumber = e.VoucherNumber,
        VendorId = e.VendorId,
        VoucherDate = e.VoucherDate,
        Description = e.Description,
        TotalAmount = e.TotalAmount,
        Status = e.Status,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy,
        Lines = e.Lines.Select(l => new LandedCostVoucherLineDto
        {
            Id = l.Id,
            TenantId = l.TenantId,
            LandedCostVoucherId = l.LandedCostVoucherId,
            ItemId = l.ItemId,
            GrnDetailId = l.GrnDetailId,
            PurchaseInvoiceDetailId = l.PurchaseInvoiceDetailId,
            AllocatedAmount = l.AllocatedAmount,
            Notes = l.Notes,
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt,
            CreatedBy = l.CreatedBy
        }).ToList()
    };
}

public class VendorContractService : IVendorContractService
{
    private readonly IVendorContractRepository _repo;
    public VendorContractService(IVendorContractRepository repo) => _repo = repo;

    public async Task<VendorContractDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetByIdAsync(id, tenantId);
        return e == null ? null : MapContract(e);
    }

    public async Task<IEnumerable<VendorContractDto>> GetByVendorAsync(Guid vendorId, Guid tenantId)
        => (await _repo.GetByVendorAsync(vendorId, tenantId)).Select(MapContract);

    public async Task<VendorContractDto?> GetActiveAsync(Guid vendorId, Guid? itemId, Guid tenantId)
    {
        var list = await _repo.GetActiveForVendorItemAsync(vendorId, itemId, DateTime.UtcNow, tenantId);
        var match = list.FirstOrDefault();
        return match == null ? null : MapContract(match);
    }

    public async Task<VendorContractDto> CreateAsync(CreateVendorContractDto dto, Guid tenantId, Guid userId)
    {
        var entity = new VendorContract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorId = dto.VendorId,
            ItemId = dto.ItemId,
            UnitPrice = dto.UnitPrice,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            PaymentTermId = dto.PaymentTermId,
            ContractNumber = dto.ContractNumber,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        return MapContract(await _repo.CreateAsync(entity));
    }

    public async Task<VendorContractDto?> UpdateAsync(UpdateVendorContractDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return null;
        entity.VendorId = dto.VendorId;
        entity.ItemId = dto.ItemId;
        entity.UnitPrice = dto.UnitPrice;
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;
        entity.PaymentTermId = dto.PaymentTermId;
        entity.ContractNumber = dto.ContractNumber;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
        return MapContract(entity);
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repo.DeleteAsync(id, tenantId);

    private static VendorContractDto MapContract(VendorContract e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        VendorId = e.VendorId,
        ItemId = e.ItemId,
        UnitPrice = e.UnitPrice,
        ValidFrom = e.ValidFrom,
        ValidTo = e.ValidTo,
        PaymentTermId = e.PaymentTermId,
        ContractNumber = e.ContractNumber,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy
    };
}
