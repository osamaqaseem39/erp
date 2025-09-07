using ERP.Domain.Core;

namespace ERP.Infrastructure.Interfaces.Core;

public interface IUserRoleRepository
{
    Task<UserRole?> GetByIdAsync(int id);
    Task<IEnumerable<UserRole>> GetAllAsync();
    Task<UserRole> AddAsync(UserRole entity);
    Task UpdateAsync(UserRole entity);
    Task DeleteAsync(int id);
}