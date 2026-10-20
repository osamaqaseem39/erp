using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.Core;

public class AdminEmailRepository : IAdminEmailRepository
{
    private readonly ERPDbContext _db;

    public AdminEmailRepository(ERPDbContext db) => _db = db;

    public Task<AdminEmail?> GetByIdAsync(Guid id, Guid tenantId) =>
        _db.AdminEmails.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

    public async Task<IReadOnlyList<AdminEmail>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var q = _db.AdminEmails.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (isActive.HasValue) q = q.Where(e => e.IsActive == isActive.Value);
        return await q.OrderBy(e => e.Email).ToListAsync();
    }

    public Task<AdminEmail?> FindByEmailAsync(Guid tenantId, string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.AdminEmails.FirstOrDefaultAsync(e =>
            e.TenantId == tenantId && e.Email.ToLower() == normalized);
    }

    public async Task<Guid> AddAsync(AdminEmail entity)
    {
        _db.AdminEmails.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(AdminEmail entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _db.AdminEmails.Update(entity);
        await _db.SaveChangesAsync();
    }
}
