using ERP.Application.Interfaces.Finance;
using ERP.Shared.DTOs.Finance;
using ERP.Infrastructure.Repositories;
using ERP.Domain.Finance;

namespace ERP.Application.Finance;

public class InvoiceService : IInvoiceService
{
    private readonly ERP.Application.Interfaces.Finance.IInvoiceRepository _repository;
    private readonly IInvoiceDeliveryNotifier _deliveryNotifier;

    public InvoiceService(
        ERP.Application.Interfaces.Finance.IInvoiceRepository repository,
        IInvoiceDeliveryNotifier? deliveryNotifier = null)
    {
        _repository = repository;
        _deliveryNotifier = deliveryNotifier ?? new NullInvoiceDeliveryNotifier();
    }

    public async Task<Guid> AddInvoiceAsync(InvoiceCreateDto dto, Guid tenantId, Guid userId)
    {
        var entity = MapToEntity(dto, tenantId, userId);
        var createdEntity = await _repository.AddAsync(entity);
        return createdEntity.InvoiceId;
    }

    public async Task<Guid> UpdateInvoiceAsync(InvoiceUpdateDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return Guid.Empty;

        MapToEntity(dto, entity, userId);
        await _repository.UpdateAsync(entity);
        return entity.InvoiceId;
    }

    public async Task<bool> DeleteInvoiceAsync(Guid invoiceId, Guid tenantId)
    {
        return await _repository.DeleteAsync(invoiceId, tenantId);
    }

    public async Task<Guid> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        var success = await _repository.PostInvoiceAsync(invoiceId, tenantId, postedBy);
        return success ? invoiceId : Guid.Empty;
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, VoidInvoiceDto dto, Guid tenantId, Guid userId)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;
        if (string.Equals(invoice.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(invoice.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            return false;

        // Must void applied payments first — balance must equal open amount
        var applied = Math.Round(invoice.TotalAmount - invoice.Balance, 2);
        if (applied > 0.01m) return false;

        invoice.Status = "Cancelled";
        invoice.Balance = 0;
        invoice.IsLocked = true;
        invoice.IsActive = false;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
        await _repository.UpdateAsync(invoice);
        return true;
    }

    public async Task<bool> SendInvoiceAsync(Guid invoiceId, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;
        if (string.Equals(invoice.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(invoice.Status, "Voided", StringComparison.OrdinalIgnoreCase))
            return false;
        if (invoice.CustomerId == Guid.Empty) return false;

        var email = await _repository.GetCustomerEmailAsync(invoice.CustomerId, tenantId);
        if (string.IsNullOrWhiteSpace(email)) return false;

        await _deliveryNotifier.NotifyAsync(new InvoiceDeliveryRequest(
            invoice.InvoiceId,
            invoice.InvoiceNumber,
            tenantId,
            invoice.CustomerId,
            email,
            invoice.TotalAmount,
            string.IsNullOrWhiteSpace(invoice.CurrencyCode) ? "USD" : invoice.CurrencyCode,
            invoice.InvoiceDate,
            invoice.DueDate), ct);

        if (string.Equals(invoice.Status, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            invoice.Status = "Issued";
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedBy = userId == Guid.Empty ? null : userId.ToString();
            await _repository.UpdateAsync(invoice);
        }

        return true;
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId)
    {
        var entity = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return null;
        var dto = MapToDto(entity);
        var lines = await _repository.GetLinesByInvoiceIdAsync(invoiceId, tenantId);
        dto.InvoiceLines = lines.Select(MapLineToDto).ToList();
        return dto;
    }

    public async Task<(IEnumerable<InvoiceDto> Invoices, int TotalCount)> SearchInvoicesAsync(InvoiceQuery query)
    {
        var entities = await _repository.GetAllAsync(query.TenantId, query.CustomerId, query.Status, query.StartDate, query.EndDate, query.PageNumber, query.PageSize);
        var totalCount = await _repository.GetCountAsync(query.TenantId, query.CustomerId, query.Status, query.StartDate, query.EndDate);

        return (entities.Select(MapToDto), totalCount);
    }

    public async Task<Guid> AddInvoiceLineAsync(InvoiceLineCreateDto dto, Guid invoiceId, Guid tenantId, Guid userId)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (invoice == null || !CanEditDocument(invoice.Status, invoice.IsLocked)) return Guid.Empty;
        if (dto.GLAccountId == Guid.Empty || dto.Quantity <= 0) return Guid.Empty;

        var net = Math.Round(dto.Quantity * dto.UnitPrice, 2);
        var tax = Math.Round(net * dto.TaxRate / 100m, 2);
        var line = new InvoiceLine
        {
            InvoiceLineId = Guid.NewGuid(),
            InvoiceId = invoiceId,
            ProductId = dto.ItemId == Guid.Empty ? null : dto.ItemId,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? "Line" : dto.Description!,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TaxRate = dto.TaxRate,
            LineTotal = net + tax,
            GLAccountId = dto.GLAccountId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId
        };
        await _repository.AddLineAsync(line);
        await RecalcInvoiceTotalsAsync(invoiceId, tenantId);
        return line.InvoiceLineId;
    }

    public async Task<Guid> UpdateInvoiceLineAsync(InvoiceLineUpdateDto dto, Guid tenantId, Guid userId)
    {
        var lineId = dto.Id;
        var line = await _repository.GetLineByIdAsync(lineId, tenantId);
        if (line == null) return Guid.Empty;
        var invoice = await _repository.GetByIdAsync(line.InvoiceId, tenantId);
        if (invoice == null || !CanEditDocument(invoice.Status, invoice.IsLocked)) return Guid.Empty;

        if (dto.ItemId.HasValue) line.ProductId = dto.ItemId == Guid.Empty ? null : dto.ItemId;
        if (dto.Description != null) line.Description = string.IsNullOrWhiteSpace(dto.Description) ? line.Description : dto.Description;
        if (dto.Quantity.HasValue) line.Quantity = dto.Quantity.Value;
        if (dto.UnitPrice.HasValue) line.UnitPrice = dto.UnitPrice.Value;
        if (dto.TaxRate.HasValue) line.TaxRate = dto.TaxRate.Value;
        if (dto.GLAccountId.HasValue && dto.GLAccountId != Guid.Empty) line.GLAccountId = dto.GLAccountId.Value;

        var net = Math.Round(line.Quantity * line.UnitPrice, 2);
        var tax = Math.Round(net * line.TaxRate / 100m, 2);
        line.LineTotal = net + tax;
        await _repository.UpdateLineAsync(line);
        await RecalcInvoiceTotalsAsync(line.InvoiceId, tenantId);
        return line.InvoiceLineId;
    }

    public async Task<bool> DeleteInvoiceLineAsync(Guid invoiceLineId, Guid tenantId)
    {
        var line = await _repository.GetLineByIdAsync(invoiceLineId, tenantId);
        if (line == null) return false;
        var invoice = await _repository.GetByIdAsync(line.InvoiceId, tenantId);
        if (invoice == null || !CanEditDocument(invoice.Status, invoice.IsLocked)) return false;
        var invoiceId = line.InvoiceId;
        var ok = await _repository.DeleteLineAsync(invoiceLineId, tenantId);
        if (ok) await RecalcInvoiceTotalsAsync(invoiceId, tenantId);
        return ok;
    }

    public async Task<IEnumerable<InvoiceLineDto>> GetInvoiceLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return [];
        var lines = await _repository.GetLinesByInvoiceIdAsync(invoiceId, tenantId);
        return lines.Select(MapLineToDto);
    }

    public async Task<Guid> AddBillAsync(BillCreateDto dto, Guid tenantId, Guid userId)
    {
        if (dto.VendorId == Guid.Empty) return Guid.Empty;
        var id = Guid.NewGuid();
        var total = dto.TotalAmount;
        var bill = new Bill
        {
            Id = id,
            BillId = id,
            TenantId = tenantId,
            CompanyId = dto.CompanyId,
            VendorId = dto.VendorId,
            SupplierId = dto.VendorId,
            BillNumber = string.IsNullOrWhiteSpace(dto.BillNumber)
                ? $"BILL-{DateTime.UtcNow:yyyyMMdd}-{id:N}"[..20].ToUpperInvariant()
                : dto.BillNumber,
            BillDate = dto.BillDate == default ? DateTime.UtcNow : dto.BillDate,
            DueDate = dto.DueDate == default ? DateTime.UtcNow.AddDays(30) : dto.DueDate,
            CurrencyCode = dto.CurrencyCode ?? "USD",
            SubTotal = dto.SubTotal,
            TaxAmount = dto.TaxAmount,
            TaxTotal = dto.TaxAmount,
            TotalAmount = total,
            Balance = total,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status,
            PurchaseOrderId = dto.PurchaseOrderId,
            Notes = dto.Notes ?? string.Empty,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsActive = dto.IsActive
        };
        await _repository.AddBillAsync(bill);
        return bill.BillId;
    }

    public async Task<Guid> UpdateBillAsync(BillUpdateDto dto, Guid tenantId, Guid userId)
    {
        var billId = dto.BillId != Guid.Empty ? dto.BillId : dto.Id;
        var bill = await _repository.GetBillByIdAsync(billId, tenantId);
        if (bill == null || !CanEditDocument(bill.Status, bill.IsLocked)) return Guid.Empty;

        if (!string.IsNullOrWhiteSpace(dto.BillNumber)) bill.BillNumber = dto.BillNumber;
        if (dto.VendorId.HasValue)
        {
            bill.VendorId = dto.VendorId.Value;
            bill.SupplierId = dto.VendorId.Value;
        }
        if (dto.BillDate.HasValue) bill.BillDate = dto.BillDate.Value;
        if (dto.DueDate.HasValue) bill.DueDate = dto.DueDate.Value;
        if (dto.SubTotal.HasValue) bill.SubTotal = dto.SubTotal.Value;
        if (dto.TaxAmount.HasValue)
        {
            bill.TaxAmount = dto.TaxAmount.Value;
            bill.TaxTotal = dto.TaxAmount.Value;
        }
        if (dto.TotalAmount.HasValue)
        {
            var paid = Math.Round(bill.TotalAmount - bill.Balance, 2);
            bill.TotalAmount = dto.TotalAmount.Value;
            bill.Balance = Math.Max(0, Math.Round(bill.TotalAmount - paid, 2));
        }
        if (!string.IsNullOrWhiteSpace(dto.Status)) bill.Status = dto.Status;
        if (dto.Notes != null) bill.Notes = dto.Notes;
        if (!string.IsNullOrWhiteSpace(dto.CurrencyCode)) bill.CurrencyCode = dto.CurrencyCode;
        if (dto.IsActive.HasValue) bill.IsActive = dto.IsActive.Value;
        bill.UpdatedBy = userId.ToString();
        bill.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateBillAsync(bill);
        return bill.BillId != Guid.Empty ? bill.BillId : bill.Id;
    }

    public async Task<bool> DeleteBillAsync(Guid billId, Guid tenantId)
        => await _repository.DeleteBillAsync(billId, tenantId);

    public async Task<Guid> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy)
    {
        var ok = await _repository.PostBillAsync(billId, tenantId, postedBy);
        return ok ? billId : Guid.Empty;
    }

    public async Task<BillDto?> GetBillByIdAsync(Guid billId, Guid tenantId)
    {
        var entity = await _repository.GetBillByIdAsync(billId, tenantId);
        return entity == null ? null : MapBillToDto(entity);
    }

    public async Task<(IEnumerable<BillDto> Bills, int TotalCount)> SearchBillsAsync(BillQuery query)
    {
        var entities = await _repository.GetAllBillsAsync(
            query.TenantId, query.VendorId, query.Status, query.FromDate, query.ToDate, query.PageNumber, query.PageSize);
        var total = await _repository.GetBillCountAsync(
            query.TenantId, query.VendorId, query.Status, query.FromDate, query.ToDate);
        return (entities.Select(MapBillToDto), total);
    }

    public async Task<Guid> AddBillLineAsync(BillLineCreateDto dto, Guid billId, Guid tenantId, Guid userId)
    {
        var bill = await _repository.GetBillByIdAsync(billId, tenantId);
        if (bill == null || !CanEditDocument(bill.Status, bill.IsLocked)) return Guid.Empty;
        if (dto.GLAccountId == Guid.Empty || dto.Quantity <= 0) return Guid.Empty;

        var key = bill.BillId != Guid.Empty ? bill.BillId : bill.Id;
        var net = Math.Round(dto.Quantity * dto.UnitPrice, 2);
        var tax = Math.Round(net * dto.TaxRate / 100m, 2);
        var line = new BillLine
        {
            BillLineId = Guid.NewGuid(),
            BillId = key,
            ProductId = dto.ItemId == Guid.Empty ? null : dto.ItemId,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? "Line" : dto.Description!,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TaxRate = dto.TaxRate,
            LineTotal = net + tax,
            GLAccountId = dto.GLAccountId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId == Guid.Empty ? null : userId
        };
        await _repository.AddBillLineAsync(line);
        await RecalcBillTotalsAsync(key, tenantId);
        return line.BillLineId;
    }

    public async Task<Guid> UpdateBillLineAsync(BillLineUpdateDto dto, Guid tenantId, Guid userId)
    {
        var line = await _repository.GetBillLineByIdAsync(dto.Id, tenantId);
        if (line == null) return Guid.Empty;
        var bill = await _repository.GetBillByIdAsync(line.BillId, tenantId);
        if (bill == null || !CanEditDocument(bill.Status, bill.IsLocked)) return Guid.Empty;

        if (dto.ItemId.HasValue) line.ProductId = dto.ItemId == Guid.Empty ? null : dto.ItemId;
        if (dto.Description != null) line.Description = string.IsNullOrWhiteSpace(dto.Description) ? line.Description : dto.Description;
        if (dto.Quantity.HasValue) line.Quantity = dto.Quantity.Value;
        if (dto.UnitPrice.HasValue) line.UnitPrice = dto.UnitPrice.Value;
        if (dto.TaxRate.HasValue) line.TaxRate = dto.TaxRate.Value;
        if (dto.GLAccountId.HasValue && dto.GLAccountId != Guid.Empty) line.GLAccountId = dto.GLAccountId.Value;

        var net = Math.Round(line.Quantity * line.UnitPrice, 2);
        var tax = Math.Round(net * line.TaxRate / 100m, 2);
        line.LineTotal = net + tax;
        await _repository.UpdateBillLineAsync(line);
        await RecalcBillTotalsAsync(line.BillId, tenantId);
        return line.BillLineId;
    }

    public async Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId)
    {
        var line = await _repository.GetBillLineByIdAsync(billLineId, tenantId);
        if (line == null) return false;
        var bill = await _repository.GetBillByIdAsync(line.BillId, tenantId);
        if (bill == null || !CanEditDocument(bill.Status, bill.IsLocked)) return false;
        var billId = line.BillId;
        var ok = await _repository.DeleteBillLineAsync(billLineId, tenantId);
        if (ok) await RecalcBillTotalsAsync(billId, tenantId);
        return ok;
    }

    public async Task<IEnumerable<BillLineDto>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId)
    {
        var bill = await _repository.GetBillByIdAsync(billId, tenantId);
        if (bill == null) return [];
        var lines = await _repository.GetBillLinesByBillIdAsync(billId, tenantId);
        return lines.Select(MapBillLineToDto);
    }

    public async Task<bool> ApplyPaymentToInvoiceAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        if (dto.Amount <= 0 || dto.InvoiceId == Guid.Empty) return false;
        var invoice = await _repository.GetByIdAsync(dto.InvoiceId, tenantId);
        if (invoice == null || invoice.Balance <= 0) return false;
        return await _repository.ApplyPaymentAsync(dto.InvoiceId, dto.Amount, tenantId);
    }

    public async Task<bool> ApplyPaymentToBillAsync(PaymentApplicationDto dto, Guid tenantId, Guid userId)
    {
        var billId = dto.BillId != Guid.Empty ? dto.BillId : dto.InvoiceId;
        if (dto.Amount <= 0 || billId == Guid.Empty) return false;
        return await _repository.ApplyPaymentToBillAsync(billId, dto.Amount, tenantId);
    }

    public Task<bool> ApplyCreditToInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId)
        => ApplyPaymentToInvoiceAsync(new PaymentApplicationDto { InvoiceId = invoiceId, Amount = amount }, tenantId, Guid.Empty);

    public async Task<bool> UnapplyCreditFromInvoiceAsync(Guid invoiceId, decimal amount, Guid tenantId)
    {
        if (amount <= 0 || invoiceId == Guid.Empty) return false;
        return await _repository.UnapplyPaymentAsync(invoiceId, amount, tenantId);
    }

    public async Task<AgingReportDto> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        var invoices = (await _repository.GetAllAsync(tenantId, pageNumber: 1, pageSize: 50_000))
            .Where(i => i.Balance > 0
                        && !string.Equals(i.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(i.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            .ToList();
        return BuildAging(
            "Receivables",
            asOfDate,
            agingBuckets,
            invoices.Select(i => (
                i.InvoiceId,
                i.InvoiceNumber,
                i.CustomerId,
                i.InvoiceDate,
                i.DueDate,
                i.Balance)));
    }

    public async Task<AgingReportDto> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        var bills = (await _repository.GetAllBillsAsync(tenantId, pageNumber: 1, pageSize: 50_000))
            .Where(b => b.Balance > 0
                        && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(b.Status, "Draft", StringComparison.OrdinalIgnoreCase))
            .ToList();
        return BuildAging(
            "Payables",
            asOfDate,
            agingBuckets,
            bills.Select(b => (
                b.BillId != Guid.Empty ? b.BillId : b.Id,
                b.BillNumber,
                b.SupplierId != Guid.Empty ? b.SupplierId : b.VendorId,
                b.BillDate,
                b.DueDate,
                b.Balance)));
    }

    public async Task<AgingReportDto> GetCustomerAgingAsync(
        Guid customerId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        var report = await GetAgedReceivablesAsync(tenantId, asOfDate, agingBuckets);
        report.Lines = report.Lines.Where(l => l.PartnerId == customerId).ToList();
        RebuildBucketTotals(report);
        return report;
    }

    public async Task<AgingReportDto> GetSupplierAgingAsync(
        Guid supplierId, Guid tenantId, DateTime asOfDate, int[]? agingBuckets = null)
    {
        var report = await GetAgedPayablesAsync(tenantId, asOfDate, agingBuckets);
        report.Lines = report.Lines.Where(l => l.PartnerId == supplierId).ToList();
        RebuildBucketTotals(report);
        return report;
    }

    public async Task<PartnerBalanceSummaryReportDto> GetPartnerBalanceSummaryAsync(Guid tenantId, DateTime asOfDate)
    {
        var invoices = (await _repository.GetAllAsync(tenantId, pageNumber: 1, pageSize: 50_000))
            .Where(i => i.Balance > 0
                        && !string.Equals(i.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(i.Status, "Draft", StringComparison.OrdinalIgnoreCase)
                        && i.InvoiceDate <= asOfDate)
            .ToList();
        var bills = (await _repository.GetAllBillsAsync(tenantId, pageNumber: 1, pageSize: 50_000))
            .Where(b => b.Balance > 0
                        && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(b.Status, "Draft", StringComparison.OrdinalIgnoreCase)
                        && b.BillDate <= asOfDate)
            .ToList();

        var customers = invoices
            .GroupBy(i => i.CustomerId)
            .Select(g => new PartnerBalanceLineDto
            {
                PartnerId = g.Key,
                PartnerKind = "Customer",
                OpenDocuments = g.Count(),
                TotalInvoiced = Math.Round(g.Sum(x => x.TotalAmount), 2),
                Balance = Math.Round(g.Sum(x => x.Balance), 2)
            })
            .OrderByDescending(x => x.Balance)
            .ToList();

        var suppliers = bills
            .GroupBy(b => b.SupplierId != Guid.Empty ? b.SupplierId : b.VendorId)
            .Select(g => new PartnerBalanceLineDto
            {
                PartnerId = g.Key,
                PartnerKind = "Supplier",
                OpenDocuments = g.Count(),
                TotalInvoiced = Math.Round(g.Sum(x => x.TotalAmount), 2),
                Balance = Math.Round(g.Sum(x => x.Balance), 2)
            })
            .OrderByDescending(x => x.Balance)
            .ToList();

        return new PartnerBalanceSummaryReportDto
        {
            AsOfDate = asOfDate.Date,
            Customers = customers,
            Suppliers = suppliers,
            TotalReceivables = Math.Round(customers.Sum(c => c.Balance), 2),
            TotalPayables = Math.Round(suppliers.Sum(s => s.Balance), 2)
        };
    }

    private static AgingReportDto BuildAging(
        string kind,
        DateTime asOfDate,
        int[]? agingBuckets,
        IEnumerable<(Guid Id, string Number, Guid PartnerId, DateTime DocDate, DateTime DueDate, decimal Balance)> docs)
    {
        var cuts = NormalizeBuckets(agingBuckets);
        var bucketDefs = BuildBucketDefs(cuts);
        var lines = new List<AgingDocumentLineDto>();

        foreach (var d in docs)
        {
            var days = (asOfDate.Date - d.DueDate.Date).Days;
            var label = ResolveBucketLabel(days, bucketDefs);
            lines.Add(new AgingDocumentLineDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.Number,
                PartnerId = d.PartnerId,
                DocumentDate = d.DocDate,
                DueDate = d.DueDate,
                DaysOverdue = days,
                Bucket = label,
                Balance = Math.Round(d.Balance, 2)
            });
        }

        var report = new AgingReportDto
        {
            AsOfDate = asOfDate.Date,
            Kind = kind,
            BucketsUsed = cuts,
            Lines = lines.OrderByDescending(l => l.DaysOverdue).ToList(),
            Buckets = bucketDefs.Select(b => new AgingBucketDto
            {
                Label = b.Label,
                FromDays = b.FromDays,
                ToDays = b.ToDays
            }).ToList()
        };
        RebuildBucketTotals(report);
        return report;
    }

    private static void RebuildBucketTotals(AgingReportDto report)
    {
        foreach (var bucket in report.Buckets)
        {
            var inBucket = report.Lines.Where(l => l.Bucket == bucket.Label).ToList();
            bucket.Amount = Math.Round(inBucket.Sum(l => l.Balance), 2);
            bucket.Count = inBucket.Count;
        }
        report.TotalBalance = Math.Round(report.Lines.Sum(l => l.Balance), 2);
    }

    private static int[] NormalizeBuckets(int[]? agingBuckets)
    {
        var cuts = (agingBuckets is { Length: > 0 } ? agingBuckets : new[] { 30, 60, 90 })
            .Where(x => x > 0)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
        return cuts.Length > 0 ? cuts : new[] { 30, 60, 90 };
    }

    private static List<(string Label, int? FromDays, int? ToDays)> BuildBucketDefs(int[] cuts)
    {
        var defs = new List<(string, int?, int?)> { ("Current", null, 0) };
        var prev = 1;
        foreach (var cut in cuts)
        {
            defs.Add(($"{prev}-{cut}", prev, cut));
            prev = cut + 1;
        }
        defs.Add(($"Over {cuts[^1]}", cuts[^1] + 1, null));
        return defs;
    }

    private static string ResolveBucketLabel(int daysOverdue, List<(string Label, int? FromDays, int? ToDays)> defs)
    {
        foreach (var d in defs)
        {
            var fromOk = !d.FromDays.HasValue || daysOverdue >= d.FromDays.Value;
            var toOk = !d.ToDays.HasValue || daysOverdue <= d.ToDays.Value;
            if (fromOk && toOk) return d.Label;
        }
        return defs[^1].Label;
    }

    private async Task RecalcInvoiceTotalsAsync(Guid invoiceId, Guid tenantId)
    {
        var invoice = await _repository.GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return;
        var lines = (await _repository.GetLinesByInvoiceIdAsync(invoiceId, tenantId)).ToList();
        var sub = Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice), 2);
        var tax = Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice * l.TaxRate / 100m), 2);
        var total = Math.Round(lines.Sum(l => l.LineTotal), 2);
        var paid = Math.Round(invoice.TotalAmount - invoice.Balance, 2);
        invoice.SubTotal = sub;
        invoice.TaxTotal = tax;
        invoice.TotalAmount = total;
        invoice.Balance = Math.Max(0, Math.Round(total - paid, 2));
        invoice.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(invoice);
    }

    private async Task RecalcBillTotalsAsync(Guid billId, Guid tenantId)
    {
        var bill = await _repository.GetBillByIdAsync(billId, tenantId);
        if (bill == null) return;
        var lines = (await _repository.GetBillLinesByBillIdAsync(billId, tenantId)).ToList();
        var sub = Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice), 2);
        var tax = Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice * l.TaxRate / 100m), 2);
        var total = Math.Round(lines.Sum(l => l.LineTotal), 2);
        var paid = Math.Round(bill.TotalAmount - bill.Balance, 2);
        bill.SubTotal = sub;
        bill.TaxAmount = tax;
        bill.TaxTotal = tax;
        bill.TotalAmount = total;
        bill.Balance = Math.Max(0, Math.Round(total - paid, 2));
        bill.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateBillAsync(bill);
    }

    private static bool CanEditDocument(string? status, bool isLocked)
    {
        if (isLocked) return false;
        return string.IsNullOrWhiteSpace(status)
               || string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase)
               || string.Equals(status, "Issued", StringComparison.OrdinalIgnoreCase);
    }

    private static InvoiceLineDto MapLineToDto(InvoiceLine line)
    {
        var net = Math.Round(line.Quantity * line.UnitPrice, 2);
        var tax = Math.Round(net * line.TaxRate / 100m, 2);
        return new InvoiceLineDto
        {
            Id = line.InvoiceLineId,
            InvoiceId = line.InvoiceId,
            ProductId = line.ProductId,
            Description = line.Description,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            TotalPrice = line.LineTotal,
            TaxRate = line.TaxRate,
            TaxAmount = tax,
            GLAccountId = line.GLAccountId
        };
    }

    private static BillDto MapBillToDto(Bill entity) => new()
    {
        Id = entity.Id != Guid.Empty ? entity.Id : entity.BillId,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        BillNumber = entity.BillNumber,
        VendorId = entity.VendorId != Guid.Empty ? entity.VendorId : entity.SupplierId,
        BillDate = entity.BillDate,
        DueDate = entity.DueDate,
        SubTotal = entity.SubTotal,
        TaxAmount = entity.TaxAmount != 0 ? entity.TaxAmount : entity.TaxTotal,
        TotalAmount = entity.TotalAmount,
        Balance = entity.Balance,
        Status = entity.Status,
        Notes = entity.Notes,
        ApprovedDate = entity.ApprovedDate,
        PaidDate = entity.PaidDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive
    };

    private static BillLineDto MapBillLineToDto(BillLine line)
    {
        var net = Math.Round(line.Quantity * line.UnitPrice, 2);
        var tax = Math.Round(net * line.TaxRate / 100m, 2);
        return new BillLineDto
        {
            Id = line.BillLineId,
            BillLineId = line.BillLineId,
            BillId = line.BillId,
            ItemId = line.ProductId ?? Guid.Empty,
            Description = line.Description,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            TaxRate = line.TaxRate,
            TaxAmount = tax,
            LineTotal = line.LineTotal
        };
    }

    private static InvoiceDto MapToDto(Invoice entity)
    {
        return new InvoiceDto
        {
            InvoiceId = entity.InvoiceId,
            TenantId = entity.TenantId,
            CustomerId = entity.CustomerId,
            InvoiceNumber = entity.InvoiceNumber,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            CurrencyCode = entity.CurrencyCode,
            SubTotal = entity.SubTotal,
            TaxTotal = entity.TaxTotal,
            TotalAmount = entity.TotalAmount,
            Balance = entity.Balance,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            SalesOrderId = entity.SalesOrderId,
            PostedAt = entity.PostedAt,
            PostedBy = entity.PostedBy,
            IsLocked = entity.IsLocked,
            TaxAmount = entity.TaxTotal,
            BalanceAmount = entity.Balance
        };
    }

    private static Invoice MapToEntity(InvoiceCreateDto dto, Guid tenantId, Guid userId)
    {
        var invoiceId = Guid.NewGuid();
        var total = dto.TotalAmount ?? 0m;
        return new Invoice
        {
            Id = invoiceId,
            InvoiceId = invoiceId,
            TenantId = tenantId,
            CustomerId = dto.CustomerId,
            InvoiceNumber = dto.InvoiceNumber ?? string.Empty,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(30),
            CurrencyCode = dto.CurrencyCode ?? "USD",
            SubTotal = dto.SubTotal ?? 0m,
            TaxTotal = dto.TaxAmount ?? 0m,
            TotalAmount = total,
            Balance = total,
            Status = dto.Status ?? "Draft",
            SalesOrderId = dto.SalesOrderId,
            IsLocked = dto.IsLocked ?? false,
            CreatedBy = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void MapToEntity(InvoiceUpdateDto dto, Invoice entity, Guid userId)
    {
        if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId.Value;
        entity.InvoiceNumber = dto.InvoiceNumber ?? entity.InvoiceNumber;
        entity.InvoiceDate = dto.InvoiceDate ?? entity.InvoiceDate;
        entity.DueDate = dto.DueDate ?? entity.DueDate;
        entity.CurrencyCode = dto.CurrencyCode ?? entity.CurrencyCode;
        entity.Status = dto.Status ?? entity.Status;
        if (dto.SalesOrderId.HasValue) entity.SalesOrderId = dto.SalesOrderId.Value;
        entity.IsLocked = dto.IsLocked ?? entity.IsLocked;
        entity.UpdatedBy = userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}