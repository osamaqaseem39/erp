using ERP.Application.Interfaces.Sales;
using ERP.Shared.DTOs.Sales;
using HotChocolate;

namespace ERP.Api.GraphQL.Sales;

[ExtendObjectType("Mutation")]
public class SalesMutations
{
    // Sales Invoice Mutations
    public async Task<SalesInvoiceDto> AddSalesInvoice(
        [Service] ISalesInvoiceService service,
        CreateSalesInvoiceDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create sales invoice");
    }

    public async Task<SalesInvoiceDto> UpdateSalesInvoice(
        [Service] ISalesInvoiceService service,
        UpdateSalesInvoiceDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update sales invoice");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Sales invoice not found");
    }

    public async Task<bool> DeleteSalesInvoice(
        [Service] ISalesInvoiceService service,
        Guid invoiceId,
        Guid tenantId)
    {
        return await service.DeleteAsync(invoiceId, tenantId);
    }

    public async Task<bool> PostSalesInvoice(
        [Service] ISalesInvoiceService service,
        Guid invoiceId,
        Guid tenantId,
        Guid postedBy)
    {
        return await service.PostInvoiceAsync(invoiceId, tenantId, postedBy);
    }

    public async Task<bool> VoidSalesInvoice(
        [Service] ISalesInvoiceService service,
        Guid invoiceId,
        Guid tenantId,
        Guid voidedBy)
    {
        return await service.VoidInvoiceAsync(invoiceId, tenantId, voidedBy);
    }

    // POS Session Mutations
    public async Task<POSSessionDto> AddPOSSession(
        [Service] IPOSSessionService service,
        CreatePOSSessionDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create POS session");
    }

    public async Task<POSSessionDto> UpdatePOSSession(
        [Service] IPOSSessionService service,
        UpdatePOSSessionDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update POS session");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("POS session not found");
    }

    public async Task<bool> DeletePOSSession(
        [Service] IPOSSessionService service,
        Guid sessionId,
        Guid tenantId)
    {
        return await service.DeleteAsync(sessionId, tenantId);
    }

    public async Task<bool> ClosePOSSession(
        [Service] IPOSSessionService service,
        ClosePOSSessionDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CloseSessionAsync(input, tenantId, userId);
    }

    // POS Transaction Mutations
    public async Task<POSTransactionDto> AddPOSTransaction(
        [Service] IPOSTransactionService service,
        CreatePOSTransactionDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create POS transaction");
    }

    public async Task<POSTransactionDto> UpdatePOSTransaction(
        [Service] IPOSTransactionService service,
        UpdatePOSTransactionDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update POS transaction");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("POS transaction not found");
    }

    public async Task<bool> DeletePOSTransaction(
        [Service] IPOSTransactionService service,
        Guid transactionId,
        Guid tenantId)
    {
        return await service.DeleteAsync(transactionId, tenantId);
    }

    // Sales Credit Note Mutations
    public async Task<SalesCreditNoteDto> AddSalesCreditNote(
        [Service] ISalesCreditNoteService service,
        CreateSalesCreditNoteDto input,
        Guid tenantId,
        Guid userId)
    {
        var id = await service.CreateAsync(input, tenantId, userId);
        return await service.GetByIdAsync(id, tenantId) ?? throw new Exception("Failed to create sales credit note");
    }

    public async Task<SalesCreditNoteDto> UpdateSalesCreditNote(
        [Service] ISalesCreditNoteService service,
        UpdateSalesCreditNoteDto input,
        Guid tenantId,
        Guid userId)
    {
        var success = await service.UpdateAsync(input, tenantId, userId);
        if (!success)
            throw new Exception("Failed to update sales credit note");
        return await service.GetByIdAsync(input.Id, tenantId) ?? throw new Exception("Sales credit note not found");
    }

    public async Task<bool> DeleteSalesCreditNote(
        [Service] ISalesCreditNoteService service,
        Guid creditNoteId,
        Guid tenantId)
        => await service.DeleteAsync(creditNoteId, tenantId);

    public async Task<bool> PostSalesCreditNote(
        [Service] ISalesCreditNoteService service,
        Guid creditNoteId,
        Guid tenantId,
        Guid postedBy)
        => await service.PostAsync(creditNoteId, tenantId, postedBy);

    public async Task<bool> VoidSalesCreditNote(
        [Service] ISalesCreditNoteService service,
        Guid creditNoteId,
        VoidSalesCreditNoteDto? input,
        Guid tenantId,
        Guid voidedBy)
        => await service.VoidAsync(creditNoteId, input ?? new VoidSalesCreditNoteDto(), tenantId, voidedBy);
}

