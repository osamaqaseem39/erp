using System.Globalization;
using System.Net.Mail;
using ERP.Application.Core.Settings;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;

namespace ERP.Application.Core;

public interface ICompanyProfileService
{
    Task<CompanyProfileDto> GetAsync(Guid companyId, Guid tenantId);
    Task<CompanyProfileDto> UpdateAsync(Guid companyId, Guid tenantId, UpdateCompanyProfileDto dto);
    Task<TenantContextDto> GetTenantContextAsync(Guid? companyId, Guid tenantId);
}

public class CompanyProfileService : ICompanyProfileService
{
    private static readonly HashSet<string> IsoCurrencies = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Select(c => { try { return new RegionInfo(c.Name).ISOCurrencySymbol; } catch { return null; } })
        .Where(c => c != null)
        .Select(c => c!)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private readonly ICompanyRepository _companies;
    private readonly ICoreSettingsService _settings;

    public CompanyProfileService(ICompanyRepository companies, ICoreSettingsService settings)
    {
        _companies = companies;
        _settings = settings;
    }

    public async Task<CompanyProfileDto> GetAsync(Guid companyId, Guid tenantId) => ToDto(await LoadAsync(companyId, tenantId));

    public async Task<CompanyProfileDto> UpdateAsync(Guid companyId, Guid tenantId, UpdateCompanyProfileDto dto)
    {
        Validate(dto);
        var company = await LoadAsync(companyId, tenantId);

        company.CompanyName = dto.CompanyName.Trim();
        company.LegalName = Clean(dto.LegalName);
        company.RegistrationNumber = Clean(dto.RegistrationNumber);
        company.TaxId = company.TaxNumber = Clean(dto.TaxId);
        company.AddressLine1 = company.Address = Clean(dto.AddressLine1);
        company.AddressLine2 = Clean(dto.AddressLine2);
        company.City = Clean(dto.City);
        company.State = Clean(dto.State);
        company.PostalCode = company.ZipCode = Clean(dto.PostalCode);
        company.Country = Clean(dto.Country);
        company.Phone = Clean(dto.Phone);
        company.MobilePhone = Clean(dto.MobilePhone);
        company.ContactPerson = Clean(dto.ContactPerson);
        company.Email = Clean(dto.Email);
        company.Website = Clean(dto.Website);
        company.LogoUrl = Clean(dto.LogoUrl);
        company.Currency = dto.Currency.Trim().ToUpperInvariant();
        company.TimeZone = dto.TimeZone.Trim();
        company.UpdatedAt = DateTime.UtcNow;

        await _companies.UpdateAsync(company);
        return ToDto(company);
    }

    public async Task<TenantContextDto> GetTenantContextAsync(Guid? companyId, Guid tenantId)
    {
        var context = new TenantContextDto
        {
            Locale = await _settings.GetValueAsync("preferences.locale") ?? "en-US",
            DateFormat = await _settings.GetValueAsync("preferences.dateFormat") ?? "dd MMM yyyy",
            FirstDayOfWeek = int.TryParse(await _settings.GetValueAsync("preferences.firstDayOfWeek"), out var day) ? day : 1,
        };
        if (companyId is Guid id)
        {
            var company = await _companies.GetByIdAsync(id);
            if (company != null && company.TenantId == tenantId)
            {
                context.CompanyName = company.CompanyName;
                context.Currency = string.IsNullOrWhiteSpace(company.Currency) ? "USD" : company.Currency;
                context.TimeZone = string.IsNullOrWhiteSpace(company.TimeZone) ? "UTC" : company.TimeZone;
            }
        }
        return context;
    }

    private async Task<Domain.Core.Company> LoadAsync(Guid companyId, Guid tenantId)
    {
        var company = await _companies.GetByIdAsync(companyId);
        // Another tenant's company is indistinguishable from a missing one.
        if (company == null || company.TenantId != tenantId) throw new NotFoundException("Company profile not found.");
        return company;
    }

    private static void Validate(UpdateCompanyProfileDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.CompanyName)) errors.Add("Company name is required.");
        else if (dto.CompanyName.Trim().Length > 200) errors.Add("Company name cannot exceed 200 characters.");
        if (!IsoCurrencies.Contains(dto.Currency?.Trim() ?? "")) errors.Add("Choose a valid ISO currency code, e.g. USD or PKR.");
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(dto.TimeZone?.Trim() ?? "", out _)) errors.Add("Choose a valid time zone, e.g. Asia/Karachi.");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !MailAddress.TryCreate(dto.Email.Trim(), out _)) errors.Add("Enter a valid email address.");
        foreach (var (url, label) in new[] { (dto.Website, "Website"), (dto.LogoUrl, "Logo URL") })
        {
            if (!string.IsNullOrWhiteSpace(url) && !(Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == "https" || uri.Scheme == "http")))
                errors.Add($"{label} must be a full web address starting with https://.");
        }
        if (errors.Count > 0) throw new ValidationException("The company profile is not valid.", errors);
    }

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;

    private static string? OrNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static CompanyProfileDto ToDto(Domain.Core.Company c) => new()
    {
        Id = c.Id,
        CompanyName = c.CompanyName,
        LegalName = OrNull(c.LegalName),
        RegistrationNumber = OrNull(c.RegistrationNumber),
        TaxId = OrNull(c.TaxId) ?? OrNull(c.TaxNumber),
        AddressLine1 = OrNull(c.AddressLine1) ?? OrNull(c.Address),
        AddressLine2 = OrNull(c.AddressLine2),
        City = OrNull(c.City),
        State = OrNull(c.State),
        PostalCode = OrNull(c.PostalCode) ?? OrNull(c.ZipCode),
        Country = OrNull(c.Country),
        Phone = OrNull(c.Phone),
        MobilePhone = OrNull(c.MobilePhone),
        ContactPerson = OrNull(c.ContactPerson),
        Email = OrNull(c.Email),
        Website = OrNull(c.Website),
        LogoUrl = OrNull(c.LogoUrl),
        Currency = string.IsNullOrWhiteSpace(c.Currency) ? "USD" : c.Currency,
        TimeZone = string.IsNullOrWhiteSpace(c.TimeZone) ? "UTC" : c.TimeZone,
    };
}
