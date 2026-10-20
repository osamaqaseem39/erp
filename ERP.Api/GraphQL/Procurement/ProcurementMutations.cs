using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using HotChocolate;

namespace ERP.Api.GraphQL.Procurement;

[ExtendObjectType("Mutation")]
public class ProcurementMutations
{
    // Vendor
    public async Task<Guid> CreateVendor(
        [Service] IVendorService vendorService,
        VendorCreateDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await vendorService.AddVendorAsync(input, tenantId, userId);

    public async Task<Guid> UpdateVendor(
        [Service] IVendorService vendorService,
        VendorUpdateDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await vendorService.UpdateVendorAsync(input, tenantId, userId);

    public async Task<bool> DeleteVendor(
        [Service] IVendorService vendorService,
        [ID] Guid vendorId,
        [ID] Guid tenantId)
        => await vendorService.DeleteVendorAsync(vendorId, tenantId);

    public async Task<bool> ApproveVendor(
        [Service] IVendorService vendorService,
        [ID] Guid vendorId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await vendorService.ApproveAsync(vendorId, tenantId, userId);

    public async Task<bool> SuspendVendor(
        [Service] IVendorService vendorService,
        [ID] Guid vendorId,
        SuspendVendorDto? input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await vendorService.SuspendAsync(
            vendorId,
            input ?? new SuspendVendorDto { VendorId = vendorId },
            tenantId,
            userId);

    // Vendor bank accounts
    public async Task<Guid> AddVendorBankAccount(
        [Service] IVendorService vendorService,
        CreateVendorBankAccountDto input,
        [ID] Guid vendorId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await vendorService.AddVendorBankAccountAsync(input, vendorId, tenantId, userId);

    public async Task<Guid> UpdateVendorBankAccount(
        [Service] IVendorService vendorService,
        UpdateVendorBankAccountDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await vendorService.UpdateVendorBankAccountAsync(input, tenantId, userId);

    public async Task<bool> DeleteVendorBankAccount(
        [Service] IVendorService vendorService,
        [ID] Guid bankAccountId,
        [ID] Guid tenantId)
        => await vendorService.DeleteVendorBankAccountAsync(bankAccountId, tenantId);

    // Purchase Requisition — Draft → Approved / Rejected
    public async Task<Guid> CreatePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        CreatePurchaseRequisitionDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await prService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdatePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        UpdatePurchaseRequisitionDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await prService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeletePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId)
        => await prService.DeleteAsync(prId, tenantId);

    public async Task<bool> ApprovePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId,
        [ID] Guid approvedBy)
        => await prService.ApproveAsync(prId, tenantId, approvedBy);

    public async Task<bool> RejectPurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId,
        [ID] Guid rejectedBy)
        => await prService.RejectAsync(prId, tenantId, rejectedBy);

    // Purchase Order — Draft → Approved / Rejected
    public async Task<Guid> CreatePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        CreatePurchaseOrderDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await poService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdatePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        UpdatePurchaseOrderDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await poService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeletePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId)
        => await poService.DeleteAsync(poId, tenantId);

    public async Task<bool> ApprovePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId,
        [ID] Guid approvedBy)
        => await poService.ApproveAsync(poId, tenantId, approvedBy);

    public async Task<bool> RejectPurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId,
        [ID] Guid rejectedBy)
        => await poService.RejectAsync(poId, tenantId, rejectedBy);

    // Goods Receipt Note — Draft → Posted (Approve posts stock) / Cancelled
    public async Task<Guid> CreateGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        CreateGoodsReceiptNoteDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await grnService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        UpdateGoodsReceiptNoteDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await grnService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId)
        => await grnService.DeleteAsync(grnId, tenantId);

    /// <summary>Posts GRN to stock (same as REST approve).</summary>
    public async Task<bool> ApproveGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await grnService.ApproveAsync(grnId, tenantId, userId);

    public async Task<bool> RejectGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await grnService.RejectAsync(grnId, tenantId, userId);

    // Purchase Invoice — Draft → Approved (creates AP) / Cancelled
    public async Task<Guid> CreatePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        CreatePurchaseInvoiceDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await piService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdatePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        UpdatePurchaseInvoiceDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await piService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeletePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId)
        => await piService.DeleteAsync(invoiceId, tenantId);

    public async Task<bool> ApprovePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await piService.ApproveAsync(invoiceId, tenantId, userId);

    public async Task<bool> RejectPurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await piService.RejectAsync(invoiceId, tenantId, userId);

    // Purchase Return — Draft → Approved → Complete / Cancel
    public async Task<Guid> CreatePurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        CreatePurchaseReturnDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await returnService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdatePurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        UpdatePurchaseReturnDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await returnService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeletePurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        [ID] Guid returnId,
        [ID] Guid tenantId)
        => await returnService.DeleteAsync(returnId, tenantId);

    public async Task<bool> ApprovePurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        [ID] Guid returnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await returnService.ApproveAsync(returnId, tenantId, userId);

    public async Task<bool> CompletePurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        [ID] Guid returnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await returnService.CompleteAsync(returnId, tenantId, userId);

    public async Task<bool> CancelPurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        [ID] Guid returnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await returnService.CancelAsync(returnId, tenantId, userId);

    // Purchase Debit Note — post / void
    public async Task<Guid> CreatePurchaseDebitNote(
        [Service] IPurchaseDebitNoteService debitNoteService,
        CreatePurchaseDebitNoteDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await debitNoteService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdatePurchaseDebitNote(
        [Service] IPurchaseDebitNoteService debitNoteService,
        UpdatePurchaseDebitNoteDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await debitNoteService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeletePurchaseDebitNote(
        [Service] IPurchaseDebitNoteService debitNoteService,
        [ID] Guid debitNoteId,
        [ID] Guid tenantId)
        => await debitNoteService.DeleteAsync(debitNoteId, tenantId);

    public async Task<bool> PostPurchaseDebitNote(
        [Service] IPurchaseDebitNoteService debitNoteService,
        [ID] Guid debitNoteId,
        [ID] Guid tenantId,
        [ID] Guid postedBy)
        => await debitNoteService.PostAsync(debitNoteId, tenantId, postedBy);

    public async Task<bool> VoidPurchaseDebitNote(
        [Service] IPurchaseDebitNoteService debitNoteService,
        [ID] Guid debitNoteId,
        VoidPurchaseDebitNoteDto? input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await debitNoteService.VoidAsync(
            debitNoteId,
            input ?? new VoidPurchaseDebitNoteDto(),
            tenantId,
            userId);

    // Request for Quotation — Draft → Open → Awarded/Cancelled/Closed
    public async Task<Guid> CreateRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        CreateRequestForQuotationDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        UpdateRequestForQuotationDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid rfqId,
        [ID] Guid tenantId)
        => await rfqService.DeleteAsync(rfqId, tenantId);

    public async Task<bool> SendRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid rfqId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.SendAsync(rfqId, tenantId, userId);

    public async Task<bool> CancelRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid rfqId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.CancelAsync(rfqId, tenantId, userId);

    public async Task<bool> CloseRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid rfqId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.CloseAsync(rfqId, tenantId, userId);

    public async Task<Guid> SubmitRfqVendorResponse(
        [Service] IRequestForQuotationService rfqService,
        CreateRFQVendorResponseDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.SubmitVendorResponseAsync(input, tenantId, userId);

    public async Task<bool> AwardRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid rfqId,
        [ID] Guid vendorResponseId,
        [ID] Guid tenantId,
        [ID] Guid userId)
        => await rfqService.AwardAsync(rfqId, vendorResponseId, tenantId, userId);
}
