using ERP.Application.Common;
using ERP.Application.Interfaces;
using ERP.Application.Interfaces.Sales;
using ERP.Domain.CRM;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.DTOs.Sales;

namespace ERP.Application.CRM;

public interface ISalesmanService
{
    Task<SalesmanDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesmanDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSalesmanDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSalesmanDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ICustomerGroupService
{
    Task<CustomerGroupDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CustomerGroupDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateCustomerGroupDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateCustomerGroupDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPriceListService
{
    Task<PriceListDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PriceListDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreatePriceListDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdatePriceListDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ISalesTerritoryService
{
    Task<SalesTerritoryDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesTerritoryDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateSalesTerritoryDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateSalesTerritoryDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IReturnOrderService
{
    Task<ReturnOrderDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<ReturnOrderDto>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CreateReturnOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> UpdateAsync(UpdateReturnOrderDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId);
    Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId);
}

public class SalesmanService : ISalesmanService
{
    private readonly ISalesmanRepository _repository;
    public SalesmanService(ISalesmanRepository repository) => _repository = repository;

    public async Task<SalesmanDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<SalesmanDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<SalesmanDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateSalesmanDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.FullName)) return Guid.Empty;
        var entity = new Salesman
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SalesmanCode = string.IsNullOrWhiteSpace(dto.SalesmanCode)
                ? $"SM-{Guid.NewGuid():N}"[..12].ToUpperInvariant()
                : dto.SalesmanCode.Trim(),
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            Cnic = dto.Cnic,
            Remarks = dto.Remarks,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim(),
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateSalesmanDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (dto.SalesmanCode != null) e.SalesmanCode = dto.SalesmanCode;
        if (dto.FullName != null) e.FullName = dto.FullName;
        if (dto.Phone != null) e.Phone = dto.Phone;
        if (dto.Email != null) e.Email = dto.Email;
        if (dto.Address != null) e.Address = dto.Address;
        if (dto.Cnic != null) e.Cnic = dto.Cnic;
        if (dto.Remarks != null) e.Remarks = dto.Remarks;
        if (dto.Status != null) e.Status = dto.Status;
        e.UpdatedBy = userId.ToString();
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repository.DeleteAsync(id, tenantId);

    private static SalesmanDto Map(Salesman e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        SalesmanCode = e.SalesmanCode,
        FullName = e.FullName,
        Phone = e.Phone,
        Email = e.Email,
        Address = e.Address,
        Cnic = e.Cnic,
        Remarks = e.Remarks,
        Status = e.Status,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

public class CustomerGroupService : ICustomerGroupService
{
    private readonly ICustomerGroupRepository _repository;
    public CustomerGroupService(ICustomerGroupRepository repository) => _repository = repository;

    public async Task<CustomerGroupDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<CustomerGroupDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<CustomerGroupDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateCustomerGroupDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.GroupName)) return Guid.Empty;
        return await _repository.CreateAsync(new CustomerGroup
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            GroupName = dto.GroupName.Trim(),
            Description = dto.Description,
            DiscountPercentage = dto.DiscountPercentage,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<bool> UpdateAsync(UpdateCustomerGroupDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (dto.GroupName != null) e.GroupName = dto.GroupName;
        e.Description = dto.Description;
        if (dto.DiscountPercentage.HasValue) e.DiscountPercentage = dto.DiscountPercentage.Value;
        e.UpdatedBy = userId.ToString();
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repository.DeleteAsync(id, tenantId);

    private static CustomerGroupDto Map(CustomerGroup e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        GroupName = e.GroupName,
        Description = e.Description,
        DiscountPercentage = e.DiscountPercentage,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}

public class PriceListService : IPriceListService
{
    private readonly IPriceListRepository _repository;
    public PriceListService(IPriceListRepository repository) => _repository = repository;

    public async Task<PriceListDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<PriceListDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PriceListDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreatePriceListDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.PriceListName)) return Guid.Empty;
        return await _repository.CreateAsync(new PriceList
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PriceListName = dto.PriceListName.Trim(),
            Description = dto.Description,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsDefault = dto.IsDefault,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<bool> UpdateAsync(UpdatePriceListDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (dto.PriceListName != null) e.PriceListName = dto.PriceListName;
        e.Description = dto.Description;
        if (dto.ValidFrom.HasValue) e.ValidFrom = dto.ValidFrom.Value;
        e.ValidTo = dto.ValidTo;
        if (dto.IsDefault.HasValue) e.IsDefault = dto.IsDefault.Value;
        e.UpdatedBy = userId.ToString();
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repository.DeleteAsync(id, tenantId);

    private static PriceListDto Map(PriceList e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        PriceListName = e.PriceListName,
        Description = e.Description,
        ValidFrom = e.ValidFrom,
        ValidTo = e.ValidTo,
        IsDefault = e.IsDefault,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };
}

public class SalesTerritoryService : ISalesTerritoryService
{
    private readonly ISalesTerritoryRepository _repository;
    public SalesTerritoryService(ISalesTerritoryRepository repository) => _repository = repository;

    public async Task<SalesTerritoryDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<SalesTerritoryDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<SalesTerritoryDto>();
        return (await _repository.GetAllAsync(tenantId)).Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateSalesTerritoryDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(dto.TerritoryName)) return Guid.Empty;
        return await _repository.CreateAsync(new SalesTerritory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TerritoryName = dto.TerritoryName.Trim(),
            Description = dto.Description,
            ManagerId = dto.ManagerId,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<bool> UpdateAsync(UpdateSalesTerritoryDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (e == null) return false;
        if (dto.TerritoryName != null) e.TerritoryName = dto.TerritoryName;
        e.Description = dto.Description;
        e.ManagerId = dto.ManagerId;
        e.UpdatedBy = userId.ToString();
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
        => tenantId != Guid.Empty && await _repository.DeleteAsync(id, tenantId);

    private static SalesTerritoryDto Map(SalesTerritory e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        TerritoryName = e.TerritoryName,
        Description = e.Description,
        ManagerId = e.ManagerId,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt
    };
}

public class ReturnOrderService : IReturnOrderService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pending"] = new[] { "Approved", "Cancelled" },
            ["Approved"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

    private readonly IReturnOrderRepository _repository;
    private readonly IStockService _stock;
    private readonly ISalesCreditNoteService _creditNotes;

    public ReturnOrderService(
        IReturnOrderRepository repository,
        IStockService stock,
        ISalesCreditNoteService creditNotes)
    {
        _repository = repository;
        _stock = stock;
        _creditNotes = creditNotes;
    }

    public async Task<ReturnOrderDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var e = await _repository.GetByIdAsync(id);
        if (e == null || e.TenantId != tenantId) return null;
        return Map(e);
    }

    public async Task<IEnumerable<ReturnOrderDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<ReturnOrderDto>();
        return (await _repository.GetAllAsync())
            .Where(e => e.TenantId == tenantId)
            .Select(Map);
    }

    public async Task<Guid> CreateAsync(CreateReturnOrderDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty || dto.CustomerId == Guid.Empty) return Guid.Empty;
        var lines = (dto.Lines ?? []).Where(l => l.ItemId != Guid.Empty && l.Quantity > 0).ToList();
        var total = lines.Count > 0
            ? Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice), 2)
            : dto.TotalAmount;
        var createdBy = userId.ToString();

        var entity = new ReturnOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReturnNumber = string.IsNullOrWhiteSpace(dto.ReturnNumber)
                ? CrmLineCalculator.GenerateNumber("RT")
                : dto.ReturnNumber.Trim(),
            ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow.Date : dto.ReturnDate.Date,
            CustomerId = dto.CustomerId,
            SalesOrderId = dto.SalesOrderId,
            SalesInvoiceId = dto.SalesInvoiceId,
            WarehouseId = dto.WarehouseId,
            Reason = dto.Reason ?? string.Empty,
            Status = "Pending",
            TotalAmount = total,
            Notes = dto.Notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in lines)
        {
            entity.Lines.Add(new ReturnOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReturnOrderId = entity.Id,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = Math.Round(line.Quantity * line.UnitPrice, 2),
                Reason = line.Reason,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
        }

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateReturnOrderDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(dto.Id);
        if (e == null || e.TenantId != tenantId) return false;
        if (!string.Equals(e.Status, "Pending", StringComparison.OrdinalIgnoreCase)) return false;

        if (dto.Reason != null) e.Reason = dto.Reason;
        if (dto.Notes != null) e.Notes = dto.Notes;
        if (dto.WarehouseId.HasValue) e.WarehouseId = dto.WarehouseId;
        if (dto.SalesInvoiceId.HasValue) e.SalesInvoiceId = dto.SalesInvoiceId;
        if (dto.TotalAmount.HasValue && (dto.Lines == null || dto.Lines.Count == 0))
            e.TotalAmount = dto.TotalAmount.Value;

        if (dto.Lines != null)
        {
            e.Lines.Clear();
            foreach (var line in dto.Lines.Where(l => l.ItemId != Guid.Empty && l.Quantity > 0))
            {
                e.Lines.Add(new ReturnOrderLine
                {
                    Id = Guid.NewGuid(),
                    TenantId = e.TenantId,
                    ReturnOrderId = e.Id,
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = Math.Round(line.Quantity * line.UnitPrice, 2),
                    Reason = line.Reason,
                    CreatedAt = DateTime.UtcNow
                });
            }
            e.TotalAmount = Math.Round(e.Lines.Sum(l => l.LineTotal), 2);
        }

        e.UpdatedBy = userId.ToString();
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(id);
        if (e == null || e.TenantId != tenantId) return false;
        if (!string.Equals(e.Status, "Pending", StringComparison.OrdinalIgnoreCase)) return false;
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> ApproveAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, "Approved");

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
        => await TransitionAsync(id, tenantId, "Cancelled");

    public async Task<bool> CompleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(id);
        if (e == null || e.TenantId != tenantId) return false;
        if (!DocumentWorkflow.CanTransition(e.Status, "Completed", Transitions)) return false;
        if (e.TotalAmount <= 0 && e.Lines.Count == 0) return false;

        if (!e.CreditNoteId.HasValue)
        {
            // Monetary return → SalesCreditNote against a posted SI (AR/GL/stock via CN post)
            if (!e.SalesInvoiceId.HasValue || e.SalesInvoiceId == Guid.Empty)
                return false;

            var lineIndex = 1;
            var cnId = await _creditNotes.CreateAsync(new CreateSalesCreditNoteDto
            {
                CreditNoteNumber = $"CN-{e.ReturnNumber}",
                CreditNoteDate = e.ReturnDate,
                CustomerId = e.CustomerId,
                OriginalSalesInvoiceId = e.SalesInvoiceId.Value,
                ReturnOrderId = e.Id,
                WarehouseId = e.WarehouseId,
                Reason = e.Reason,
                Notes = string.IsNullOrWhiteSpace(e.Notes) ? $"ReturnOrder:{e.Id}" : e.Notes,
                Lines = e.Lines.Count > 0
                    ? e.Lines.Select(l => new CreateSalesCreditNoteLineDto
                    {
                        ItemId = l.ItemId,
                        Description = string.IsNullOrWhiteSpace(l.Reason) ? e.Reason : l.Reason,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        TaxRate = 0,
                        LineNumber = lineIndex++
                    }).ToList()
                    : [
                        new CreateSalesCreditNoteLineDto
                        {
                            Description = e.Reason,
                            Quantity = 1,
                            UnitPrice = e.TotalAmount,
                            TaxRate = 0,
                            LineNumber = 1
                        }
                    ]
            }, e.TenantId, userId);

            if (cnId == Guid.Empty) return false;
            if (!await _creditNotes.PostAsync(cnId, e.TenantId, userId))
                return false;

            e.CreditNoteId = cnId;
        }
        else if (e.WarehouseId.HasValue && e.WarehouseId != Guid.Empty && e.Lines.Count > 0)
        {
            // Already linked CN (posted elsewhere): stock-only if warehouse set and CN did not receive
            var cn = await _creditNotes.GetByIdAsync(e.CreditNoteId.Value, e.TenantId);
            if (cn == null || !cn.StockReceived)
            {
                var stockLines = e.Lines.Select(l => (l.ItemId, l.Quantity));
                var received = await _stock.ReceiveStockAsync(
                    e.TenantId, e.WarehouseId.Value, stockLines, userId.ToString());
                if (!received) return false;
            }
        }

        e.Status = "Completed";
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(e);
    }

    private async Task<bool> TransitionAsync(Guid id, Guid tenantId, string toStatus)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _repository.GetByIdAsync(id);
        if (e == null || e.TenantId != tenantId) return false;
        if (!DocumentWorkflow.CanTransition(e.Status, toStatus, Transitions)) return false;
        e.Status = toStatus;
        e.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(e);
    }

    private static ReturnOrderDto Map(ReturnOrder e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        ReturnNumber = e.ReturnNumber,
        ReturnDate = e.ReturnDate,
        CustomerId = e.CustomerId,
        CustomerName = e.Customer?.CompanyName,
        SalesOrderId = e.SalesOrderId,
        SalesInvoiceId = e.SalesInvoiceId,
        WarehouseId = e.WarehouseId,
        CreditNoteId = e.CreditNoteId,
        Reason = e.Reason,
        Status = e.Status,
        TotalAmount = e.TotalAmount,
        Notes = e.Notes,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        Lines = e.Lines?.Select(l => new ReturnOrderLineDto
        {
            Id = l.Id,
            ReturnOrderId = l.ReturnOrderId,
            ItemId = l.ItemId,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal,
            Reason = l.Reason
        }).ToList() ?? []
    };
}
