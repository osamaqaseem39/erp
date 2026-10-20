using ERP.Application.Services.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using ERP.Shared.Mappers.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class ProcurementTenantIsolationTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class StubVendors : IVendorRepository
    {
        private readonly Dictionary<Guid, Vendor> _byId = new();

        public void Seed(Vendor vendor) => _byId[vendor.Id] = vendor;

        public Task<Vendor?> GetByIdAsync(Guid id, Guid tenantId) =>
            Task.FromResult(_byId.TryGetValue(id, out var v) && v.TenantId == tenantId ? v : null);

        public Task<Vendor?> GetByCodeAsync(string vendorCode, Guid companyId, Guid tenantId) =>
            Task.FromResult(_byId.Values.FirstOrDefault(v =>
                v.TenantId == tenantId && v.CompanyId == companyId && v.VendorCode == vendorCode));

        public Task<Vendor?> GetByEmailAsync(string email, Guid tenantId) =>
            Task.FromResult(_byId.Values.FirstOrDefault(v =>
                v.TenantId == tenantId && string.Equals(v.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<IEnumerable<Vendor>> GetAllAsync(Guid tenantId, Guid? companyId = null) =>
            Task.FromResult<IEnumerable<Vendor>>(_byId.Values.Where(v =>
                v.TenantId == tenantId && (!companyId.HasValue || v.CompanyId == companyId)));

        public Task<(IEnumerable<Vendor> Vendors, int TotalCount)> SearchAsync(
            Guid tenantId, Guid? companyId = null, string? searchTerm = null, bool? isActive = null,
            int pageNumber = 1, int pageSize = 50, string sortBy = "VendorName", string sortDirection = "ASC")
        {
            var q = _byId.Values.Where(v => v.TenantId == tenantId);
            var list = q.ToList();
            return Task.FromResult<(IEnumerable<Vendor>, int)>((list, list.Count));
        }

        public Task<Guid> AddAsync(Vendor vendor)
        {
            _byId[vendor.Id] = vendor;
            return Task.FromResult(vendor.Id);
        }

        public Task<bool> UpdateAsync(Vendor vendor)
        {
            _byId[vendor.Id] = vendor;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(Guid id, Guid tenantId) =>
            Task.FromResult(_byId.TryGetValue(id, out var v) && v.TenantId == tenantId && _byId.Remove(id));

        public Task<bool> ExistsAsync(Guid id, Guid tenantId) =>
            Task.FromResult(_byId.TryGetValue(id, out var v) && v.TenantId == tenantId);

        public Task<bool> ExistsByCodeAsync(string vendorCode, Guid companyId, Guid tenantId, Guid? excludeId = null) =>
            Task.FromResult(false);

        public Task<bool> ExistsByEmailAsync(string email, Guid tenantId, Guid? excludeId = null) =>
            Task.FromResult(false);
    }

    [Fact]
    public async Task VendorBankAccount_Other_Tenant_Cannot_Read_Or_Delete()
    {
        await using var db = Db();
        var vendorId = Guid.NewGuid();
        var vendors = new StubVendors();
        vendors.Seed(new Vendor
        {
            Id = vendorId,
            TenantId = TenantB,
            CompanyId = Company,
            VendorCode = "VB",
            VendorName = "Vendor B",
            IsActive = true
        });

        var banks = new VendorBankAccountRepository(db);
        var svc = new VendorService(vendors, banks, new VendorMapper());

        var accountId = await svc.AddVendorBankAccountAsync(new CreateVendorBankAccountDto
        {
            BankName = "Secret Bank",
            AccountNumber = "999",
            Currency = "USD"
        }, vendorId, TenantB, User);

        Assert.Empty(await svc.GetVendorBankAccountsAsync(vendorId, TenantA));
        Assert.False(await svc.DeleteVendorBankAccountAsync(accountId, TenantA));
        Assert.Single(await svc.GetVendorBankAccountsAsync(vendorId, TenantB));
    }

    [Fact]
    public async Task Rfq_Other_Tenant_Cannot_Get_Or_Delete()
    {
        await using var db = Db();
        var rfqId = Guid.NewGuid();
        db.RequestForQuotations.Add(new RequestForQuotation
        {
            Id = rfqId,
            TenantId = TenantB,
            CompanyId = Company,
            RequestorId = User,
            RFQNumber = "RFQ-B",
            Title = "Secret",
            IssuedDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repo = new RequestForQuotationRepository(db);
        Assert.Null(await repo.GetByIdAsync(rfqId, TenantA));
        Assert.False(await repo.DeleteAsync(rfqId, TenantA));
        Assert.Empty(await repo.GetAllAsync(TenantA));
        Assert.Single(await repo.GetAllAsync(TenantB));
    }

    [Fact]
    public async Task PurchaseReturn_Empty_Tenant_Fails_Closed()
    {
        await using var db = Db();
        db.PurchaseReturns.Add(new PurchaseReturn
        {
            Id = Guid.NewGuid(),
            TenantId = TenantA,
            VendorId = Guid.NewGuid(),
            ReturnNumber = "PR-1",
            ReturnDate = DateTime.UtcNow,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repo = new PurchaseReturnRepository(db);
        Assert.Empty(await repo.GetAllAsync(Guid.Empty));
        Assert.Null(await repo.GetByIdAsync(db.PurchaseReturns.First().Id, Guid.Empty));
    }
}
