using ERP.Domain.Core;
using ERP.Shared.DTOs;

namespace ERP.Infrastructure.Repositories.Core;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId);
    Task<IEnumerable<User>> GetByCompanyAsync(Guid companyId);
    Task<IEnumerable<User>> GetByRoleAsync(Guid roleId);
    Task<Guid> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires);
    Task<bool> UpdateLastLoginAsync(Guid userId);
    Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash);
}