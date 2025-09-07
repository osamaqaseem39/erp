using ERP.Domain.Core;

namespace ERP.Infrastructure.Interfaces.Core;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByIdAsync(int id);
    Task<IEnumerable<UserSession>> GetAllAsync();
    Task<UserSession> AddAsync(UserSession entity);
    Task UpdateAsync(UserSession entity);
    Task DeleteAsync(int id);
}