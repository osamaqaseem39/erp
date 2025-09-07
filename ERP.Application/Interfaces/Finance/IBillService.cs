using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces.Finance;

public interface IBillService
{
    Task<BillDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BillDto>> GetAllAsync(Guid tenantId, Guid? vendorId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<BillDto> CreateAsync(CreateBillDto dto, Guid tenantId, Guid userId);
    Task<BillDto> UpdateAsync(UpdateBillDto dto, Guid tenantId, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<IEnumerable<BillDto>> GetByVendorAsync(Guid vendorId, Guid tenantId);
    Task<IEnumerable<BillDto>> GetByStatusAsync(string status, Guid tenantId);
    Task<IEnumerable<BillDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId);
    Task<BillDto> ApproveAsync(Guid id, Guid tenantId, Guid userId);
    Task<BillDto> MarkAsPaidAsync(Guid id, PaymentDto paymentDto, Guid tenantId, Guid userId);
    Task<BillDto> VoidBillAsync(Guid id, VoidBillDto dto, Guid tenantId, Guid userId);
}