using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface IAdminEmailRepository
{
    Task<AdminEmail?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<AdminEmail>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<AdminEmail?> FindByEmailAsync(Guid tenantId, string email);
    Task<Guid> AddAsync(AdminEmail entity);
    Task UpdateAsync(AdminEmail entity);
}
