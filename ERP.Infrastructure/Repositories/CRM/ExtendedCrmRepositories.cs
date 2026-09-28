using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public interface ICustomerGroupRepository
{
    Task<CustomerGroup?> GetByIdAsync(Guid id);
    Task<IEnumerable<CustomerGroup>> GetAllAsync();
    Task<Guid> CreateAsync(CustomerGroup entity);
    Task<bool> UpdateAsync(CustomerGroup entity);
    Task<bool> DeleteAsync(Guid id);
}

public interface IPriceListRepository
{
    Task<PriceList?> GetByIdAsync(Guid id);
    Task<IEnumerable<PriceList>> GetAllAsync();
    Task<Guid> CreateAsync(PriceList entity);
    Task<bool> UpdateAsync(PriceList entity);
    Task<bool> DeleteAsync(Guid id);
}

public interface ISalesTerritoryRepository
{
    Task<SalesTerritory?> GetByIdAsync(Guid id);
    Task<IEnumerable<SalesTerritory>> GetAllAsync();
    Task<Guid> CreateAsync(SalesTerritory entity);
    Task<bool> UpdateAsync(SalesTerritory entity);
    Task<bool> DeleteAsync(Guid id);
}

public interface IReturnOrderRepository
{
    Task<ReturnOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReturnOrder>> GetAllAsync();
    Task<Guid> CreateAsync(ReturnOrder entity);
    Task<bool> UpdateAsync(ReturnOrder entity);
    Task<bool> DeleteAsync(Guid id);
}

public class CustomerGroupRepository : CrmRepositoryBase, ICustomerGroupRepository
{
    public CustomerGroupRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider) { }
    private IQueryable<CustomerGroup> Query() => ApplyTenant(Context.CustomerGroups);
    public async Task<CustomerGroup?> GetByIdAsync(Guid id) => await Query().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<IEnumerable<CustomerGroup>> GetAllAsync() => await Query().ToListAsync();
    public async Task<Guid> CreateAsync(CustomerGroup entity) { if (TenantId.HasValue && TenantId != Guid.Empty) entity.TenantId = TenantId.Value; Context.CustomerGroups.Add(entity); await Context.SaveChangesAsync(); return entity.Id; }
    public async Task<bool> UpdateAsync(CustomerGroup entity) { if (!MatchesTenant(entity)) return false; Context.CustomerGroups.Update(entity); return await Context.SaveChangesAsync() > 0; }
    public async Task<bool> DeleteAsync(Guid id) { var e = await GetByIdAsync(id); if (e == null) return false; Context.CustomerGroups.Remove(e); return await Context.SaveChangesAsync() > 0; }
}

public class PriceListRepository : CrmRepositoryBase, IPriceListRepository
{
    public PriceListRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider) { }
    private IQueryable<PriceList> Query() => ApplyTenant(Context.PriceLists);
    public async Task<PriceList?> GetByIdAsync(Guid id) => await Query().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<IEnumerable<PriceList>> GetAllAsync() => await Query().ToListAsync();
    public async Task<Guid> CreateAsync(PriceList entity) { if (TenantId.HasValue && TenantId != Guid.Empty) entity.TenantId = TenantId.Value; Context.PriceLists.Add(entity); await Context.SaveChangesAsync(); return entity.Id; }
    public async Task<bool> UpdateAsync(PriceList entity) { if (!MatchesTenant(entity)) return false; Context.PriceLists.Update(entity); return await Context.SaveChangesAsync() > 0; }
    public async Task<bool> DeleteAsync(Guid id) { var e = await GetByIdAsync(id); if (e == null) return false; Context.PriceLists.Remove(e); return await Context.SaveChangesAsync() > 0; }
}

public class SalesTerritoryRepository : CrmRepositoryBase, ISalesTerritoryRepository
{
    public SalesTerritoryRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider) { }
    private IQueryable<SalesTerritory> Query() => ApplyTenant(Context.SalesTerritories);
    public async Task<SalesTerritory?> GetByIdAsync(Guid id) => await Query().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<IEnumerable<SalesTerritory>> GetAllAsync() => await Query().ToListAsync();
    public async Task<Guid> CreateAsync(SalesTerritory entity) { if (TenantId.HasValue && TenantId != Guid.Empty) entity.TenantId = TenantId.Value; Context.SalesTerritories.Add(entity); await Context.SaveChangesAsync(); return entity.Id; }
    public async Task<bool> UpdateAsync(SalesTerritory entity) { if (!MatchesTenant(entity)) return false; Context.SalesTerritories.Update(entity); return await Context.SaveChangesAsync() > 0; }
    public async Task<bool> DeleteAsync(Guid id) { var e = await GetByIdAsync(id); if (e == null) return false; Context.SalesTerritories.Remove(e); return await Context.SaveChangesAsync() > 0; }
}

public class ReturnOrderRepository : CrmRepositoryBase, IReturnOrderRepository
{
    public ReturnOrderRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider) { }
    private IQueryable<ReturnOrder> Query() => ApplyTenant(Context.ReturnOrders.Include(r => r.Customer));
    public async Task<ReturnOrder?> GetByIdAsync(Guid id) => await Query().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<IEnumerable<ReturnOrder>> GetAllAsync() => await Query().ToListAsync();
    public async Task<Guid> CreateAsync(ReturnOrder entity) { if (TenantId.HasValue && TenantId != Guid.Empty) entity.TenantId = TenantId.Value; Context.ReturnOrders.Add(entity); await Context.SaveChangesAsync(); return entity.Id; }
    public async Task<bool> UpdateAsync(ReturnOrder entity) { if (!MatchesTenant(entity)) return false; Context.ReturnOrders.Update(entity); return await Context.SaveChangesAsync() > 0; }
    public async Task<bool> DeleteAsync(Guid id) { var e = await GetByIdAsync(id); if (e == null) return false; Context.ReturnOrders.Remove(e); return await Context.SaveChangesAsync() > 0; }
}
