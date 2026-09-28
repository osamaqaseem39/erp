using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using System;
using System.Linq;

namespace ERP.Application.Procurement;

/// <summary>DI stub for IGoodsReceiptNoteService. Replace with real business logic incrementally.</summary>
public class GoodsReceiptNoteService : IGoodsReceiptNoteService
{
    public Task<GoodsReceiptNoteDto?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<GoodsReceiptNoteDto?>(null);
    }

    public Task<IEnumerable<GoodsReceiptNoteDto>> GetAllAsync()
    {
        return Task.FromResult(Enumerable.Empty<GoodsReceiptNoteDto>());
    }

    public Task<Guid> CreateAsync(CreateGoodsReceiptNoteDto dto)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> UpdateAsync(UpdateGoodsReceiptNoteDto dto)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<GoodsReceiptNoteDto>> GetByPurchaseOrderAsync(Guid purchaseOrderId)
    {
        return Task.FromResult(Enumerable.Empty<GoodsReceiptNoteDto>());
    }

    public Task<IEnumerable<GoodsReceiptNoteDto>> GetByStatusAsync(string status)
    {
        return Task.FromResult(Enumerable.Empty<GoodsReceiptNoteDto>());
    }

    public Task<IEnumerable<GoodsReceiptNoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(Enumerable.Empty<GoodsReceiptNoteDto>());
    }

    public Task<bool> ApproveAsync(Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<bool> RejectAsync(Guid id)
    {
        return Task.FromResult(false);
    }

}
