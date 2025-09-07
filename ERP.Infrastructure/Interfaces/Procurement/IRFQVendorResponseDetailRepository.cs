using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IRFQVendorResponseDetailRepository
{
    Task<RFQVendorResponseDetail?> GetByIdAsync(int id);
    Task<IEnumerable<RFQVendorResponseDetail>> GetAllAsync();
    Task<RFQVendorResponseDetail> AddAsync(RFQVendorResponseDetail entity);
    Task UpdateAsync(RFQVendorResponseDetail entity);
    Task DeleteAsync(int id);
}