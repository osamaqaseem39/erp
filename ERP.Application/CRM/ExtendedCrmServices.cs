using ERP.Domain.CRM;
using ERP.Infrastructure.Repositories.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public interface ICustomerGroupService
{
    Task<CustomerGroupDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<CustomerGroupDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateCustomerGroupDto dto);
    Task<bool> UpdateAsync(UpdateCustomerGroupDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public interface IPriceListService
{
    Task<PriceListDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PriceListDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreatePriceListDto dto);
    Task<bool> UpdateAsync(UpdatePriceListDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public interface ISalesTerritoryService
{
    Task<SalesTerritoryDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SalesTerritoryDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateSalesTerritoryDto dto);
    Task<bool> UpdateAsync(UpdateSalesTerritoryDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public interface IReturnOrderService
{
    Task<ReturnOrderDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReturnOrderDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateReturnOrderDto dto);
    Task<bool> UpdateAsync(UpdateReturnOrderDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class CustomerGroupService : ICustomerGroupService
{
    private readonly ICustomerGroupRepository _repository;
    public CustomerGroupService(ICustomerGroupRepository repository) => _repository = repository;

    public async Task<CustomerGroupDto?> GetByIdAsync(Guid id) { var e = await _repository.GetByIdAsync(id); return e == null ? null : Map(e); }
    public async Task<IEnumerable<CustomerGroupDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map);
    public async Task<Guid> CreateAsync(CreateCustomerGroupDto dto) => await _repository.CreateAsync(new CustomerGroup { TenantId = dto.TenantId, GroupName = dto.GroupName, Description = dto.Description, DiscountPercentage = dto.DiscountPercentage, CreatedBy = dto.CreatedBy });
    public async Task<bool> UpdateAsync(UpdateCustomerGroupDto dto) { var e = await _repository.GetByIdAsync(dto.Id); if (e == null) return false; if (dto.GroupName != null) e.GroupName = dto.GroupName; e.Description = dto.Description; if (dto.DiscountPercentage.HasValue) e.DiscountPercentage = dto.DiscountPercentage.Value; e.UpdatedBy = dto.UpdatedBy; return await _repository.UpdateAsync(e); }
    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);
    private static CustomerGroupDto Map(CustomerGroup e) => new() { Id = e.Id, TenantId = e.TenantId, GroupName = e.GroupName, Description = e.Description, DiscountPercentage = e.DiscountPercentage, IsActive = e.IsActive, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt };
}

public class PriceListService : IPriceListService
{
    private readonly IPriceListRepository _repository;
    public PriceListService(IPriceListRepository repository) => _repository = repository;

    public async Task<PriceListDto?> GetByIdAsync(Guid id) { var e = await _repository.GetByIdAsync(id); return e == null ? null : Map(e); }
    public async Task<IEnumerable<PriceListDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map);
    public async Task<Guid> CreateAsync(CreatePriceListDto dto) => await _repository.CreateAsync(new PriceList { TenantId = dto.TenantId, PriceListName = dto.PriceListName, Description = dto.Description, ValidFrom = dto.ValidFrom, ValidTo = dto.ValidTo, IsDefault = dto.IsDefault, CreatedBy = dto.CreatedBy });
    public async Task<bool> UpdateAsync(UpdatePriceListDto dto) { var e = await _repository.GetByIdAsync(dto.Id); if (e == null) return false; if (dto.PriceListName != null) e.PriceListName = dto.PriceListName; e.Description = dto.Description; if (dto.ValidFrom.HasValue) e.ValidFrom = dto.ValidFrom.Value; e.ValidTo = dto.ValidTo; if (dto.IsDefault.HasValue) e.IsDefault = dto.IsDefault.Value; return await _repository.UpdateAsync(e); }
    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);
    private static PriceListDto Map(PriceList e) => new() { Id = e.Id, TenantId = e.TenantId, PriceListName = e.PriceListName, Description = e.Description, ValidFrom = e.ValidFrom, ValidTo = e.ValidTo, IsDefault = e.IsDefault, IsActive = e.IsActive, CreatedAt = e.CreatedAt };
}

public class SalesTerritoryService : ISalesTerritoryService
{
    private readonly ISalesTerritoryRepository _repository;
    public SalesTerritoryService(ISalesTerritoryRepository repository) => _repository = repository;

    public async Task<SalesTerritoryDto?> GetByIdAsync(Guid id) { var e = await _repository.GetByIdAsync(id); return e == null ? null : Map(e); }
    public async Task<IEnumerable<SalesTerritoryDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map);
    public async Task<Guid> CreateAsync(CreateSalesTerritoryDto dto) => await _repository.CreateAsync(new SalesTerritory { TenantId = dto.TenantId, TerritoryName = dto.TerritoryName, Description = dto.Description, ManagerId = dto.ManagerId, CreatedBy = dto.CreatedBy });
    public async Task<bool> UpdateAsync(UpdateSalesTerritoryDto dto) { var e = await _repository.GetByIdAsync(dto.Id); if (e == null) return false; if (dto.TerritoryName != null) e.TerritoryName = dto.TerritoryName; e.Description = dto.Description; e.ManagerId = dto.ManagerId; return await _repository.UpdateAsync(e); }
    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);
    private static SalesTerritoryDto Map(SalesTerritory e) => new() { Id = e.Id, TenantId = e.TenantId, TerritoryName = e.TerritoryName, Description = e.Description, ManagerId = e.ManagerId, IsActive = e.IsActive, CreatedAt = e.CreatedAt };
}

public class ReturnOrderService : IReturnOrderService
{
    private readonly IReturnOrderRepository _repository;
    public ReturnOrderService(IReturnOrderRepository repository) => _repository = repository;

    public async Task<ReturnOrderDto?> GetByIdAsync(Guid id) { var e = await _repository.GetByIdAsync(id); return e == null ? null : Map(e); }
    public async Task<IEnumerable<ReturnOrderDto>> GetAllAsync() => (await _repository.GetAllAsync()).Select(Map);
    public async Task<Guid> CreateAsync(CreateReturnOrderDto dto) => await _repository.CreateAsync(new ReturnOrder { TenantId = dto.TenantId, ReturnNumber = dto.ReturnNumber ?? CrmLineCalculator.GenerateNumber("RT"), ReturnDate = dto.ReturnDate, CustomerId = dto.CustomerId, SalesOrderId = dto.SalesOrderId, Reason = dto.Reason, Status = dto.Status, TotalAmount = dto.TotalAmount, Notes = dto.Notes, CreatedBy = dto.CreatedBy });
    public async Task<bool> UpdateAsync(UpdateReturnOrderDto dto) { var e = await _repository.GetByIdAsync(dto.Id); if (e == null) return false; if (dto.Reason != null) e.Reason = dto.Reason; if (dto.Status != null) e.Status = dto.Status; if (dto.TotalAmount.HasValue) e.TotalAmount = dto.TotalAmount.Value; e.Notes = dto.Notes; return await _repository.UpdateAsync(e); }
    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);
    private static ReturnOrderDto Map(ReturnOrder e) => new() { Id = e.Id, TenantId = e.TenantId, ReturnNumber = e.ReturnNumber, ReturnDate = e.ReturnDate, CustomerId = e.CustomerId, CustomerName = e.Customer?.CompanyName, SalesOrderId = e.SalesOrderId, Reason = e.Reason, Status = e.Status, TotalAmount = e.TotalAmount, Notes = e.Notes, IsActive = e.IsActive, CreatedAt = e.CreatedAt };
}
