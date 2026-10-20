using ERP.Shared.DTOs.Reports;

namespace ERP.Application.Interfaces.Reports;

public interface IOperationalReportService
{
    Task<PendingSalesOrderReportDto> GetPendingSalesOrdersAsync(Guid tenantId, DateTime? asOfDate = null);
    Task<SalesInvoiceSummaryReportDto> GetSalesInvoiceSummaryAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<SalesSummaryByItemReportDto> GetSalesSummaryByItemAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<SalesCustomerItemReportDto> GetSalesSummaryCustomerItemAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<SalesCustomerItemReportDto> GetSalesSummaryItemCustomerAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<SalesItemRateDetailReportDto> GetSalesItemRateDetailAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, Guid? itemId = null, Guid? customerId = null);
    Task<CustomerInfoReportDto> GetCustomerInformationAsync(Guid tenantId, string? status = null);
    Task<CreditNoteReportDto> GetCreditNoteReportAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<PurchaseInvoiceSummaryReportDto> GetPurchaseInvoiceSummaryAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<StockBalanceReportDto> GetStockBalanceReportAsync(Guid tenantId, Guid? warehouseId = null);
    Task<SalesOrderScheduleReportDto> GetSalesOrderScheduleAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
}
