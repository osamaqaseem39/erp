using ERP.Domain.Core;

namespace ERP.Infrastructure.Repositories.Core;

public interface ICoreMasterRepository
{
    // Country
    Task<Country?> GetCountryAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<Country>> ListCountriesAsync(Guid tenantId, bool? isActive = null);
    Task<Guid> AddCountryAsync(Country entity);
    Task UpdateCountryAsync(Country entity);

    // City
    Task<City?> GetCityAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<City>> ListCitiesAsync(Guid tenantId, Guid? countryId = null, bool? isActive = null);
    Task<Guid> AddCityAsync(City entity);
    Task UpdateCityAsync(City entity);

    // Branch
    Task<Branch?> GetBranchAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<Branch>> ListBranchesAsync(Guid tenantId, bool? isActive = null);
    Task<Guid> AddBranchAsync(Branch entity);
    Task UpdateBranchAsync(Branch entity);

    // PaymentTerm
    Task<PaymentTerm?> GetPaymentTermAsync(Guid id, Guid tenantId);
    Task<IReadOnlyList<PaymentTerm>> ListPaymentTermsAsync(Guid tenantId, bool? isActive = null);
    Task<Guid> AddPaymentTermAsync(PaymentTerm entity);
    Task UpdatePaymentTermAsync(PaymentTerm entity);
}
