using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOpportunityRepository _opportunityRepository;

    public LeadService(
        ILeadRepository repository,
        ICustomerRepository customerRepository,
        IOpportunityRepository opportunityRepository)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _opportunityRepository = opportunityRepository;
    }

    public async Task<LeadDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<LeadDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateLeadDto dto)
    {
        var entity = MapToEntity(dto);
        if (string.IsNullOrWhiteSpace(entity.LeadCode))
            entity.LeadCode = CrmLineCalculator.GenerateNumber("LD");

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateLeadDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<LeadDto>> GetBySourceAsync(string source)
    {
        var entities = await _repository.GetBySourceAsync(source);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeadDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeadDto>> GetByAssignedToAsync(Guid assignedTo)
    {
        var entities = await _repository.GetByAssignedToAsync(assignedTo);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeadDto>> SearchAsync(string searchTerm)
    {
        var entities = await _repository.SearchAsync(searchTerm);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> ConvertToOpportunityAsync(Guid leadId)
    {
        var lead = await _repository.GetByIdAsync(leadId);
        if (lead == null) return Guid.Empty;

        var displayName = !string.IsNullOrWhiteSpace(lead.CompanyName)
            ? lead.CompanyName!
            : $"{lead.FirstName} {lead.LastName}".Trim();

        var opportunity = new Opportunity
        {
            TenantId = lead.TenantId,
            OpportunityCode = CrmLineCalculator.GenerateNumber("OP"),
            OpportunityName = displayName,
            Title = displayName,
            LeadId = lead.Id,
            Description = lead.Notes,
            Stage = "Qualification",
            Status = "Open",
            Amount = lead.EstimatedValue ?? 0,
            Probability = 10,
            Currency = "USD",
            Source = string.IsNullOrWhiteSpace(lead.Source) ? lead.LeadSource : lead.Source,
            AssignedTo = lead.AssignedTo,
            AssignedToUserId = lead.AssignedToUserId ?? lead.AssignedTo,
            Notes = lead.Notes,
            CreatedBy = lead.CreatedBy
        };

        var opportunityId = await _opportunityRepository.CreateAsync(opportunity);

        lead.Status = "Converted";
        lead.ConvertedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(lead);

        return opportunityId;
    }

    public async Task<Guid> ConvertToCustomerAsync(Guid leadId)
    {
        var lead = await _repository.GetByIdAsync(leadId);
        if (lead == null) return Guid.Empty;

        var displayName = !string.IsNullOrWhiteSpace(lead.CompanyName)
            ? lead.CompanyName!
            : $"{lead.FirstName} {lead.LastName}".Trim();

        var customer = new Customer
        {
            TenantId = lead.TenantId,
            CompanyId = lead.CompanyId ?? Guid.Empty,
            CustomerCode = CrmLineCalculator.GenerateNumber("CU"),
            CustomerName = displayName,
            CompanyName = lead.CompanyName ?? displayName,
            ContactPerson = lead.ContactPerson ?? $"{lead.FirstName} {lead.LastName}".Trim(),
            Email = lead.Email,
            Phone = lead.Phone,
            Address = lead.Address,
            City = lead.City,
            State = lead.State,
            Country = lead.Country,
            PostalCode = lead.PostalCode,
            Industry = lead.Industry,
            CustomerType = string.IsNullOrWhiteSpace(lead.CompanyName) ? "Individual" : "Company",
            Status = "Active",
            Notes = lead.Notes,
            CreatedBy = lead.CreatedBy
        };

        var customerId = await _customerRepository.CreateAsync(customer);

        lead.Status = "Converted";
        lead.ConvertedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(lead);

        return customerId;
    }

    private static LeadDto MapToDto(Lead entity) => new()
    {
        Id = entity.Id,
        TenantId = entity.TenantId,
        CompanyId = entity.CompanyId,
        LeadCode = entity.LeadCode,
        FirstName = entity.FirstName,
        LastName = entity.LastName,
        EstimatedValue = entity.EstimatedValue,
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
        Source = entity.Source,
        Status = entity.Status,
        AssignedTo = entity.AssignedTo,
        AssignedToName = entity.AssignedToUser != null
            ? $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim()
            : string.Empty,
        LeadScore = entity.LeadScore,
        Notes = entity.Notes,
        ConvertedDate = entity.ConvertedDate,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive
    };

    private static Lead MapToEntity(CreateLeadDto dto) => new()
    {
        TenantId = dto.TenantId,
        CompanyId = dto.CompanyId,
        LeadCode = dto.LeadCode,
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
        Source = dto.Source,
        Status = dto.Status,
        AssignedTo = dto.AssignedTo,
        LeadScore = dto.LeadScore,
        Notes = dto.Notes,
        CreatedBy = dto.CreatedBy
    };

    private static void MapToEntity(UpdateLeadDto dto, Lead entity)
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
        entity.Source = dto.Source;
        entity.Status = dto.Status;
        entity.AssignedTo = dto.AssignedTo;
        entity.LeadScore = dto.LeadScore;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
