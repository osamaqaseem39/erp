using ERP.Application.Interfaces.Reports;
using ERP.Infrastructure.Database;
using ERP.Shared.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Reports;

public class OperationalReportService : IOperationalReportService
{
    private readonly ERPDbContext _db;

    public OperationalReportService(ERPDbContext db) => _db = db;

    public async Task<PendingSalesOrderReportDto> GetPendingSalesOrdersAsync(Guid tenantId, DateTime? asOfDate = null)
    {
        if (tenantId == Guid.Empty)
            return new PendingSalesOrderReportDto { AsOfDate = (asOfDate ?? DateTime.UtcNow).Date, Lines = [], OrderCount = 0 };
        var asOf = (asOfDate ?? DateTime.UtcNow).Date;
        var q = _db.SalesOrders.AsNoTracking()
            .Include(o => o.Customer)
            .Where(o => o.TenantId == tenantId)
            .Where(o => o.OrderDate <= asOf)
            .Where(o => o.InvoicedDate == null)
            .Where(o => o.Status != "Cancelled" && o.Status != "Delivered" && o.Status != "Invoiced");

        var rows = await q.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderNumber).ToListAsync();
        var lines = rows.Select(o => new PendingSalesOrderLineDto
        {
            SalesOrderId = o.Id,
            OrderNumber = string.IsNullOrWhiteSpace(o.SalesOrderNumber) ? o.OrderNumber : o.SalesOrderNumber,
            OrderDate = o.OrderDate,
            RequiredDate = o.RequiredDate,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer?.CompanyName ?? o.Customer?.CustomerName,
            Status = o.Status,
            SubTotal = o.SubTotal,
            TaxAmount = o.TaxAmount,
            TotalAmount = o.TotalAmount
        }).ToList();

        return new PendingSalesOrderReportDto
        {
            AsOfDate = asOf,
            Lines = lines,
            OrderCount = lines.Count,
            TotalValue = Math.Round(lines.Sum(l => l.TotalAmount), 2)
        };
    }

    public async Task<SalesInvoiceSummaryReportDto> GetSalesInvoiceSummaryAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        if (tenantId == Guid.Empty)
            return new SalesInvoiceSummaryReportDto { FromDate = fromDate.Date, ToDate = toDate.Date, Lines = [] };
        var fromD = fromDate.Date;
        var toD = toDate.Date;
        var invoices = await (
            from i in _db.SalesInvoices.AsNoTracking()
            join c in _db.Customers.AsNoTracking() on i.CustomerId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            where (i.TenantId == tenantId)
                  && i.InvoiceDate >= fromD && i.InvoiceDate <= toD
                  && i.Status != "Voided"
            orderby i.InvoiceDate, i.InvoiceNumber
            select new SalesInvoiceSummaryLineDto
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                CustomerId = i.CustomerId,
                CustomerName = c != null ? (c.CompanyName ?? c.CustomerName) : null,
                Status = i.Status,
                SubTotal = i.SubTotal,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                BalanceAmount = i.BalanceAmount
            }).ToListAsync();

        return new SalesInvoiceSummaryReportDto
        {
            FromDate = fromD,
            ToDate = toD,
            Lines = invoices,
            InvoiceCount = invoices.Count,
            TotalSubTotal = Math.Round(invoices.Sum(x => x.SubTotal), 2),
            TotalTax = Math.Round(invoices.Sum(x => x.TaxAmount), 2),
            TotalAmount = Math.Round(invoices.Sum(x => x.TotalAmount), 2),
            TotalPaid = Math.Round(invoices.Sum(x => x.PaidAmount), 2),
            TotalBalance = Math.Round(invoices.Sum(x => x.BalanceAmount), 2)
        };
    }

    public async Task<SalesSummaryByItemReportDto> GetSalesSummaryByItemAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var fromD = fromDate.Date;
        var toD = toDate.Date;
        var q =
            from l in _db.SalesInvoiceLines.AsNoTracking()
            join i in _db.SalesInvoices.AsNoTracking() on l.SalesInvoiceId equals i.Id
            where (i.TenantId == tenantId)
                  && i.InvoiceDate >= fromD && i.InvoiceDate <= toD
                  && i.Status != "Voided"
            group new { l, i } by new { l.ItemId, l.Description } into g
            select new SalesSummaryByItemLineDto
            {
                ItemId = g.Key.ItemId,
                ItemDescription = g.Key.Description,
                Quantity = g.Sum(x => x.l.Quantity),
                NetAmount = g.Sum(x => x.l.LineTotal - x.l.TaxAmount),
                TaxAmount = g.Sum(x => x.l.TaxAmount),
                LineTotal = g.Sum(x => x.l.LineTotal),
                InvoiceCount = g.Count()
            };

        var lines = await q.OrderByDescending(x => x.LineTotal).ToListAsync();
        lines.ForEach(l =>
        {
            l.Quantity = Math.Round(l.Quantity, 4);
            l.NetAmount = Math.Round(l.NetAmount, 2);
            l.TaxAmount = Math.Round(l.TaxAmount, 2);
            l.LineTotal = Math.Round(l.LineTotal, 2);
        });

        return new SalesSummaryByItemReportDto
        {
            FromDate = fromD,
            ToDate = toD,
            Lines = lines,
            TotalQuantity = Math.Round(lines.Sum(l => l.Quantity), 4),
            TotalAmount = Math.Round(lines.Sum(l => l.LineTotal), 2)
        };
    }

    public async Task<SalesCustomerItemReportDto> GetSalesSummaryCustomerItemAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
        => await BuildCustomerItemAsync(tenantId, fromDate, toDate, customerFirst: true);

    public async Task<SalesCustomerItemReportDto> GetSalesSummaryItemCustomerAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
        => await BuildCustomerItemAsync(tenantId, fromDate, toDate, customerFirst: false);

    private async Task<SalesCustomerItemReportDto> BuildCustomerItemAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, bool customerFirst)
    {
        var fromD = fromDate.Date;
        var toD = toDate.Date;
        var raw =
            from l in _db.SalesInvoiceLines.AsNoTracking()
            join i in _db.SalesInvoices.AsNoTracking() on l.SalesInvoiceId equals i.Id
            join c in _db.Customers.AsNoTracking() on i.CustomerId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            where (i.TenantId == tenantId)
                  && i.InvoiceDate >= fromD && i.InvoiceDate <= toD
                  && i.Status != "Voided"
            group new { l, i, c } by new
            {
                i.CustomerId,
                CustomerName = c != null ? (c.CompanyName ?? c.CustomerName) : null,
                l.ItemId,
                l.Description
            } into g
            select new SalesCustomerItemLineDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                ItemId = g.Key.ItemId,
                ItemDescription = g.Key.Description,
                Quantity = g.Sum(x => x.l.Quantity),
                LineTotal = g.Sum(x => x.l.LineTotal)
            };

        var lines = await raw.ToListAsync();
        lines = customerFirst
            ? lines.OrderBy(l => l.CustomerName).ThenBy(l => l.ItemDescription).ToList()
            : lines.OrderBy(l => l.ItemDescription).ThenBy(l => l.CustomerName).ToList();

        foreach (var l in lines)
        {
            l.Quantity = Math.Round(l.Quantity, 4);
            l.LineTotal = Math.Round(l.LineTotal, 2);
        }

        return new SalesCustomerItemReportDto
        {
            FromDate = fromD,
            ToDate = toD,
            Lines = lines,
            TotalAmount = Math.Round(lines.Sum(l => l.LineTotal), 2)
        };
    }

    public async Task<SalesItemRateDetailReportDto> GetSalesItemRateDetailAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate, Guid? itemId = null, Guid? customerId = null)
    {
        var fromD = fromDate.Date;
        var toD = toDate.Date;
        var q =
            from l in _db.SalesInvoiceLines.AsNoTracking()
            join i in _db.SalesInvoices.AsNoTracking() on l.SalesInvoiceId equals i.Id
            join c in _db.Customers.AsNoTracking() on i.CustomerId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            where (i.TenantId == tenantId)
                  && i.InvoiceDate >= fromD && i.InvoiceDate <= toD
                  && i.Status != "Voided"
                  && (!itemId.HasValue || itemId == Guid.Empty || l.ItemId == itemId)
                  && (!customerId.HasValue || customerId == Guid.Empty || i.CustomerId == customerId)
            orderby l.ItemId, i.InvoiceDate descending
            select new SalesItemRateDetailLineDto
            {
                ItemId = l.ItemId,
                ItemDescription = l.Description,
                CustomerId = i.CustomerId,
                CustomerName = c != null ? (c.CompanyName ?? c.CustomerName) : null,
                InvoiceDate = i.InvoiceDate,
                InvoiceNumber = i.InvoiceNumber,
                UnitPrice = l.UnitPrice,
                Quantity = l.Quantity,
                Tax1Rate = l.Tax1Rate != 0 ? l.Tax1Rate : l.TaxRate,
                Tax2Rate = l.Tax2Rate
            };

        return new SalesItemRateDetailReportDto
        {
            FromDate = fromD,
            ToDate = toD,
            ItemId = itemId,
            CustomerId = customerId,
            Lines = await q.ToListAsync()
        };
    }

    public async Task<CustomerInfoReportDto> GetCustomerInformationAsync(Guid tenantId, string? status = null)
    {
        var q = _db.Customers.AsNoTracking()
            .Include(c => c.Salesman)
            .Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(c => c.Status == status);

        var rows = await q.OrderBy(c => c.CustomerCode).ToListAsync();
        var lines = rows.Select(c => new CustomerInfoReportLineDto
        {
            CustomerId = c.Id,
            CustomerCode = c.CustomerCode,
            CustomerName = c.CustomerName,
            CompanyName = c.CompanyName,
            Phone = c.Phone,
            Email = c.Email,
            City = c.City,
            Status = c.Status,
            CreditLimit = c.CreditLimit,
            CreditDays = c.CreditDays,
            TaxType = c.TaxType,
            Ntn = c.Ntn,
            Strn = c.Strn,
            Cnic = c.Cnic,
            SalesmanId = c.SalesmanId,
            SalesmanName = c.Salesman?.FullName
        }).ToList();

        return new CustomerInfoReportDto { Lines = lines, CustomerCount = lines.Count };
    }

    public async Task<CreditNoteReportDto> GetCreditNoteReportAsync(Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var fromD = fromDate.Date;
        var toD = toDate.Date;
        var lines = await (
            from cn in _db.SalesCreditNotes.AsNoTracking()
            join c in _db.Customers.AsNoTracking() on cn.CustomerId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            where (cn.TenantId == tenantId)
                  && cn.CreditNoteDate >= fromD && cn.CreditNoteDate <= toD
                  && cn.Status != "Voided"
            orderby cn.CreditNoteDate, cn.CreditNoteNumber
            select new CreditNoteReportLineDto
            {
                CreditNoteId = cn.Id,
                CreditNoteNumber = cn.CreditNoteNumber,
                CreditNoteDate = cn.CreditNoteDate,
                CustomerId = cn.CustomerId,
                CustomerName = c != null ? (c.CompanyName ?? c.CustomerName) : null,
                Status = cn.Status,
                TotalAmount = cn.TotalAmount,
                OriginalSalesInvoiceId = cn.OriginalSalesInvoiceId
            }).ToListAsync();

        return new CreditNoteReportDto
        {
            FromDate = fromD,
            ToDate = toD,
            Lines = lines,
            Count = lines.Count,
            TotalAmount = Math.Round(lines.Sum(l => l.TotalAmount), 2)
        };
    }

    public async Task<PurchaseInvoiceSummaryReportDto> GetPurchaseInvoiceSummaryAsync(
        Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var fromD = fromDate.Date;
        var toD = toDate.Date;
        var lines = await (
            from pi in _db.PurchaseInvoices.AsNoTracking()
            join v in _db.Vendors.AsNoTracking() on pi.VendorId equals v.Id into vj
            from v in vj.DefaultIfEmpty()
            where (pi.TenantId == tenantId)
                  && pi.InvoiceDate >= fromD && pi.InvoiceDate <= toD
                  && pi.Status != "Cancelled"
            orderby pi.InvoiceDate, pi.InvoiceNumber
            select new PurchaseInvoiceSummaryLineDto
            {
                InvoiceId = pi.Id,
                InvoiceNumber = pi.InvoiceNumber,
                InvoiceDate = pi.InvoiceDate,
                VendorId = pi.VendorId,
                VendorName = v != null ? v.VendorName : null,
                Status = pi.Status,
                TotalAmount = pi.TotalAmount,
                TaxAmount = pi.TaxAmount,
                NetPayable = pi.NetPayable
            }).ToListAsync();

        return new PurchaseInvoiceSummaryReportDto
        {
            FromDate = fromD,
            ToDate = toD,
            Lines = lines,
            InvoiceCount = lines.Count,
            TotalAmount = Math.Round(lines.Sum(l => l.TotalAmount), 2)
        };
    }

    public async Task<StockBalanceReportDto> GetStockBalanceReportAsync(Guid tenantId, Guid? warehouseId = null)
    {
        var q =
            from s in _db.StockBalances.AsNoTracking()
            join i in _db.Items.AsNoTracking() on s.ItemId equals i.Id into ij
            from i in ij.DefaultIfEmpty()
            join w in _db.Warehouses.AsNoTracking() on s.WarehouseId equals w.Id into wj
            from w in wj.DefaultIfEmpty()
            where (s.TenantId == tenantId)
                  && (!warehouseId.HasValue || warehouseId == Guid.Empty || s.WarehouseId == warehouseId)
            orderby w != null ? w.WarehouseName : "", i != null ? i.ItemName : ""
            select new StockBalanceReportLineDto
            {
                ItemId = s.ItemId,
                ItemCode = i != null ? i.ItemCode : null,
                ItemName = i != null ? i.ItemName : null,
                WarehouseId = s.WarehouseId,
                WarehouseName = w != null ? w.WarehouseName : null,
                CurrentQuantity = s.CurrentQuantity,
                ReservedQuantity = s.ReservedQuantity,
                AvailableQuantity = s.CurrentQuantity - s.ReservedQuantity
            };

        var lines = await q.ToListAsync();
        return new StockBalanceReportDto
        {
            WarehouseId = warehouseId,
            Lines = lines,
            TotalQuantity = Math.Round(lines.Sum(l => l.CurrentQuantity), 4)
        };
    }

    public async Task<SalesOrderScheduleReportDto> GetSalesOrderScheduleAsync(
        Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty)
            return new SalesOrderScheduleReportDto { FromDate = fromDate, ToDate = toDate, Lines = [], LineCount = 0 };

        var rangeStart = fromDate?.Date;
        var rangeEnd = toDate?.Date;
        var q =
            from o in _db.SalesOrders.AsNoTracking()
            join l in _db.SalesOrderLines.AsNoTracking() on o.Id equals l.SalesOrderId
            join c in _db.Customers.AsNoTracking() on o.CustomerId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            join i in _db.Items.AsNoTracking() on l.ItemId equals i.Id into ij
            from i in ij.DefaultIfEmpty()
            where o.TenantId == tenantId
                  && o.Status != "Cancelled"
                  && (!rangeStart.HasValue || (o.RequiredDate ?? o.OrderDate) >= rangeStart.Value)
                  && (!rangeEnd.HasValue || (o.RequiredDate ?? o.OrderDate) <= rangeEnd.Value)
            orderby o.RequiredDate ?? o.OrderDate, o.OrderNumber
            select new SalesOrderScheduleLineDto
            {
                SalesOrderId = o.Id,
                SalesOrderLineId = l.Id,
                OrderNumber = string.IsNullOrWhiteSpace(o.SalesOrderNumber) ? o.OrderNumber : o.SalesOrderNumber,
                OrderDate = o.OrderDate,
                RequiredDate = o.RequiredDate,
                CustomerId = o.CustomerId,
                CustomerName = c != null ? (c.CompanyName ?? c.CustomerName) : null,
                ItemId = l.ItemId,
                ItemCode = i != null ? i.ItemCode : null,
                ItemName = i != null ? i.ItemName : l.Description,
                Quantity = l.Quantity,
                Status = o.Status
            };

        var lines = await q.ToListAsync();
        return new SalesOrderScheduleReportDto
        {
            FromDate = rangeStart,
            ToDate = rangeEnd,
            Lines = lines,
            LineCount = lines.Count
        };
    }
}
