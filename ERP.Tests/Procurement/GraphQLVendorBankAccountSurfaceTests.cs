using ERP.Api.GraphQL.Procurement;
using ERP.Application.Services.Procurement;
using ERP.Domain.Procurement;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Procurement;
using ERP.Shared.DTOs.Procurement;
using ERP.Shared.Mappers.Procurement;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Procurement;

public class GraphQLVendorBankAccountSurfaceTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();
    private static readonly Guid User = Guid.NewGuid();
    private static readonly Guid Company = Guid.NewGuid();

    private static ERPDbContext Db() => new(new DbContextOptionsBuilder<ERPDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static VendorService Svc(ERPDbContext db) =>
        new(new VendorRepository(db), new VendorBankAccountRepository(db), new VendorMapper());

    [Fact]
    public void Surface_ExposesVendorBankAccountMutations()
    {
        var queries = typeof(ProcurementQueries).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementQueries))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);
        var mutations = typeof(ProcurementMutations).GetMethods()
            .Where(m => m.DeclaringType == typeof(ProcurementMutations))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("GetVendorBankAccounts", queries);
        Assert.Contains("AddVendorBankAccount", mutations);
        Assert.Contains("UpdateVendorBankAccount", mutations);
        Assert.Contains("DeleteVendorBankAccount", mutations);
    }

    [Fact]
    public async Task GraphQL_BankAccount_Create_List_Update_Delete_TenantFailClosed()
    {
        await using var db = Db();
        var vendorId = Guid.NewGuid();
        db.Vendors.Add(new Vendor
        {
            Id = vendorId,
            TenantId = Tenant,
            CompanyId = Company,
            VendorCode = "V-BA",
            VendorName = "Bank Vendor",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = Svc(db);
        var queries = new ProcurementQueries();
        var mutations = new ProcurementMutations();

        var accountId = await mutations.AddVendorBankAccount(svc, new CreateVendorBankAccountDto
        {
            BankName = "HBL",
            AccountNumber = "123456",
            Currency = "PKR",
            IsPrimary = true
        }, vendorId, Tenant, User);
        Assert.NotEqual(Guid.Empty, accountId);

        var listed = (await queries.GetVendorBankAccounts(svc, vendorId, Tenant)).ToList();
        Assert.Single(listed);
        Assert.Equal("HBL", listed[0].BankName);

        Assert.Empty(await queries.GetVendorBankAccounts(svc, vendorId, Other));

        var updatedId = await mutations.UpdateVendorBankAccount(svc, new UpdateVendorBankAccountDto
        {
            Id = accountId,
            BankName = "UBL",
            AccountNumber = "654321"
        }, Tenant, User);
        Assert.Equal(accountId, updatedId);
        Assert.Equal("UBL", (await queries.GetVendorBankAccounts(svc, vendorId, Tenant)).Single().BankName);

        Assert.False(await mutations.DeleteVendorBankAccount(svc, accountId, Other));
        Assert.True(await mutations.DeleteVendorBankAccount(svc, accountId, Tenant));
        Assert.Empty(await queries.GetVendorBankAccounts(svc, vendorId, Tenant));
    }
}
