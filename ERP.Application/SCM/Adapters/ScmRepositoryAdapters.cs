using ERP.Application.SCM.Interfaces;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using InfraWarehouseRepository = ERP.Infrastructure.Repositories.SCM.IWarehouseRepository;

namespace ERP.Application.SCM.Adapters;

/// <summary>
/// Forwards the Application-layer warehouse contract to the stored-procedure repository (same shape).
/// </summary>
public class ApplicationWarehouseRepositoryAdapter : IWarehouseRepository
{
    private readonly InfraWarehouseRepository _inner;

    public ApplicationWarehouseRepositoryAdapter(InfraWarehouseRepository inner) => _inner = inner;

    public Task<Warehouse?> GetByIdAsync(Guid id, Guid tenantId) => _inner.GetByIdAsync(id, tenantId);
    public Task<IEnumerable<Warehouse>> GetAllAsync(Guid tenantId, Guid companyId) => _inner.GetAllAsync(tenantId, companyId);
    public Task<Warehouse> AddAsync(Warehouse warehouse) => _inner.AddAsync(warehouse);
    public Task<Warehouse> UpdateAsync(Warehouse warehouse) => _inner.UpdateAsync(warehouse);
    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _inner.DeleteAsync(id, tenantId);
    public Task<bool> ExistsAsync(Guid id, Guid tenantId) => _inner.ExistsAsync(id, tenantId);

    public Task<bool> CodeExistsAsync(string code, Guid tenantId, Guid companyId, Guid? excludeId = null)
        => _inner.CodeExistsAsync(code, tenantId, companyId, excludeId);
}

/// <summary>
/// Implements the Application-layer item contract against EF Core.
/// </summary>
public class ApplicationItemRepositoryAdapter : IItemRepository
{
    private readonly ERPDbContext _db;

    public ApplicationItemRepositoryAdapter(ERPDbContext db) => _db = db;

    private IQueryable<Item> ForTenant(Guid tenantId)
        => _db.Items.Where(i => tenantId == Guid.Empty || i.TenantId == tenantId);

    public Task<Item?> GetByIdAsync(Guid id, Guid tenantId)
        => ForTenant(tenantId).FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Item>> GetAllAsync(Guid tenantId, Guid companyId)
        => await ForTenant(tenantId).Where(i => i.CompanyId == companyId).ToListAsync();

    public async Task<IEnumerable<Item>> GetByCategoryAsync(Guid categoryId, Guid tenantId)
        => await ForTenant(tenantId).Where(i => i.CategoryId == categoryId).ToListAsync();

    public async Task<Item> AddAsync(Item item)
    {
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateAsync(Item item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _db.Items.Update(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        var item = await GetByIdAsync(id, tenantId);
        if (item == null) return false;
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id, Guid tenantId)
        => ForTenant(tenantId).AnyAsync(i => i.Id == id);

    public Task<bool> CodeExistsAsync(string code, Guid tenantId, Guid companyId, Guid? excludeId = null)
        => ForTenant(tenantId).AnyAsync(i => i.CompanyId == companyId && i.ItemCode == code
            && (!excludeId.HasValue || i.Id != excludeId.Value));

    public async Task<IEnumerable<Item>> GetLowStockItemsAsync(Guid tenantId, Guid companyId)
        => await ForTenant(tenantId)
            .Where(i => i.CompanyId == companyId && i.ReorderLevel > 0
                && (_db.StockBalances.Where(b => b.ItemId == i.Id).Sum(b => (decimal?)b.CurrentQuantity) ?? 0) <= i.ReorderLevel)
            .ToListAsync();
}
