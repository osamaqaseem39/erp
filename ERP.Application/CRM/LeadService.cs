using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class LeadService : ILeadService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["New"] = new[] { "Qualified", "Lost" },
            ["Qualified"] = new[] { "Converted", "Lost" },
            ["Converted"] = Array.Empty<string>(),
            ["Lost"] = Array.Empty<string>()
        };

    private readonly IDocumentNumberService _numbers;
    private readonly ILeadRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOpportunityRepository _opportunityRepository;

    public LeadService(
        ILeadRepository repository,
        ICustomerRepository customerRepository,
        IOpportunityRepository opportunityRepository,
        IDocumentNumberService numbers)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _opportunityRepository = opportunityRepository;
        _numbers = numbers;
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
        // ERP-FIX-5: always start New
        entity.Status = "New";
        if (string.IsNullOrWhiteSpace(entity.LeadCode))
            entity.LeadCode = await _numbers.NextAsync("crm.lead");
        if (string.IsNullOrWhiteSpace(entity.LeadSource) && !string.IsNullOrWhiteSpace(entity.Source))
            entity.LeadSource = entity.Source;
        if (string.IsNullOrWhiteSpace(entity.Source) && !string.IsNullOrWhiteSpace(entity.LeadSource))
            entity.Source = entity.LeadSource;

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateLeadDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;
        if (!IsEditable(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via qualify/lose/convert
        entity.Status = previousStatus;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (!string.Equals(StatusOf(entity), "New", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repository.DeleteAsync(id);
    }

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

    public async Task<bool> QualifyAsync(Guid id, Guid tenantId, Guid userId)
    {
        var lead = await GetScopedAsync(id, tenantId);
        if (lead == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(lead), "Qualified", Transitions))
            return false;

        lead.Status = "Qualified";
        lead.UpdatedBy = userId.ToString();
        lead.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(lead);
    }

    public async Task<bool> MarkLostAsync(Guid id, Guid tenantId, Guid userId)
    {
        var lead = await GetScopedAsync(id, tenantId);
        if (lead == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(lead), "Lost", Transitions))
            return false;

        lead.Status = "Lost";
        lead.UpdatedBy = userId.ToString();
        lead.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(lead);
    }

    public async Task<Guid> ConvertToOpportunityAsync(Guid leadId, Guid tenantId, Guid userId)
    {
        var lead = await GetScopedAsync(leadId, tenantId);
        if (lead == null) return Guid.Empty;
        if (!DocumentWorkflow.CanTransition(StatusOf(lead), "Converted", Transitions))
            return Guid.Empty;

        var displayName = DisplayName(lead);

        var opportunity = new Opportunity
        {
            TenantId = lead.TenantId,
            OpportunityCode = await _numbers.NextAsync("crm.opportunity"),
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
            CreatedBy = userId.ToString()
        };

        var opportunityId = await _opportunityRepository.CreateAsync(opportunity);

        lead.Status = "Converted";
        lead.ConvertedDate = DateTime.UtcNow;
        lead.UpdatedBy = userId.ToString();
        lead.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(lead);

        return opportunityId;
    }

    public async Task<Guid> ConvertToCustomerAsync(Guid leadId, Guid tenantId, Guid userId)
    {
        var lead = await GetScopedAsync(leadId, tenantId);
        if (lead == null) return Guid.Empty;
        if (!DocumentWorkflow.CanTransition(StatusOf(lead), "Converted", Transitions))
            return Guid.Empty;

        var displayName = DisplayName(lead);

        var customer = new Customer
        {
            TenantId = lead.TenantId,
            CompanyId = lead.CompanyId ?? Guid.Empty,
            CustomerCode = await _numbers.NextAsync("crm.customer"),
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
            CreatedBy = userId.ToString()
        };

        var customerId = await _customerRepository.CreateAsync(customer);

        lead.Status = "Converted";
        lead.ConvertedDate = DateTime.UtcNow;
        lead.UpdatedBy = userId.ToString();
        lead.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(lead);

        return customerId;
    }

    private async Task<Lead?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return null;
        return entity;
    }

    private static bool IsEditable(string? status)
    {
        var s = StatusOf(status);
        return string.Equals(s, "New", StringComparison.OrdinalIgnoreCase)
               || string.Equals(s, "Qualified", StringComparison.OrdinalIgnoreCase);
    }

    private static string StatusOf(Lead entity) => StatusOf(entity.Status);

    private static string StatusOf(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "New" : status.Trim();

    private static string DisplayName(Lead lead) =>
        !string.IsNullOrWhiteSpace(lead.CompanyName)
            ? lead.CompanyName!
            : $"{lead.FirstName} {lead.LastName}".Trim();

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
        LeadCode = dto.LeadCode ?? string.Empty,
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
        Source = dto.Source ?? string.Empty,
        Status = "New",
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
        entity.Source = dto.Source ?? entity.Source;
        entity.AssignedTo = dto.AssignedTo;
        entity.LeadScore = dto.LeadScore;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
