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
        return await prService.GetByIdAsync(prId);
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
        var allRequisitions = await prService.GetAllAsync();
        var filteredRequisitions = allRequisitions.AsQueryable();

        if (status != null)
            filteredRequisitions = filteredRequisitions.Where(pr => pr.Status == status);
        if (fromDate.HasValue)
            filteredRequisitions = filteredRequisitions.Where(pr => pr.RequestDate >= fromDate.Value);
        if (toDate.HasValue)
            filteredRequisitions = filteredRequisitions.Where(pr => pr.RequestDate <= toDate.Value);

        var totalCount = filteredRequisitions.Count();
        var pagedRequisitions = filteredRequisitions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedRequisitions, totalCount);
    }

    public async Task<IEnumerable<PurchaseRequisitionDetailDto>> GetPurchaseRequisitionDetails(
        [Service] IPurchaseRequisitionService prService,
        [ID] Guid prId,
        [ID] Guid tenantId)
    {
        // Note: GetPurchaseRequisitionDetailsAsync not available in current interface
        return new List<PurchaseRequisitionDetailDto>();
    }

    // Purchase Order Queries
    public async Task<PurchaseOrderDto?> GetPurchaseOrderById(
        [Service] IPurchaseOrderService poService,
        [ID] Guid poId,
        [ID] Guid tenantId)
    {
        return await poService.GetByIdAsync(poId);
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
        var allOrders = await poService.GetAllAsync();
        var filteredOrders = allOrders.AsQueryable();

        if (status != null)
            filteredOrders = filteredOrders.Where(po => po.Status == status);
        if (fromDate.HasValue)
            filteredOrders = filteredOrders.Where(po => po.OrderDate >= fromDate.Value);
        if (toDate.HasValue)
            filteredOrders = filteredOrders.Where(po => po.OrderDate <= toDate.Value);

        var totalCount = filteredOrders.Count();
        var pagedOrders = filteredOrders
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
        // Note: GetPurchaseOrderDetailsAsync not available in current interface
        return new List<PurchaseOrderDetailDto>();
    }

    // Goods Receipt Note Queries
    public async Task<GoodsReceiptNoteDto?> GetGoodsReceiptNoteById(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId)
    {
        return await grnService.GetByIdAsync(grnId);
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
        var allGrns = await grnService.GetAllAsync();
        var filteredGrns = allGrns.AsQueryable();

        if (status != null)
            filteredGrns = filteredGrns.Where(grn => grn.Status == status);
        if (fromDate.HasValue)
            filteredGrns = filteredGrns.Where(grn => grn.ReceivedDate >= fromDate.Value);
        if (toDate.HasValue)
            filteredGrns = filteredGrns.Where(grn => grn.ReceivedDate <= toDate.Value);

        var totalCount = filteredGrns.Count();
        var pagedGrns = filteredGrns
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedGrns, totalCount);
    }

    public async Task<IEnumerable<GoodsReceiptNoteDetailDto>> GetGoodsReceiptNoteDetails(
        [Service] IGoodsReceiptNoteService grnService,
        [ID] Guid grnId,
        [ID] Guid tenantId)
    {
        // Note: GetGoodsReceiptNoteDetailsAsync not available in current interface
        return new List<GoodsReceiptNoteDetailDto>();
    }

    // Purchase Invoice Queries
    public async Task<PurchaseInvoiceDto?> GetPurchaseInvoiceById(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId)
    {
        return await piService.GetByIdAsync(invoiceId);
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
        var allInvoices = await piService.GetAllAsync();
        var filteredInvoices = allInvoices.AsQueryable();

        if (status != null)
            filteredInvoices = filteredInvoices.Where(pi => pi.Status == status);
        if (fromDate.HasValue)
            filteredInvoices = filteredInvoices.Where(pi => pi.InvoiceDate >= fromDate.Value);
        if (toDate.HasValue)
            filteredInvoices = filteredInvoices.Where(pi => pi.InvoiceDate <= toDate.Value);

        var totalCount = filteredInvoices.Count();
        var pagedInvoices = filteredInvoices
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedInvoices, totalCount);
    }

    public async Task<IEnumerable<PurchaseInvoiceDetailDto>> GetPurchaseInvoiceDetails(
        [Service] IPurchaseInvoiceService piService,
        [ID] Guid invoiceId,
        [ID] Guid tenantId)
    {
        // Note: GetPurchaseInvoiceDetailsAsync not available in current interface
        return new List<PurchaseInvoiceDetailDto>();
    }
}