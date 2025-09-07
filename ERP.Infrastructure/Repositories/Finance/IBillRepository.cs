using ERP.Domain.Finance;

namespace ERP.Infrastructure.Repositories.Finance;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(Guid id);
    Task<IEnumerable<Bill>> GetAllAsync();
    Task<IEnumerable<Bill>> GetBySupplierAsync(Guid supplierId);
    Task<IEnumerable<Bill>> GetByStatusAsync(string status);
    Task<IEnumerable<Bill>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Bill?> GetByNumberAsync(string billNumber);
    Task<Guid> CreateAsync(Bill bill);
    Task<bool> UpdateAsync(Bill bill);
    Task<bool> DeleteAsync(Guid id);
}