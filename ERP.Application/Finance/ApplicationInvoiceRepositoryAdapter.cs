using ERP.Application.Interfaces.Finance;
using ERP.Domain.Finance;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Finance;

/// <summary>
/// Adapter implementing the Application-layer invoice repository contract against EF Core.
/// </summary>
public class ApplicationInvoiceRepositoryAdapter : IInvoiceRepository
{
    private readonly ERPDbContext _db;

    public ApplicationInvoiceRepositoryAdapter(ERPDbContext db) => _db = db;

    public async Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid tenantId)
        => await _db.Invoices.Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && (tenantId == Guid.Empty || i.TenantId == tenantId));

    public async Task<IEnumerable<Invoice>> GetAllAsync(Guid tenantId, Guid? customerId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
    {
        var q = _db.Invoices.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(i => i.TenantId == tenantId);
        if (customerId.HasValue) q = q.Where(i => i.CustomerId == customerId);
        if (!string.IsNullOrEmpty(status)) q = q.Where(i => i.Status == status);
        if (fromDate.HasValue) q = q.Where(i => i.InvoiceDate >= fromDate);
        if (toDate.HasValue) q = q.Where(i => i.InvoiceDate <= toDate);
        return await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetCountAsync(Guid tenantId, Guid? customerId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var q = _db.Invoices.AsQueryable();
        if (tenantId != Guid.Empty) q = q.Where(i => i.TenantId == tenantId);
        return await q.CountAsync();
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, Guid tenantId)
        => await _db.Invoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && i.TenantId == tenantId);

    public async Task<Invoice> AddAsync(Invoice invoice)
    {
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

    public Task<bool> IsLockedAsync(Guid invoiceId, Guid tenantId) => Task.FromResult(false);

    public async Task<bool> PostInvoiceAsync(Guid invoiceId, Guid tenantId, Guid postedBy)
    {
        var invoice = await GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;
        invoice.Status = "Posted";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApplyPaymentAsync(Guid invoiceId, decimal amount, Guid tenantId)
    {
        var invoice = await GetByIdAsync(invoiceId, tenantId);
        if (invoice == null) return false;
        invoice.Balance = Math.Max(0, invoice.Balance - amount);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<InvoiceLine>> GetLinesByInvoiceIdAsync(Guid invoiceId, Guid tenantId)
        => await _db.Set<InvoiceLine>().Where(l => l.InvoiceId == invoiceId).ToListAsync();

    public async Task<InvoiceLine?> GetLineByIdAsync(Guid invoiceLineId, Guid tenantId)
        => await _db.Set<InvoiceLine>().FirstOrDefaultAsync(l => l.InvoiceLineId == invoiceLineId);

    public async Task<InvoiceLine> AddLineAsync(InvoiceLine invoiceLine)
    {
        _db.Set<InvoiceLine>().Add(invoiceLine);
        await _db.SaveChangesAsync();
        return invoiceLine;
    }

    public async Task<InvoiceLine> UpdateLineAsync(InvoiceLine invoiceLine)
    {
        _db.Set<InvoiceLine>().Update(invoiceLine);
        await _db.SaveChangesAsync();
        return invoiceLine;
    }

    public async Task<bool> DeleteLineAsync(Guid invoiceLineId, Guid tenantId)
    {
        var line = await GetLineByIdAsync(invoiceLineId, tenantId);
        if (line == null) return false;
        _db.Set<InvoiceLine>().Remove(line);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<decimal> GetSubTotalAsync(Guid invoiceId, Guid tenantId) => Task.FromResult(0m);
    public Task<decimal> GetTaxTotalAsync(Guid invoiceId, Guid tenantId) => Task.FromResult(0m);
    public Task<decimal> GetTotalAmountAsync(Guid invoiceId, Guid tenantId) => Task.FromResult(0m);

    public async Task<Bill?> GetBillByIdAsync(Guid billId, Guid tenantId)
        => await _db.Bills.FirstOrDefaultAsync(b => b.BillId == billId);

    public async Task<IEnumerable<Bill>> GetAllBillsAsync(Guid tenantId, Guid? supplierId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
        => await _db.Bills.Take(pageSize).ToListAsync();

    public Task<int> GetBillCountAsync(Guid tenantId, Guid? supplierId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        => Task.FromResult(0);

    public Task<Bill?> GetBillByBillNumberAsync(string billNumber, Guid tenantId) => Task.FromResult<Bill?>(null);

    public async Task<Bill> AddBillAsync(Bill bill)
    {
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

    public Task<bool> DeleteBillAsync(Guid billId, Guid tenantId) => Task.FromResult(false);
    public Task<bool> IsBillLockedAsync(Guid billId, Guid tenantId) => Task.FromResult(false);
    public Task<bool> PostBillAsync(Guid billId, Guid tenantId, Guid postedBy) => Task.FromResult(false);
    public Task<bool> ApplyPaymentToBillAsync(Guid billId, decimal amount, Guid tenantId) => Task.FromResult(false);

    public Task<IEnumerable<BillLine>> GetBillLinesByBillIdAsync(Guid billId, Guid tenantId)
        => Task.FromResult(Enumerable.Empty<BillLine>());
    public Task<BillLine?> GetBillLineByIdAsync(Guid billLineId, Guid tenantId) => Task.FromResult<BillLine?>(null);
    public Task<BillLine> AddBillLineAsync(BillLine billLine) => throw new NotImplementedException();
    public Task<BillLine> UpdateBillLineAsync(BillLine billLine) => throw new NotImplementedException();
    public Task<bool> DeleteBillLineAsync(Guid billLineId, Guid tenantId) => Task.FromResult(false);
    public Task<decimal> GetBillSubTotalAsync(Guid billId, Guid tenantId) => Task.FromResult(0m);
    public Task<decimal> GetBillTaxTotalAsync(Guid billId, Guid tenantId) => Task.FromResult(0m);
    public Task<decimal> GetBillTotalAmountAsync(Guid billId, Guid tenantId) => Task.FromResult(0m);

    public Task<IEnumerable<Invoice>> GetAgedReceivablesAsync(Guid tenantId, DateTime asOfDate, int[] agingBuckets)
        => Task.FromResult(Enumerable.Empty<Invoice>());
    public Task<IEnumerable<Bill>> GetAgedPayablesAsync(Guid tenantId, DateTime asOfDate, int[] agingBuckets)
        => Task.FromResult(Enumerable.Empty<Bill>());
    public Task<IEnumerable<Invoice>> GetCustomerAgingAsync(Guid customerId, Guid tenantId, DateTime asOfDate)
        => Task.FromResult(Enumerable.Empty<Invoice>());
    public Task<IEnumerable<Bill>> GetSupplierAgingAsync(Guid supplierId, Guid tenantId, DateTime asOfDate)
        => Task.FromResult(Enumerable.Empty<Bill>());
}
