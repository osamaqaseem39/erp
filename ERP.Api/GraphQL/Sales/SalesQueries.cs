using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;
using HotChocolate;

namespace ERP.Api.GraphQL.Sales;

[ExtendObjectType("Query")]
public class SalesQueries
{
    // Sales Invoice Queries
    public async Task<SalesInvoiceDto?> GetSalesInvoice(
        [Service] ISalesInvoiceService service,
        Guid invoiceId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(invoiceId, tenantId);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetSalesInvoices(
        [Service] ISalesInvoiceService service,
        Guid tenantId,
        Guid? customerId = null,
        string? status = null)
    {
        if (customerId.HasValue)
            return await service.GetByCustomerAsync(customerId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetSalesInvoicesByCustomer(
        [Service] ISalesInvoiceService service,
        Guid customerId,
        Guid tenantId)
    {
        return await service.GetByCustomerAsync(customerId, tenantId);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetSalesInvoicesByStatus(
        [Service] ISalesInvoiceService service,
        string status,
        Guid tenantId)
    {
        return await service.GetByStatusAsync(status, tenantId);
    }

    // POS Session Queries
    public async Task<POSSessionDto?> GetPOSSession(
        [Service] IPOSSessionService service,
        Guid sessionId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(sessionId, tenantId);
    }

    public async Task<IEnumerable<POSSessionDto>> GetPOSSessions(
        [Service] IPOSSessionService service,
        Guid tenantId,
        Guid? cashierId = null,
        string? status = null)
    {
        if (cashierId.HasValue)
            return await service.GetByCashierAsync(cashierId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    public async Task<IEnumerable<POSSessionDto>> GetPOSSessionsByCashier(
        [Service] IPOSSessionService service,
        Guid cashierId,
        Guid tenantId)
    {
        return await service.GetByCashierAsync(cashierId, tenantId);
    }

    public async Task<IEnumerable<POSSessionDto>> GetPOSSessionsByStatus(
        [Service] IPOSSessionService service,
        string status,
        Guid tenantId)
    {
        return await service.GetByStatusAsync(status, tenantId);
    }

    // POS Transaction Queries
    public async Task<POSTransactionDto?> GetPOSTransaction(
        [Service] IPOSTransactionService service,
        Guid transactionId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(transactionId, tenantId);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetPOSTransactions(
        [Service] IPOSTransactionService service,
        Guid tenantId,
        Guid? sessionId = null,
        Guid? customerId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (sessionId.HasValue)
            return await service.GetBySessionAsync(sessionId.Value, tenantId);
        if (customerId.HasValue)
            return await service.GetByCustomerAsync(customerId.Value, tenantId);
        if (startDate.HasValue && endDate.HasValue)
            return await service.GetByDateRangeAsync(startDate.Value, endDate.Value, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetPOSTransactionsBySession(
        [Service] IPOSTransactionService service,
        Guid sessionId,
        Guid tenantId)
    {
        return await service.GetBySessionAsync(sessionId, tenantId);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetPOSTransactionsByCustomer(
        [Service] IPOSTransactionService service,
        Guid customerId,
        Guid tenantId)
    {
        return await service.GetByCustomerAsync(customerId, tenantId);
    }

    public async Task<IEnumerable<POSTransactionDto>> GetPOSTransactionsByDateRange(
        [Service] IPOSTransactionService service,
        DateTime startDate,
        DateTime endDate,
        Guid tenantId)
    {
        return await service.GetByDateRangeAsync(startDate, endDate, tenantId);
    }

    // Sales Credit Note Queries
    public async Task<SalesCreditNoteDto?> GetSalesCreditNote(
        [Service] ISalesCreditNoteService service,
        Guid creditNoteId,
        Guid tenantId)
        => await service.GetByIdAsync(creditNoteId, tenantId);

    public async Task<IEnumerable<SalesCreditNoteDto>> GetSalesCreditNotes(
        [Service] ISalesCreditNoteService service,
        Guid tenantId,
        Guid? customerId = null,
        string? status = null)
    {
        if (customerId.HasValue)
            return await service.GetByCustomerAsync(customerId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await service.GetByStatusAsync(status, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    public async Task<IEnumerable<SalesCreditNoteDto>> GetSalesCreditNotesByCustomer(
        [Service] ISalesCreditNoteService service,
        Guid customerId,
        Guid tenantId)
        => await service.GetByCustomerAsync(customerId, tenantId);

    public async Task<IEnumerable<SalesCreditNoteDto>> GetSalesCreditNotesByStatus(
        [Service] ISalesCreditNoteService service,
        string status,
        Guid tenantId)
        => await service.GetByStatusAsync(status, tenantId);
}

