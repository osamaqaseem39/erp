using ERP.Shared.Exceptions;
using ERP.Shared.DTOs;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Domain.CRM;

namespace ERP.Application.CRM;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<PagedResult<CustomerDto>> GetPagedAsync(CustomerPagedQuery query, CancellationToken cancellationToken = default)
    {
        var page = await _repository.GetPagedAsync(query, cancellationToken);
        return page.Map(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateCustomerDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateCustomerDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var dependents = await _repository.GetDependentCountsAsync(id);
        if (dependents.Count > 0)
        {
            var summary = string.Join(", ", dependents.Select(d => $"{d.Value} {(d.Value == 1 ? Singular(d.Key) : d.Key)}"));
            throw new ConflictException($"It has {summary}. Mark it inactive instead to keep that history.");
        }
        return await _repository.DeleteAsync(id);
    }

    private static string Singular(string plural) => plural switch
    {
        "opportunities" => "opportunity",
        "activities" => "activity",
        _ => plural.TrimEnd('s'),
    };

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.Trim();
        var entities = await _repository.GetAllAsync();
        return entities
            .Where(c =>
                c.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.CustomerCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerDto>> GetByIndustryAsync(string industry)
    {
        var entities = await _repository.GetByIndustryAsync(industry);
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
            PaymentTerms = entity.PaymentTerms,
            TaxId = entity.TaxId,
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
        return new Customer
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            CustomerCode = dto.CustomerCode,
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
            Status = dto.Status,
            CreditLimit = dto.CreditLimit,
            PaymentTerms = dto.PaymentTerms ?? "Net 30",
            TaxId = dto.TaxId,
            Website = dto.Website,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateCustomerDto dto, Customer entity)
    {
        entity.CompanyName = dto.CompanyName;
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
        entity.Status = dto.Status;
        entity.CreditLimit = dto.CreditLimit;
        entity.PaymentTerms = dto.PaymentTerms ?? entity.PaymentTerms;
        entity.TaxId = dto.TaxId;
        entity.Website = dto.Website;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}