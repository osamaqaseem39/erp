using ERP.Application.Interfaces;
using ERP.Shared.DTOs.Finance;

namespace ERP.Api.Services;

public class BillApiService : IBillApiService
{
    private readonly IBillService _service;

    public BillApiService(IBillService service)
    {
        _service = service;
    }

    public async Task<BillDto?> GetByIdAsync(Guid id)
    {
        return await _service.GetByIdAsync(id);
    }

    public async Task<IEnumerable<BillDto>> GetAllAsync()
    {
        return await _service.GetAllAsync();
    }

    public async Task<Guid> CreateAsync(CreateBillDto dto)
    {
        return await _service.CreateAsync(dto);
    }

    public async Task<bool> UpdateAsync(UpdateBillDto dto)
    {
        return await _service.UpdateAsync(dto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _service.DeleteAsync(id);
    }

    public async Task<IEnumerable<BillDto>> GetByVendorAsync(Guid vendorId)
    {
        return await _service.GetByVendorAsync(vendorId);
    }

    public async Task<IEnumerable<BillDto>> GetByStatusAsync(string status)
    {
        return await _service.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<BillDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _service.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        return await _service.ApproveAsync(id);
    }

    public async Task<bool> MarkAsPaidAsync(Guid id)
    {
        return await _service.MarkAsPaidAsync(id);
    }
}