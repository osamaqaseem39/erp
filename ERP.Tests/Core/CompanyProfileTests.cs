using ERP.Application.Core;
using ERP.Application.Core.Settings;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Repositories.Admin;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.Abstractions;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests.Core;

public class CompanyProfileTests
{
    private static readonly Guid Tenant = Guid.NewGuid();
    private readonly ERPDbContext _db = new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly CompanyProfileService _service;
    private readonly CoreSettingsService _settings;
    private readonly Company _company;

    public CompanyProfileTests()
    {
        _company = new Company { TenantId = Tenant, CompanyName = "Acme", TaxNumber = "TN-1", Address = "1 Old St", ZipCode = "54000", Currency = "USD", TimeZone = "UTC" };
        _db.Companies.Add(_company);
        _db.SaveChanges();
        _settings = new CoreSettingsService(new SystemSettingRepository(_db, new FixedTenant(Tenant)), new PlainProtector());
        _service = new CompanyProfileService(new CompanyRepository(_db), _settings);
    }

    [Fact]
    public async Task ReadsLegacyDuplicateColumnsIntoOneShape()
    {
        var profile = await _service.GetAsync(_company.Id, Tenant);

        Assert.Equal(("TN-1", "1 Old St", "54000"), (profile.TaxId, profile.AddressLine1, profile.PostalCode));
    }

    [Fact]
    public async Task UpdateKeepsDuplicateColumnsInSync()
    {
        await _service.UpdateAsync(_company.Id, Tenant, Valid() with { TaxId = "NTN-9", AddressLine1 = "5 New Rd", PostalCode = "75500", Currency = "pkr", TimeZone = "Asia/Karachi" });

        var row = await _db.Companies.SingleAsync(c => c.Id == _company.Id);
        Assert.Equal(("NTN-9", "NTN-9"), (row.TaxId, row.TaxNumber));
        Assert.Equal(("5 New Rd", "5 New Rd"), (row.AddressLine1, row.Address));
        Assert.Equal(("75500", "75500"), (row.PostalCode, row.ZipCode));
        Assert.Equal(("PKR", "Asia/Karachi"), (row.Currency, row.TimeZone));
    }

    [Fact]
    public async Task UpdatePersistsContactPersonAndMobilePhone()
    {
        await _service.UpdateAsync(_company.Id, Tenant, Valid() with
        {
            ContactPerson = "Aslam Khan",
            MobilePhone = "+92-300-1234567",
            Phone = "+92-21-1111111",
            LogoUrl = "https://cdn.example.com/logo.png",
        });

        var profile = await _service.GetAsync(_company.Id, Tenant);
        Assert.Equal("Aslam Khan", profile.ContactPerson);
        Assert.Equal("+92-300-1234567", profile.MobilePhone);
        Assert.Equal("+92-21-1111111", profile.Phone);
        Assert.Equal("https://cdn.example.com/logo.png", profile.LogoUrl);
    }

    [Theory]
    [InlineData("", "USD", "UTC")]
    [InlineData("Acme", "XYZ", "UTC")]
    [InlineData("Acme", "USD", "Mars/Olympus")]
    public async Task InvalidProfilesAreRejected(string name, string currency, string timeZone)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateAsync(_company.Id, Tenant, Valid() with { CompanyName = name, Currency = currency, TimeZone = timeZone }));
    }

    [Fact]
    public async Task AnotherTenantsCompanyIsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(_company.Id, Guid.NewGuid()));
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(_company.Id, Guid.NewGuid(), Valid()));
    }

    [Fact]
    public async Task TenantContextCombinesCompanyAndPreferences()
    {
        await _service.UpdateAsync(_company.Id, Tenant, Valid() with { Currency = "EUR", TimeZone = "Europe/Berlin" });
        await _settings.UpdateCategoryAsync("preferences", new UpdateSettingsCategoryDto { Values = new() { ["preferences.locale"] = "de-DE" } });

        var context = await _service.GetTenantContextAsync(_company.Id, Tenant);

        Assert.Equal(("Acme", "EUR", "Europe/Berlin", "de-DE"), (context.CompanyName, context.Currency, context.TimeZone, context.Locale));
    }

    private static UpdateRequest Valid() => new() { CompanyName = "Acme", Currency = "USD", TimeZone = "UTC" };

    /// <summary>Record wrapper so tests can use `with` on the DTO.</summary>
    private sealed record UpdateRequest
    {
        public string CompanyName { get; init; } = "";
        public string? TaxId { get; init; }
        public string? AddressLine1 { get; init; }
        public string? PostalCode { get; init; }
        public string? Phone { get; init; }
        public string? MobilePhone { get; init; }
        public string? ContactPerson { get; init; }
        public string? LogoUrl { get; init; }
        public string Currency { get; init; } = "USD";
        public string TimeZone { get; init; } = "UTC";

        public static implicit operator UpdateCompanyProfileDto(UpdateRequest r) => new()
        {
            CompanyName = r.CompanyName,
            TaxId = r.TaxId,
            AddressLine1 = r.AddressLine1,
            PostalCode = r.PostalCode,
            Phone = r.Phone,
            MobilePhone = r.MobilePhone,
            ContactPerson = r.ContactPerson,
            LogoUrl = r.LogoUrl,
            Currency = r.Currency,
            TimeZone = r.TimeZone,
        };
    }

    private sealed class PlainProtector : ISecretProtector
    {
        public string Protect(string plaintext) => plaintext;
        public string Unprotect(string protectedValue) => protectedValue;
    }

    private sealed class FixedTenant(Guid id) : ITenantProvider
    {
        public Guid? GetTenantId() => id;
    }
}
