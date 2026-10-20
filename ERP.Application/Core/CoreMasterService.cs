using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;

namespace ERP.Application.Core;

public interface ICoreMasterService
{
    Task<IReadOnlyList<CountryDto>> ListCountriesAsync(Guid tenantId, bool? isActive = null);
    Task<CountryDto?> GetCountryAsync(Guid id, Guid tenantId);
    Task<CountryDto> CreateCountryAsync(CreateCountryDto dto, Guid tenantId, Guid userId);
    Task<CountryDto> UpdateCountryAsync(Guid id, UpdateCountryDto dto, Guid tenantId, Guid userId);
    Task DeleteCountryAsync(Guid id, Guid tenantId, Guid userId);

    Task<IReadOnlyList<CityDto>> ListCitiesAsync(Guid tenantId, Guid? countryId = null, bool? isActive = null);
    Task<CityDto?> GetCityAsync(Guid id, Guid tenantId);
    Task<CityDto> CreateCityAsync(CreateCityDto dto, Guid tenantId, Guid userId);
    Task<CityDto> UpdateCityAsync(Guid id, UpdateCityDto dto, Guid tenantId, Guid userId);
    Task DeleteCityAsync(Guid id, Guid tenantId, Guid userId);

    Task<IReadOnlyList<BranchDto>> ListBranchesAsync(Guid tenantId, bool? isActive = null);
    Task<BranchDto?> GetBranchAsync(Guid id, Guid tenantId);
    Task<BranchDto> CreateBranchAsync(CreateBranchDto dto, Guid tenantId, Guid userId);
    Task<BranchDto> UpdateBranchAsync(Guid id, UpdateBranchDto dto, Guid tenantId, Guid userId);
    Task DeleteBranchAsync(Guid id, Guid tenantId, Guid userId);

    Task<IReadOnlyList<PaymentTermDto>> ListPaymentTermsAsync(Guid tenantId, bool? isActive = null);
    Task<PaymentTermDto?> GetPaymentTermAsync(Guid id, Guid tenantId);
    Task<PaymentTermDto> CreatePaymentTermAsync(CreatePaymentTermDto dto, Guid tenantId, Guid userId);
    Task<PaymentTermDto> UpdatePaymentTermAsync(Guid id, UpdatePaymentTermDto dto, Guid tenantId, Guid userId);
    Task DeletePaymentTermAsync(Guid id, Guid tenantId, Guid userId);
}

public class CoreMasterService : ICoreMasterService
{
    private readonly ICoreMasterRepository _repo;
    public CoreMasterService(ICoreMasterRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<CountryDto>> ListCountriesAsync(Guid tenantId, bool? isActive = null) =>
        (await _repo.ListCountriesAsync(tenantId, isActive)).Select(MapCountry).ToList();

    public async Task<CountryDto?> GetCountryAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetCountryAsync(id, tenantId);
        return e == null ? null : MapCountry(e);
    }

    public async Task<CountryDto> CreateCountryAsync(CreateCountryDto dto, Guid tenantId, Guid userId)
    {
        var code = Require(dto.Code, "Code").ToUpperInvariant();
        var name = Require(dto.Name, "Name");
        var entity = new Country
        {
            TenantId = tenantId,
            Code = code,
            Name = name,
            CreatedBy = userId.ToString()
        };
        await _repo.AddCountryAsync(entity);
        return MapCountry(entity);
    }

    public async Task<CountryDto> UpdateCountryAsync(Guid id, UpdateCountryDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetCountryAsync(id, tenantId) ?? throw new NotFoundException("Country not found.");
        if (!string.IsNullOrWhiteSpace(dto.Code)) entity.Code = dto.Code.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateCountryAsync(entity);
        return MapCountry(entity);
    }

    public async Task DeleteCountryAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetCountryAsync(id, tenantId) ?? throw new NotFoundException("Country not found.");
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateCountryAsync(entity);
    }

    public async Task<IReadOnlyList<CityDto>> ListCitiesAsync(Guid tenantId, Guid? countryId = null, bool? isActive = null) =>
        (await _repo.ListCitiesAsync(tenantId, countryId, isActive)).Select(MapCity).ToList();

    public async Task<CityDto?> GetCityAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetCityAsync(id, tenantId);
        return e == null ? null : MapCity(e);
    }

    public async Task<CityDto> CreateCityAsync(CreateCityDto dto, Guid tenantId, Guid userId)
    {
        if (dto.CountryId == Guid.Empty) throw new ValidationException("Country is required.", new List<string> { "CountryId" });
        _ = await _repo.GetCountryAsync(dto.CountryId, tenantId) ?? throw new NotFoundException("Country not found.");
        var entity = new City
        {
            TenantId = tenantId,
            CountryId = dto.CountryId,
            Name = Require(dto.Name, "Name"),
            CreatedBy = userId.ToString()
        };
        await _repo.AddCityAsync(entity);
        return MapCity(entity);
    }

    public async Task<CityDto> UpdateCityAsync(Guid id, UpdateCityDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetCityAsync(id, tenantId) ?? throw new NotFoundException("City not found.");
        if (dto.CountryId.HasValue && dto.CountryId.Value != Guid.Empty)
        {
            _ = await _repo.GetCountryAsync(dto.CountryId.Value, tenantId) ?? throw new NotFoundException("Country not found.");
            entity.CountryId = dto.CountryId.Value;
        }
        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateCityAsync(entity);
        return MapCity(entity);
    }

    public async Task DeleteCityAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetCityAsync(id, tenantId) ?? throw new NotFoundException("City not found.");
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateCityAsync(entity);
    }

    public async Task<IReadOnlyList<BranchDto>> ListBranchesAsync(Guid tenantId, bool? isActive = null) =>
        (await _repo.ListBranchesAsync(tenantId, isActive)).Select(MapBranch).ToList();

    public async Task<BranchDto?> GetBranchAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetBranchAsync(id, tenantId);
        return e == null ? null : MapBranch(e);
    }

    public async Task<BranchDto> CreateBranchAsync(CreateBranchDto dto, Guid tenantId, Guid userId)
    {
        if (dto.CityId.HasValue && dto.CityId.Value != Guid.Empty)
            _ = await _repo.GetCityAsync(dto.CityId.Value, tenantId) ?? throw new NotFoundException("City not found.");
        var entity = new Branch
        {
            TenantId = tenantId,
            Code = Require(dto.Code, "Code").ToUpperInvariant(),
            Name = Require(dto.Name, "Name"),
            Address = dto.Address?.Trim(),
            CityId = dto.CityId,
            CreatedBy = userId.ToString()
        };
        await _repo.AddBranchAsync(entity);
        return MapBranch(entity);
    }

    public async Task<BranchDto> UpdateBranchAsync(Guid id, UpdateBranchDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetBranchAsync(id, tenantId) ?? throw new NotFoundException("Branch not found.");
        if (!string.IsNullOrWhiteSpace(dto.Code)) entity.Code = dto.Code.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        if (dto.Address != null) entity.Address = dto.Address.Trim();
        if (dto.CityId.HasValue) entity.CityId = dto.CityId;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateBranchAsync(entity);
        return MapBranch(entity);
    }

    public async Task DeleteBranchAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetBranchAsync(id, tenantId) ?? throw new NotFoundException("Branch not found.");
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateBranchAsync(entity);
    }

    public async Task<IReadOnlyList<PaymentTermDto>> ListPaymentTermsAsync(Guid tenantId, bool? isActive = null) =>
        (await _repo.ListPaymentTermsAsync(tenantId, isActive)).Select(MapTerm).ToList();

    public async Task<PaymentTermDto?> GetPaymentTermAsync(Guid id, Guid tenantId)
    {
        var e = await _repo.GetPaymentTermAsync(id, tenantId);
        return e == null ? null : MapTerm(e);
    }

    public async Task<PaymentTermDto> CreatePaymentTermAsync(CreatePaymentTermDto dto, Guid tenantId, Guid userId)
    {
        if (dto.Days < 0) throw new ValidationException("Days must be >= 0.", new List<string> { "Days" });
        var entity = new PaymentTerm
        {
            TenantId = tenantId,
            Name = Require(dto.Name, "Name"),
            Days = dto.Days,
            Description = dto.Description?.Trim(),
            CreatedBy = userId.ToString()
        };
        await _repo.AddPaymentTermAsync(entity);
        return MapTerm(entity);
    }

    public async Task<PaymentTermDto> UpdatePaymentTermAsync(Guid id, UpdatePaymentTermDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetPaymentTermAsync(id, tenantId) ?? throw new NotFoundException("Payment term not found.");
        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        if (dto.Days.HasValue)
        {
            if (dto.Days.Value < 0) throw new ValidationException("Days must be >= 0.", new List<string> { "Days" });
            entity.Days = dto.Days.Value;
        }
        if (dto.Description != null) entity.Description = dto.Description.Trim();
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdatePaymentTermAsync(entity);
        return MapTerm(entity);
    }

    public async Task DeletePaymentTermAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetPaymentTermAsync(id, tenantId) ?? throw new NotFoundException("Payment term not found.");
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdatePaymentTermAsync(entity);
    }

    private static string Require(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{field} is required.", new List<string> { field });
        return value.Trim();
    }

    private static CountryDto MapCountry(Country e) => new()
    {
        Id = e.Id, Code = e.Code, Name = e.Name, IsActive = e.IsActive
    };

    private static CityDto MapCity(City e) => new()
    {
        Id = e.Id, CountryId = e.CountryId, Name = e.Name, IsActive = e.IsActive
    };

    private static BranchDto MapBranch(Branch e) => new()
    {
        Id = e.Id, Code = e.Code, Name = e.Name, Address = e.Address, CityId = e.CityId, IsActive = e.IsActive
    };

    private static PaymentTermDto MapTerm(PaymentTerm e) => new()
    {
        Id = e.Id, Name = e.Name, Days = e.Days, Description = e.Description, IsActive = e.IsActive
    };
}
