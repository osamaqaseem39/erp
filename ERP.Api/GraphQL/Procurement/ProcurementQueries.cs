using ERP.Application.Interfaces.Procurement;
using ERP.Shared.DTOs.Procurement;
using HotChocolate;

namespace ERP.Api.GraphQL.Procurement;

[ExtendObjectType("Query")]
public class ProcurementQueries
{
    // Vendor Queries
    public async Task<VendorDto?> GetVendorById(
        [Service] IVendorService vendorService,
        [ID] Guid vendorId,
        [ID] Guid tenantId)
    {
        return await vendorService.GetVendorByIdAsync(vendorId, tenantId);
    }

    public async Task<(IEnumerable<VendorDto> Vendors, int TotalCount)> GetVendors(
        [Service] IVendorService vendorService,
        [ID] Guid tenantId,
        [ID] Guid? companyId = null,
        string? searchTerm = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50,
        string sortBy = "VendorName",
        string sortDirection = "ASC")
    {
        var query = new VendorQuery
        {
            TenantId = tenantId,
            CompanyId = companyId,
            SearchTerm = searchTerm,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        return await vendorService.SearchVendorsAsync(query);
    }

    public async Task<IEnumerable<VendorBankAccountDto>> GetVendorBankAccounts(
        [Service] IVendorService vendorService,
        [ID] Guid vendorId,
        [ID] Guid tenantId)
    {
        return await vendorService.GetVendorBankAccountsAsync(vendorId, tenantId);
    }

    // Purchase Requisition Queries
    public async Task<PurchaseRequisitionDto?> GetPurchaseRequisitionById(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId)
    {
        return await prService.GetByIdAsync(prId, tenantId);
    }

    public async Task<(IEnumerable<PurchaseRequisitionDto> PurchaseRequisitions, int TotalCount)> GetPurchaseRequisitions(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid tenantId,
        [ID] Guid? companyId = null,
        [ID] Guid? requestedByUserId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        string sortBy = "RequestDate",
        string sortDirection = "DESC")
    {
        var allRequisitions = (await prService.GetAllAsync(tenantId)).AsQueryable();
        if (requestedByUserId.HasValue)
            allRequisitions = allRequisitions.Where(pr => pr.RequestedByUserId == requestedByUserId.Value);
        if (status != null)
            allRequisitions = allRequisitions.Where(pr => pr.Status == status);
        if (fromDate.HasValue)
            allRequisitions = allRequisitions.Where(pr => pr.RequestDate >= fromDate.Value);
        if (toDate.HasValue)
            allRequisitions = allRequisitions.Where(pr => pr.RequestDate <= toDate.Value);

        var totalCount = allRequisitions.Count();
        var pagedRequisitions = allRequisitions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return (pagedRequisitions, totalCount);
    }

    public async Task<IEnumerable<PurchaseRequisitionDetailDto>> GetPurchaseRequisitionDetails(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId)
    {
        var pr = await prService.GetByIdAsync(prId, tenantId);
        return pr?.Details ?? pr?.RequisitionDetails ?? Enumerable.Empty<PurchaseRequisitionDetailDto>();
    }

    // Purchase Order Queries
    public async Task<PurchaseOrderDto?> GetPurchaseOrderById(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId)
    {
        return await poService.GetByIdAsync(poId, tenantId);
    }

    public async Task<(IEnumerable<PurchaseOrderDto> PurchaseOrders, int TotalCount)> GetPurchaseOrders(
        [Service] IPurchaseOrderService poService,
        [ID] Guid tenantId,
        [ID] Guid? companyId = null,
        [ID] Guid? vendorId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        string sortBy = "OrderDate",
        string sortDirection = "DESC")
    {
        var allOrders = (await poService.GetAllAsync(tenantId)).AsQueryable();
        if (vendorId.HasValue)
            allOrders = allOrders.Where(po => po.VendorId == vendorId.Value);
        if (status != null)
            allOrders = allOrders.Where(po => po.Status == status);
        if (fromDate.HasValue)
            allOrders = allOrders.Where(po => po.OrderDate >= fromDate.Value);
        if (toDate.HasValue)
            allOrders = allOrders.Where(po => po.OrderDate <= toDate.Value);

        var totalCount = allOrders.Count();
        var pagedOrders = allOrders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedOrders, totalCount);
    }

    public async Task<IEnumerable<PurchaseOrderDetailDto>> GetPurchaseOrderDetails(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId)
    {
        var order = await poService.GetByIdAsync(poId, tenantId);
        return order?.Details ?? order?.PurchaseOrderDetails ?? Enumerable.Empty<PurchaseOrderDetailDto>();
    }

    // Goods Receipt Note Queries
    public async Task<GoodsReceiptNoteDto?> GetGoodsReceiptNoteById(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId)
    {
        return await grnService.GetByIdAsync(grnId, tenantId);
    }

    public async Task<(IEnumerable<GoodsReceiptNoteDto> GoodsReceiptNotes, int TotalCount)> GetGoodsReceiptNotes(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid tenantId,
        [ID] Guid? companyId = null,
        [ID] Guid? poId = null,
        [ID] Guid? warehouseId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        string sortBy = "ReceivedDate",
        string sortDirection = "DESC")
    {
        var allGrns = (await grnService.GetAllAsync(tenantId)).AsQueryable();
        if (poId.HasValue)
            allGrns = allGrns.Where(grn => grn.POId == poId.Value || grn.PurchaseOrderId == poId.Value);
        if (status != null)
            allGrns = allGrns.Where(grn => grn.Status == status);
        if (fromDate.HasValue)
            allGrns = allGrns.Where(grn => (grn.ReceivedDate ?? grn.ReceiptDate) >= fromDate.Value);
        if (toDate.HasValue)
            allGrns = allGrns.Where(grn => (grn.ReceivedDate ?? grn.ReceiptDate) <= toDate.Value);

        var totalCount = allGrns.Count();
        var pagedGrns = allGrns.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return (pagedGrns, totalCount);
    }

    public async Task<IEnumerable<GoodsReceiptNoteDetailDto>> GetGoodsReceiptNoteDetails(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId)
    {
        var grn = await grnService.GetByIdAsync(grnId, tenantId);
        return grn?.Details ?? grn?.GRNDetails ?? Enumerable.Empty<GoodsReceiptNoteDetailDto>();
    }

    // Purchase Invoice Queries
    public async Task<PurchaseInvoiceDto?> GetPurchaseInvoiceById(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId)
    {
        return await piService.GetByIdAsync(invoiceId, tenantId);
    }

    public async Task<(IEnumerable<PurchaseInvoiceDto> PurchaseInvoices, int TotalCount)> GetPurchaseInvoices(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid tenantId,
        [ID] Guid? companyId = null,
        [ID] Guid? vendorId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        string sortBy = "InvoiceDate",
        string sortDirection = "DESC")
    {
        var allInvoices = (await piService.GetAllAsync(tenantId)).AsQueryable();
        if (vendorId.HasValue)
            allInvoices = allInvoices.Where(pi => pi.VendorId == vendorId.Value);
        if (status != null)
            allInvoices = allInvoices.Where(pi => pi.Status == status);
        if (fromDate.HasValue)
            allInvoices = allInvoices.Where(pi => pi.InvoiceDate >= fromDate.Value);
        if (toDate.HasValue)
            allInvoices = allInvoices.Where(pi => pi.InvoiceDate <= toDate.Value);

        var totalCount = allInvoices.Count();
        var pagedInvoices = allInvoices.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return (pagedInvoices, totalCount);
    }

    public async Task<IEnumerable<PurchaseInvoiceDetailDto>> GetPurchaseInvoiceDetails(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId)
    {
        var invoice = await piService.GetByIdAsync(invoiceId, tenantId);
        return invoice?.Details ?? invoice?.InvoiceDetails ?? Enumerable.Empty<PurchaseInvoiceDetailDto>();
    }

    // Request for Quotation
    public async Task<RequestForQuotationDto?> GetRequestForQuotation(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid rfqId,
        [ID] Guid tenantId)
        => await rfqService.GetByIdAsync(rfqId, tenantId);

    public async Task<IEnumerable<RequestForQuotationDto>> GetRequestForQuotations(
        [Service] IRequestForQuotationService rfqService,
        [ID] Guid tenantId,
        string? status = null)
        => string.IsNullOrEmpty(status)
            ? await rfqService.GetAllAsync(tenantId)
            : await rfqService.GetByStatusAsync(status, tenantId);

    // Purchase Return
    public async Task<PurchaseReturnDto?> GetPurchaseReturn(
        [Service] IPurchaseReturnService returnService,
        [ID] Guid returnId,
        [ID] Guid tenantId)
        => await returnService.GetByIdAsync(returnId, tenantId);

    public async Task<IEnumerable<PurchaseReturnDto>> GetPurchaseReturns(
        [Service] IPurchaseReturnService returnService,
        [ID] Guid tenantId,
        string? status = null)
    {
        var all = await returnService.GetAllAsync(tenantId);
        if (string.IsNullOrEmpty(status)) return all;
        return all.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
    }

    // Purchase Debit Note
    public async Task<PurchaseDebitNoteDto?> GetPurchaseDebitNote(
        [Service] IPurchaseDebitNoteService debitNoteService,
        [ID] Guid debitNoteId,
        [ID] Guid tenantId)
        => await debitNoteService.GetByIdAsync(debitNoteId, tenantId);

    public async Task<IEnumerable<PurchaseDebitNoteDto>> GetPurchaseDebitNotes(
        [Service] IPurchaseDebitNoteService debitNoteService,
        [ID] Guid tenantId,
        [ID] Guid? vendorId = null,
        string? status = null)
    {
        if (vendorId.HasValue)
            return await debitNoteService.GetByVendorAsync(vendorId.Value, tenantId);
        if (!string.IsNullOrEmpty(status))
            return await debitNoteService.GetByStatusAsync(status, tenantId);
        return await debitNoteService.GetAllAsync(tenantId);
    }
}