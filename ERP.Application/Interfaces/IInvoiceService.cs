using ERP.Shared.DTOs.Finance;

namespace ERP.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<InvoiceDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateInvoiceDto dto);
    Task<bool> UpdateAsync(UpdateInvoiceDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<InvoiceDto>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<InvoiceDto>> GetByStatusAsync(string status);
    Task<IEnumerable<InvoiceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> SendInvoiceAsync(Guid id);
    Task<bool> MarkAsPaidAsync(Guid id);
}