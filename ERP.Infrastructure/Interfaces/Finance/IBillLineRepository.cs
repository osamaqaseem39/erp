using ERP.Domain.Finance;

namespace ERP.Infrastructure.Interfaces.Finance;

public interface IBillLineRepository
{
    Task<BillLine?> GetByIdAsync(int id);
    Task<IEnumerable<BillLine>> GetAllAsync();
    Task<BillLine> AddAsync(BillLine entity);
    Task UpdateAsync(BillLine entity);
    Task DeleteAsync(int id);
}