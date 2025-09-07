using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using HotChocolate;

namespace ERP.Api.GraphQL.Procurement;

[ExtendObjectType("Mutation")]
public class ProcurementMutations
{
    // Vendor Mutations
    public async Task<Guid> CreateVendor(
        [Service] IVendorService vendorService,
        VendorCreateDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await vendorService.AddVendorAsync(input, tenantId, userId);
    }

    public async Task<Guid> UpdateVendor(
        [Service] IVendorService vendorService,
        VendorUpdateDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await vendorService.UpdateVendorAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteVendor(
        [Service] IVendorService vendorService,
        [ID] Guid vendorId,
        [ID] Guid tenantId)
    {
        return await vendorService.DeleteVendorAsync(vendorId, tenantId);
    }

    // Purchase Requisition Mutations
    public async Task<Guid> CreatePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        CreatePurchaseRequisitionDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await prService.CreateAsync(input);
    }

    public async Task<bool> UpdatePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        UpdatePurchaseRequisitionDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await prService.UpdateAsync(input);
    }

    public async Task<bool> DeletePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId)
    {
        return await prService.DeleteAsync(prId);
    }

    public async Task<bool> SubmitPurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Submit functionality not available in current interface
        return false;
    }

    public async Task<bool> ApprovePurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId,
        [ID] Guid approvedBy)
    {
        return await prService.ApproveAsync(prId);
    }

    public async Task<bool> RejectPurchaseRequisition(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId,
        [ID] Guid rejectedBy,
        string rejectionReason)
    {
        return await prService.RejectAsync(prId);
    }

    // Purchase Order Mutations
    public async Task<Guid> CreatePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        CreatePurchaseOrderDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await poService.CreateAsync(input);
    }

    public async Task<bool> UpdatePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        UpdatePurchaseOrderDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await poService.UpdateAsync(input);
    }

    public async Task<bool> DeletePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId)
    {
        return await poService.DeleteAsync(poId);
    }

    public async Task<bool> IssuePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Issue functionality not available in current interface
        return false;
    }

    public async Task<bool> ApprovePurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId,
        [ID] Guid approvedBy)
    {
        return await poService.ApproveAsync(poId);
    }

    public async Task<bool> CancelPurchaseOrder(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId,
        [ID] Guid userId,
        string cancellationReason)
    {
        // Note: Cancel functionality not available in current interface
        return false;
    }

    // Goods Receipt Note Mutations
    public async Task<Guid> CreateGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        CreateGoodsReceiptNoteDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await grnService.CreateAsync(input);
    }

    public async Task<bool> UpdateGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        UpdateGoodsReceiptNoteDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await grnService.UpdateAsync(input);
    }

    public async Task<bool> DeleteGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId)
    {
        return await grnService.DeleteAsync(grnId);
    }

    public async Task<bool> VerifyGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Verify functionality not available in current interface
        return false;
    }

    public async Task<bool> PostGoodsReceiptNote(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Post functionality not available in current interface
        return false;
    }

    // Purchase Invoice Mutations
    public async Task<Guid> CreatePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        CreatePurchaseInvoiceDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await piService.CreateAsync(input);
    }

    public async Task<bool> UpdatePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        UpdatePurchaseInvoiceDto input,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        return await piService.UpdateAsync(input);
    }

    public async Task<bool> DeletePurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId)
    {
        return await piService.DeleteAsync(invoiceId);
    }

    public async Task<bool> PostPurchaseInvoice(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Post functionality not available in current interface
        return false;
    }

    public async Task<bool> MarkInvoiceAsPaid(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Mark as paid functionality not available in current interface
        return false;
    }

    public async Task<bool> MarkInvoiceAsPartiallyPaid(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        decimal paidAmount,
        [ID] Guid tenantId,
        [ID] Guid userId)
    {
        // Note: Mark as partially paid functionality not available in current interface
        return false;
    }
}