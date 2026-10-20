using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using ERP.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public interface ISalesmanRepository
{
    Task<Salesman?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<Salesman>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(Salesman entity);
    Task<bool> UpdateAsync(Salesman entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ICustomerGroupRepository
{
    Task<CustomerGroup?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<CustomerGroup>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(CustomerGroup entity);
    Task<bool> UpdateAsync(CustomerGroup entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IPriceListRepository
{
    Task<PriceList?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<PriceList>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(PriceList entity);
    Task<bool> UpdateAsync(PriceList entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface ISalesTerritoryRepository
{
    Task<SalesTerritory?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<SalesTerritory>> GetAllAsync(Guid tenantId);
    Task<Guid> CreateAsync(SalesTerritory entity);
    Task<bool> UpdateAsync(SalesTerritory entity);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
}

public interface IReturnOrderRepository
{
    Task<ReturnOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReturnOrder>> GetAllAsync();
    Task<Guid> CreateAsync(ReturnOrder entity);
    Task<bool> UpdateAsync(ReturnOrder entity);
    Task<bool> DeleteAsync(Guid id);
}

public class SalesmanRepository : ISalesmanRepository
{
    private readonly ERPDbContext _db;

    public SalesmanRepository(ERPDbContext context, ITenantProvider _) => _db = context;

    public async Task<Salesman?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.Salesmen.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<Salesman>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Salesman>();
        return await _db.Salesmen.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Salesman entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _db.Salesmen.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Salesman entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.Salesmen
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        _db.Entry(existing).CurrentValues.SetValues(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.Salesmen.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        if (await _db.Customers.AnyAsync(c => c.SalesmanId == id && c.TenantId == tenantId))
            return false;
        _db.Salesmen.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class CustomerGroupRepository : ICustomerGroupRepository
{
    private readonly ERPDbContext _db;

    public CustomerGroupRepository(ERPDbContext context, ITenantProvider _) => _db = context;

    public async Task<CustomerGroup?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.CustomerGroups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<CustomerGroup>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<CustomerGroup>();
        return await _db.CustomerGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.GroupName)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(CustomerGroup entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _db.CustomerGroups.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(CustomerGroup entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.CustomerGroups
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        _db.Entry(existing).CurrentValues.SetValues(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.CustomerGroups.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        _db.CustomerGroups.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class PriceListRepository : IPriceListRepository
{
    private readonly ERPDbContext _db;

    public PriceListRepository(ERPDbContext context, ITenantProvider _) => _db = context;

    public async Task<PriceList?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.PriceLists.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<PriceList>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<PriceList>();
        return await _db.PriceLists.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.PriceListName)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(PriceList entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _db.PriceLists.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(PriceList entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.PriceLists
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        _db.Entry(existing).CurrentValues.SetValues(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.PriceLists.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        _db.PriceLists.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class SalesTerritoryRepository : ISalesTerritoryRepository
{
    private readonly ERPDbContext _db;

    public SalesTerritoryRepository(ERPDbContext context, ITenantProvider _) => _db = context;

    public async Task<SalesTerritory?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _db.SalesTerritories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<SalesTerritory>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<SalesTerritory>();
        return await _db.SalesTerritories.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.TerritoryName)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(SalesTerritory entity)
    {
        if (entity.TenantId == Guid.Empty) return Guid.Empty;
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
        _db.SalesTerritories.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(SalesTerritory entity)
    {
        if (entity.TenantId == Guid.Empty) return false;
        var existing = await _db.SalesTerritories
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.TenantId == entity.TenantId);
        if (existing == null) return false;
        _db.Entry(existing).CurrentValues.SetValues(entity);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var e = await _db.SalesTerritories.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (e == null) return false;
        _db.SalesTerritories.Remove(e);
        return await _db.SaveChangesAsync() > 0;
    }
}

public class ReturnOrderRepository : CrmRepositoryBase, IReturnOrderRepository
{
    public ReturnOrderRepository(ERPDbContext context, ITenantProvider tenantProvider) : base(context, tenantProvider) { }
    private IQueryable<ReturnOrder> Query() => ApplyTenant(
        Context.ReturnOrders.Include(r => r.Customer).Include(r => r.Lines));
    public async Task<ReturnOrder?> GetByIdAsync(Guid id) => await Query().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<IEnumerable<ReturnOrder>> GetAllAsync() => await Query().ToListAsync();
    public async Task<Guid> CreateAsync(ReturnOrder entity)
    {
        if (TenantId.HasValue && TenantId != Guid.Empty) entity.TenantId = TenantId.Value;
        Context.ReturnOrders.Add(entity);
        await Context.SaveChangesAsync();
        return entity.Id;
    }
    public async Task<bool> UpdateAsync(ReturnOrder entity)
    {
        if (!MatchesTenant(entity)) return false;
        Context.ReturnOrders.Update(entity);
        return await Context.SaveChangesAsync() > 0;
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var e = await GetByIdAsync(id);
        if (e == null) return false;
        Context.ReturnOrders.Remove(e);
        return await Context.SaveChangesAsync() > 0;
    }
}
