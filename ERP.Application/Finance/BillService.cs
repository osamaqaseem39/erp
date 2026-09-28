using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using System;
using System.Linq;

namespace ERP.Application.Finance;

/// <summary>DI stub for IBillService. Replace with real business logic incrementally.</summary>
public class BillService : IBillService
{
    public Task<BillDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        return Task.FromResult<BillDto?>(null);
    }

    public Task<IEnumerable<BillDto>> GetAllAsync(Guid tenantId, Guid? vendorId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        return Task.FromResult(Enumerable.Empty<BillDto>());
    }

    public Task<BillDto> CreateAsync(CreateBillDto dto, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("BillService.CreateAsync is not yet implemented");
    }

    public Task<BillDto> UpdateAsync(UpdateBillDto dto, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("BillService.UpdateAsync is not yet implemented");
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<BillDto>> GetByVendorAsync(Guid vendorId, Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<BillDto>());
    }

    public Task<IEnumerable<BillDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<BillDto>());
    }

    public Task<IEnumerable<BillDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid tenantId)
    {
        return Task.FromResult(Enumerable.Empty<BillDto>());
    }

    public Task<BillDto> ApproveAsync(Guid id, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("BillService.ApproveAsync is not yet implemented");
    }

    public Task<BillDto> MarkAsPaidAsync(Guid id, PaymentDto paymentDto, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("BillService.MarkAsPaidAsync is not yet implemented");
    }

    public Task<BillDto> VoidBillAsync(Guid id, VoidBillDto dto, Guid tenantId, Guid userId)
    {
        throw new NotImplementedException("BillService.VoidBillAsync is not yet implemented");
    }

}
