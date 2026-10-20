using ERP.Application.Common;
using ERP.Shared.Exceptions;
using ERP.Shared.DTOs;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Domain.CRM;

namespace ERP.Application.CRM;

public class CustomerService : ICustomerService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Active"] = new[] { "Inactive", "Suspended" },
            ["Inactive"] = new[] { "Active" },
            ["Suspended"] = new[] { "Active" }
        };

    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(
        CustomerPagedQuery query,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) return new PagedResult<CustomerDto>();
        var page = await _repository.GetPagedAsync(query, tenantId, cancellationToken);
        return page.Map(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateCustomerDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (string.IsNullOrWhiteSpace(dto.CustomerCode) || string.IsNullOrWhiteSpace(dto.CustomerName))
            return Guid.Empty;

        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start Active; status only via Activate/Deactivate
        entity.Status = "Active";
        entity.IsActive = true;
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateCustomerDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        // ERP-FIX-5: status / IsActive only via Activate/Deactivate
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var dependents = await _repository.GetDependentCountsAsync(id, tenantId);
        if (dependents.Count > 0)
        {
            var summary = string.Join(", ", dependents.Select(d => $"{d.Value} {(d.Value == 1 ? Singular(d.Key) : d.Key)}"));
            throw new ConflictException($"It has {summary}. Mark it inactive instead to keep that history.");
        }
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ActivateAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase) && entity.IsActive)
            return true;
        // Allow re-activate from Inactive/Suspended; also heal Active+!IsActive
        if (!string.Equals(entity.Status, "Active", StringComparison.OrdinalIgnoreCase)
            && !DocumentWorkflow.CanTransition(entity.Status, "Active", Transitions))
            return false;

        entity.Status = "Active";
        entity.IsActive = true;
        entity.UpdatedBy = userId == Guid.Empty ? entity.UpdatedBy : userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeactivateAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (string.Equals(entity.Status, "Inactive", StringComparison.OrdinalIgnoreCase) && !entity.IsActive)
            return true;
        if (!string.Equals(entity.Status, "Inactive", StringComparison.OrdinalIgnoreCase)
            && !DocumentWorkflow.CanTransition(entity.Status, "Inactive", Transitions))
            return false;

        entity.Status = "Inactive";
        entity.IsActive = false;
        entity.UpdatedBy = userId == Guid.Empty ? entity.UpdatedBy : userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private static string Singular(string plural) => plural switch
    {
        "opportunities" => "opportunity",
        "activities" => "activity",
        _ => plural.TrimEnd('s'),
    };

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var term = searchTerm.Trim();
        var entities = await _repository.GetAllAsync(tenantId);
        return entities
            .Where(c =>
                c.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.CustomerCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerDto>> GetByIndustryAsync(string industry, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByIndustryAsync(industry, tenantId);
        return entities.Select(MapToDto);
    }

    private static CustomerDto MapToDto(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            CustomerCode = entity.CustomerCode,
            CustomerName = entity.CustomerName,
            CompanyName = entity.CompanyName,
            ContactPerson = entity.ContactPerson,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            Industry = entity.Industry,
            CustomerType = entity.CustomerType,
            Status = entity.Status,
            CreditLimit = entity.CreditLimit,
            CreditDays = entity.CreditDays,
            PaymentTerms = entity.PaymentTerms,
            TaxId = entity.TaxId,
            TaxType = entity.TaxType,
            Ntn = entity.Ntn,
            Strn = entity.Strn,
            Cnic = entity.Cnic,
            SalesmanId = entity.SalesmanId,
            SalesmanName = entity.Salesman?.FullName,
            Website = entity.Website,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Customer MapToEntity(CreateCustomerDto dto)
    {
        var ntn = dto.Ntn ?? dto.TaxId;
        return new Customer
        {
            CompanyId = dto.CompanyId,
            CustomerCode = dto.CustomerCode,
            CustomerName = dto.CustomerName,
            CompanyName = dto.CompanyName,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            Industry = dto.Industry,
            CustomerType = dto.CustomerType,
            CreditLimit = dto.CreditLimit,
            CreditDays = dto.CreditDays,
            PaymentTerms = dto.PaymentTerms ?? "Net 30",
            TaxId = dto.TaxId ?? ntn,
            TaxType = dto.TaxType,
            Ntn = ntn,
            Strn = dto.Strn,
            Cnic = dto.Cnic,
            SalesmanId = dto.SalesmanId,
            Website = dto.Website,
            Notes = dto.Notes
        };
    }

    private static void MapToEntity(UpdateCustomerDto dto, Customer entity)
    {
        entity.CompanyName = dto.CompanyName;
        if (!string.IsNullOrWhiteSpace(dto.CustomerName)) entity.CustomerName = dto.CustomerName;
        entity.ContactPerson = dto.ContactPerson;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.State = dto.State;
        entity.Country = dto.Country;
        entity.PostalCode = dto.PostalCode;
        entity.Industry = dto.Industry;
        entity.CustomerType = dto.CustomerType;
        // Status intentionally not mapped (FIX-5)
        entity.CreditLimit = dto.CreditLimit;
        if (dto.CreditDays.HasValue) entity.CreditDays = dto.CreditDays.Value;
        entity.PaymentTerms = dto.PaymentTerms ?? entity.PaymentTerms;
        entity.TaxId = dto.TaxId;
        if (dto.TaxType != null) entity.TaxType = dto.TaxType;
        if (dto.Ntn != null) entity.Ntn = dto.Ntn;
        if (dto.Strn != null) entity.Strn = dto.Strn;
        if (dto.Cnic != null) entity.Cnic = dto.Cnic;
        if (dto.SalesmanId.HasValue) entity.SalesmanId = dto.SalesmanId;
        entity.Website = dto.Website;
        entity.Notes = dto.Notes;
    }
}
