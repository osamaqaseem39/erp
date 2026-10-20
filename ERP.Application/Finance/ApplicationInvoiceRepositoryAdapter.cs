using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

/// <summary>
/// Adapter implementing the Application-layer invoice repository contract against EF Core (tenant fail-closed).
/// </summary>
public class ApplicationInvoiceRepositoryAdapter : IInvoiceRepository
{
    private readonly ERPDbContext _db;

    public ApplicationInvoiceRepositoryAdapter(ERPDbContext db) => _db = db;

    private IQueryable<Invoice> InvoicesForTenant(Guid tenantId)
        => tenantId == Guid.Empty
            ? _db.Invoices.Where(_ => false)
            : _db.Invoices.Where(i => i.TenantId == tenantId);

    private IQueryable<Bill> BillsForTenant(Guid tenantId)
        => tenantId == Guid.Empty
            ? _db.Bills.Where(_ => false)
            : _db.Bills.Where(b => b.TenantId == tenantId);

    public async Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await InvoicesForTenant(tenantId).Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(
        Guid tenantId,
        Guid? customerId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Invoice>();

        var q = InvoicesForTenant(tenantId);
        if (customerId.HasValue) q = q.Where(i => i.CustomerId == customerId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(i => i.Status == status);
        if (fromDate.HasValue) q = q.Where(i => i.InvoiceDate >= fromDate);
        if (toDate.HasValue) q = q.Where(i => i.InvoiceDate <= toDate);
        return await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetCountAsync(
        Guid tenantId,
        Guid? customerId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty) return 0;

        var q = InvoicesForTenant(tenantId);
        if (customerId.HasValue) q = q.Where(i => i.CustomerId == customerId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(i => i.Status == status);
        if (fromDate.HasValue) q = q.Where(i => i.InvoiceDate >= fromDate);
        if (toDate.HasValue) q = q.Where(i => i.InvoiceDate <= toDate);
        return await q.CountAsync();
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await InvoicesForTenant(tenantId)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<Invoice> AddAsync(Invoice invoice)
    {
        if (invoice.TenantId == Guid.Empty)
            throw new InvalidOperationException("TenantId is required.");
        if (invoice.InvoiceId == Guid.Empty) invoice.InvoiceId = Guid.NewGuid();
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice)
    {
        _db.Invoices.Update(invoice);
        await _db.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(Guid invoiceId, Guid tenantId)
    {
        var entity = await GetByIdAsync(invoiceId, tenantId);
        if (entity == null) return false;
        _db.Invoices.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsLockedAsync(Guid invoiceId, Guid tenantId)
    {
        var invoice = await GetByIdAsync(invoiceId, tenantId);
        return invoice != null && (invoice.IsLocked || IsImmutableStatus(invoice.Status));
    }

    public async Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        var invoice = await GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;
        invoice.Status = "Posted";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GetCustomerEmailAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var email = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == customerId && c.TenantId == tenantId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync();
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public async Task<bool> ApplyPaymentAsync(Guid invoiceId, decimal amount, Guid tenantId)
    {
        if (amount <= 0) return false;
        var invoice = await GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;
        if (invoice.Balance <= 0) return false;

        var apply = Math.Min(amount, invoice.Balance);
        invoice.Balance = Math.Round(invoice.Balance - apply, 2);
        invoice.Status = invoice.Balance <= 0 ? "Paid" : "PartiallyPaid";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnapplyPaymentAsync(Guid invoiceId, decimal amount, Guid tenantId)
    {
        if (amount <= 0) return false;
        var invoice = await GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;

        var restore = Math.Min(amount, Math.Max(0, invoice.TotalAmount - invoice.Balance));
        if (restore <= 0) return false;
        invoice.Balance = Math.Round(Math.Min(invoice.TotalAmount, invoice.Balance + restore), 2);
        invoice.Status = invoice.Balance >= invoice.TotalAmount ? "Issued" :
            invoice.Balance <= 0 ? "Paid" : "PartiallyPaid";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<InvoiceLine>> GetLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return Array.Empty<InvoiceLine>();
        return await (
            from l in _db.InvoiceLines
            join i in InvoicesForTenant(tenantId) on l.InvoiceId equals i.InvoiceId
            where l.InvoiceId == invoiceId
            select l).ToListAsync();
    }

    public async Task<InvoiceLine?> GetLineByIdAsync(Guid invoiceLineId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await (
            from l in _db.InvoiceLines
            join i in InvoicesForTenant(tenantId) on l.InvoiceId equals i.InvoiceId
            where l.InvoiceLineId == invoiceLineId
            select l).FirstOrDefaultAsync();
    }

    public async Task<InvoiceLine> AddLineAsync(InvoiceLine invoiceLine)
    {
        if (invoiceLine.InvoiceLineId == Guid.Empty) invoiceLine.InvoiceLineId = Guid.NewGuid();
        _db.InvoiceLines.Add(invoiceLine);
        await _db.SaveChangesAsync();
        return invoiceLine;
    }

    public async Task<InvoiceLine> UpdateLineAsync(InvoiceLine invoiceLine)
    {
        _db.InvoiceLines.Update(invoiceLine);
        await _db.SaveChangesAsync();
        return invoiceLine;
    }

    public async Task<bool> DeleteLineAsync(Guid invoiceLineId, Guid tenantId)
    {
        var line = await GetLineByIdAsync(invoiceLineId, tenantId);
        if (line == null) return false;
        _db.InvoiceLines.Remove(line);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetSubTotalAsync(Guid invoiceId, Guid tenantId)
    {
        var lines = await GetLinesByInvoiceIdAsync(invoiceId, tenantId);
        return Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice), 2);
    }

    public async Task<decimal> GetTaxTotalAsync(Guid invoiceId, Guid tenantId)
    {
        var lines = await GetLinesByInvoiceIdAsync(invoiceId, tenantId);
        return Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice * l.TaxRate / 100m), 2);
    }

    public async Task<decimal> GetTotalAmountAsync(Guid invoiceId, Guid tenantId)
    {
        var lines = await GetLinesByInvoiceIdAsync(invoiceId, tenantId);
        return Math.Round(lines.Sum(l => l.LineTotal), 2);
    }

    public async Task<Bill?> GetBillByIdAsync(Guid billId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await BillsForTenant(tenantId)
            .FirstOrDefaultAsync(b => b.Id == billId || b.BillId == billId);
    }

    public async Task<IEnumerable<Bill>> GetAllBillsAsync(
        Guid tenantId,
        Guid? supplierId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Bill>();

        var q = BillsForTenant(tenantId);
        if (supplierId.HasValue) q = q.Where(b => b.SupplierId == supplierId || b.VendorId == supplierId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(b => b.Status == status);
        if (fromDate.HasValue) q = q.Where(b => b.BillDate >= fromDate);
        if (toDate.HasValue) q = q.Where(b => b.BillDate <= toDate);
        return await q.OrderByDescending(b => b.BillDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetBillCountAsync(
        Guid tenantId,
        Guid? supplierId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (tenantId == Guid.Empty) return 0;

        var q = BillsForTenant(tenantId);
        if (supplierId.HasValue) q = q.Where(b => b.SupplierId == supplierId || b.VendorId == supplierId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(b => b.Status == status);
        if (fromDate.HasValue) q = q.Where(b => b.BillDate >= fromDate);
        if (toDate.HasValue) q = q.Where(b => b.BillDate <= toDate);
        return await q.CountAsync();
    }

    public async Task<Bill?> GetBillByBillNumberAsync(string billNumber, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await BillsForTenant(tenantId)
            .FirstOrDefaultAsync(b => b.BillNumber == billNumber);
    }

    public async Task<Bill> AddBillAsync(Bill bill)
    {
        if (bill.TenantId == Guid.Empty)
            throw new InvalidOperationException("TenantId is required.");
        if (bill.Id == Guid.Empty) bill.Id = Guid.NewGuid();
        if (bill.BillId == Guid.Empty) bill.BillId = bill.Id;
        _db.Bills.Add(bill);
        await _db.SaveChangesAsync();
        return bill;
    }

    public async Task<Bill> UpdateBillAsync(Bill bill)
    {
        _db.Bills.Update(bill);
        await _db.SaveChangesAsync();
        return bill;
    }

    public async Task<bool> DeleteBillAsync(Guid billId, Guid tenantId)
    {
        var bill = await GetBillByIdAsync(billId, tenantId);
        if (bill == null) return false;
        if (bill.IsLocked || IsImmutableStatus(bill.Status)) return false;
        _db.Bills.Remove(bill);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsBillLockedAsync(Guid billId, Guid tenantId)
    {
        var bill = await GetBillByIdAsync(billId, tenantId);
        return bill != null && (bill.IsLocked || IsImmutableStatus(bill.Status));
    }

    public async Task<bool> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy)
    {
        var bill = await GetBillByIdAsync(billId, tenantId);
        if (bill == null) return false;
        if (IsImmutableStatus(bill.Status) && !string.Equals(bill.Status, "Issued", StringComparison.OrdinalIgnoreCase))
            return false;
        bill.Status = "Posted";
        bill.PostedAt = DateTime.UtcNow;
        bill.PostedBy = postedBy;
        bill.IsLocked = true;
        bill.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApplyPaymentToBillAsync(Guid billId, decimal amount, Guid tenantId)
    {
        if (amount <= 0) return false;
        var bill = await GetBillByIdAsync(billId, tenantId);
        if (bill == null) return false;
        if (bill.Balance <= 0) return false;

        var apply = Math.Min(amount, bill.Balance);
        bill.Balance = Math.Round(bill.Balance - apply, 2);
        bill.Status = bill.Balance <= 0 ? "Paid" : "PartiallyPaid";
        if (bill.Balance <= 0) bill.PaidDate = DateTime.UtcNow;
        bill.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnapplyPaymentFromBillAsync(Guid billId, decimal amount, Guid tenantId)
    {
        if (amount <= 0) return false;
        var bill = await GetBillByIdAsync(billId, tenantId);
        if (bill == null) return false;

        var restore = Math.Min(amount, Math.Max(0, bill.TotalAmount - bill.Balance));
        if (restore <= 0) return false;
        bill.Balance = Math.Round(Math.Min(bill.TotalAmount, bill.Balance + restore), 2);
        bill.Status = bill.Balance >= bill.TotalAmount ? "Issued" :
            bill.Balance <= 0 ? "Paid" : "PartiallyPaid";
        if (bill.Balance > 0) bill.PaidDate = null;
        bill.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BillLine>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId)
    {
        var bill = await GetBillByIdAsync(billId, tenantId);
        if (bill == null) return [];
        var keys = new[] { bill.BillId, bill.Id }.Where(x => x != Guid.Empty).Distinct().ToArray();
        return await _db.BillLines.Where(l => keys.Contains(l.BillId)).ToListAsync();
    }

    public async Task<BillLine?> GetBillLineByIdAsync(Guid billLineId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var line = await _db.BillLines.FirstOrDefaultAsync(l => l.BillLineId == billLineId);
        if (line == null) return null;
        var bill = await GetBillByIdAsync(line.BillId, tenantId);
        return bill == null ? null : line;
    }

    public async Task<BillLine> AddBillLineAsync(BillLine billLine)
    {
        if (billLine.BillLineId == Guid.Empty) billLine.BillLineId = Guid.NewGuid();
        _db.BillLines.Add(billLine);
        await _db.SaveChangesAsync();
        return billLine;
    }

    public async Task<BillLine> UpdateBillLineAsync(BillLine billLine)
    {
        _db.BillLines.Update(billLine);
        await _db.SaveChangesAsync();
        return billLine;
    }

    public async Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId)
    {
        var line = await GetBillLineByIdAsync(billLineId, tenantId);
        if (line == null) return false;
        _db.BillLines.Remove(line);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetBillSubTotalAsync(Guid billId, Guid tenantId)
    {
        var lines = await GetBillLinesByBillIdAsync(billId, tenantId);
        return Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice), 2);
    }

    public async Task<decimal> GetBillTaxTotalAsync(Guid billId, Guid tenantId)
    {
        var lines = await GetBillLinesByBillIdAsync(billId, tenantId);
        return Math.Round(lines.Sum(l => l.Quantity * l.UnitPrice * l.TaxRate / 100m), 2);
    }

    public async Task<decimal> GetBillTotalAmountAsync(Guid billId, Guid tenantId)
    {
        var lines = await GetBillLinesByBillIdAsync(billId, tenantId);
        return Math.Round(lines.Sum(l => l.LineTotal), 2);
    }

    private static bool IsImmutableStatus(string? status) =>
        string.Equals(status, "Posted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Voided", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "PartiallyPaid", StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<Invoice>> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[] agingBuckets)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Invoice>();
        return await InvoicesForTenant(tenantId)
            .Where(i => i.Balance > 0 && i.Status != "Cancelled" && i.Status != "Draft" && i.InvoiceDate <= asOfDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[] agingBuckets)
    {
        if (tenantId == Guid.Empty) return Array.Empty<Bill>();
        return await BillsForTenant(tenantId)
            .Where(b => b.Balance > 0 && b.Status != "Cancelled" && b.Status != "Draft" && b.BillDate <= asOfDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate)
    {
        var all = await GetAgedReceivablesAsync(tenantId, asOfDate, []);
        return all.Where(i => i.CustomerId == customerId);
    }

    public async Task<IEnumerable<Bill>> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate)
    {
        var all = await GetAgedPayablesAsync(tenantId, asOfDate, []);
        return all.Where(b => b.SupplierId == supplierId || b.VendorId == supplierId);
    }
}
