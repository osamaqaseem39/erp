using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IRFQDetailRepository
{
    Task<RFQDetail?> GetByIdAsync(int id);
    Task<IEnumerable<RFQDetail>> GetAllAsync();
    Task<RFQDetail> AddAsync(RFQDetail entity);
    Task UpdateAsync(RFQDetail entity);
    Task DeleteAsync(int id);
}