using ERP.Domain.HRM;

namespace ERP.Infrastructure.Repositories.HRM;

public interface IShiftRepository
{
    Task<Shift?> GetByIdAsync(Guid id);
    Task<IEnumerable<Shift>> GetAllAsync();
    Task<IEnumerable<Shift>> GetActiveAsync();
    Task<Guid> CreateAsync(Shift shift);
    Task<bool> UpdateAsync(Shift shift);
    Task<bool> DeleteAsync(Guid id);
}