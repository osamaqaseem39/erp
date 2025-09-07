using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Interfaces.Admin;

public interface ISystemLogService
{
    Task<SystemLogDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<SystemLogDto>> GetAllAsync();
    Task<IEnumerable<SystemLogDto>> GetByLevelAsync(string logLevel);
    Task<IEnumerable<SystemLogDto>> GetByUserAsync(Guid userId);
    Task<IEnumerable<SystemLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Guid> CreateAsync(CreateSystemLogDto dto);
    Task<bool> UpdateAsync(UpdateSystemLogDto dto);
    Task<bool> DeleteAsync(Guid id);
}