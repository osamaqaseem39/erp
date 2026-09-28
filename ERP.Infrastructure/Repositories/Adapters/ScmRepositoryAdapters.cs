using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Contracts = ERP.Infrastructure.Interfaces.SCM;

namespace ERP.Infrastructure.Repositories.Adapters;

/// <summary>
/// Implements the non-tenant Interfaces.SCM warehouse contract against EF Core.
/// </summary>
public class WarehouseRepositoryAdapter : Contracts.IWarehouseRepository
{
    private readonly ERPDbContext _db;

    public WarehouseRepositoryAdapter(ERPDbContext db) => _db = db;

    public Task<Warehouse?> GetByIdAsync(Guid id)
        => _db.Warehouses.FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
        => await _db.Warehouses.ToListAsync();

    public async Task<Warehouse> AddAsync(Warehouse entity)
    {
        _db.Warehouses.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Warehouse entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Warehouses.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Warehouses.FindAsync(id);
        if (entity == null) return;
        _db.Warehouses.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid id)
        => _db.Warehouses.AnyAsync(w => w.Id == id);

    public async Task<IEnumerable<Warehouse>> GetByCompanyIdAsync(Guid companyId)
        => await _db.Warehouses.Where(w => w.CompanyId == companyId).ToListAsync();

    public Task<Warehouse?> GetByCodeAsync(string code)
        => _db.Warehouses.FirstOrDefaultAsync(w => w.WarehouseCode == code);
}
