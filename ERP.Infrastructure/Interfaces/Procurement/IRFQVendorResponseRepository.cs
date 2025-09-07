using ERP.Domain.Procurement;

namespace ERP.Infrastructure.Interfaces.Procurement;

public interface IRFQVendorResponseRepository
{
    Task<RFQVendorResponse?> GetByIdAsync(int id);
    Task<IEnumerable<RFQVendorResponse>> GetAllAsync();
    Task<RFQVendorResponse> AddAsync(RFQVendorResponse entity);
    Task UpdateAsync(RFQVendorResponse entity);
    Task DeleteAsync(int id);
}