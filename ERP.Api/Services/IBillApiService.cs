using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Services;

public interface IBillApiService : IApiService<BillDto, CreateBillDto, UpdateBillDto>
{
    Task<IEnumerable<BillDto>> GetByVendorAsync(Guid vendorId);
    Task<IEnumerable<BillDto>> GetByStatusAsync(string status);
    Task<IEnumerable<BillDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> MarkAsPaidAsync(Guid id);
}