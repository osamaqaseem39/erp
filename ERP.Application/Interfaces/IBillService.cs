using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface IBillService
{
    Task<BillDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<BillDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateBillDto dto);
    Task<bool> UpdateAsync(UpdateBillDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<BillDto>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<BillDto>> GetByStatusAsync(string status);
    Task<IEnumerable<BillDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> MarkAsPaidAsync(Guid id);
}