using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(Guid id);
    Task<IEnumerable<Bill>> GetAllAsync();
    Task<Bill> CreateAsync(Bill bill);
    Task<Bill> UpdateAsync(Bill bill);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Bill?> GetByNumberAsync(string number);
    Task<bool> IsNumberUniqueAsync(string number, Guid? excludeId = null);
    Task<IEnumerable<Bill>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<Bill>> GetByStatusAsync(string status);
    Task<IEnumerable<Bill>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}