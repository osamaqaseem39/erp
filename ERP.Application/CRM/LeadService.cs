using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.CRM;
using ERP.Shared.Exceptions;

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

    public async Task<LeadDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<LeadDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateLeadDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (!dto.AllowDuplicate)
        {
            var matches = await _repository.FindDuplicatesAsync(dto.Email, dto.Phone, null, tenantId);
            if (matches.Count > 0)
                throw new ConflictException(
                    "Possible duplicate: " + string.Join("; ", matches.Take(3).Select(m => $"{m.Kind} {m.Code} {m.Name}".Replace("  ", " ").Trim()))
                    + ". Tick \"Allow duplicate\" to save anyway.");
        }
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start New
        entity.Status = "New";
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        // Lead codes are always issued by the tenant sequence (LD-2026-00001); a client value is ignored.
        entity.LeadCode = await _numbers.NextAsync("crm.lead");

        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateLeadDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!IsEditable(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via qualify/lose/convert
        entity.Status = previousStatus;
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!string.Equals(StatusOf(entity), "New", StringComparison.OrdinalIgnoreCase))
            return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<LeadDto>> GetBySourceAsync(string source, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetBySourceAsync(source, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeadDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeadDto>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByAssignedToAsync(assignedTo, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<LeadDto>> SearchAsync(string searchTerm, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.SearchAsync(searchTerm, tenantId);
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

    public async Task<bool> MarkLostAsync(Guid id, LoseLeadDto dto, Guid tenantId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.LostReason)) return false;
        var lead = await GetScopedAsync(id, tenantId);
        if (lead == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(lead), "Lost", Transitions))
            return false;

        lead.Status = "Lost";
        lead.LostReason = dto.LostReason.Trim();
        lead.LostNote = string.IsNullOrWhiteSpace(dto.LostNote) ? null : dto.LostNote.Trim();
        lead.UpdatedBy = userId.ToString();
        lead.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(lead);
    }

    public Task<IReadOnlyList<LeadDuplicateDto>> FindDuplicatesAsync(string? email, string? phone, Guid? excludeLeadId, Guid tenantId) =>
        _repository.FindDuplicatesAsync(email, phone, excludeLeadId, tenantId);

    public async Task<Guid> ConvertToOpportunityAsync(Guid leadId, Guid tenantId, Guid userId, ConvertLeadToOpportunityDto? dto = null)
    {
        var lead = await GetScopedAsync(leadId, tenantId);
        if (lead == null) return Guid.Empty;
        if (!DocumentWorkflow.CanTransition(StatusOf(lead), "Converted", Transitions))
            return Guid.Empty;

        var displayName = string.IsNullOrWhiteSpace(dto?.OpportunityName) ? DisplayName(lead) : dto!.OpportunityName!.Trim();

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
            Amount = dto?.Amount ?? lead.EstimatedValue ?? 0,
            Probability = Math.Clamp(dto?.Probability ?? 10, 0, 100),
            ExpectedCloseDate = dto?.ExpectedCloseDate,
            CustomerId = dto?.CustomerId,
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
        return await _repository.GetByIdAsync(id, tenantId);
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
        LeadSource = SourceOf(entity),
        Source = SourceOf(entity),
        Status = entity.Status,
        Priority = entity.Priority,
        LastContactDate = entity.LastContactDate,
        AssignedToUserId = OwnerOf(entity),
        AssignedToId = OwnerOf(entity),
        AssignedTo = OwnerOf(entity),
        AssignedToName = entity.AssignedToUser != null
            ? $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim()
            : string.Empty,
        AssignedToUserName = entity.AssignedToUser != null
            ? $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim()
            : null,
        LeadScore = entity.LeadScore,
        Notes = entity.Notes,
        ConvertedDate = entity.ConvertedDate,
        LostReason = entity.LostReason,
        LostNote = entity.LostNote,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedBy = entity.UpdatedBy,
        IsActive = entity.IsActive
    };

    private static Lead MapToEntity(CreateLeadDto dto)
    {
        var entity = new Lead
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            Status = "New",
            CreatedBy = dto.CreatedBy
        };
        ApplyFields(entity, dto.FirstName, dto.LastName, dto.CompanyName, dto.ContactPerson, dto.Email, dto.Phone,
            dto.Address, dto.City, dto.State, dto.Country, dto.PostalCode, dto.Industry,
            Coalesce(dto.LeadSource, dto.Source), dto.Priority, dto.EstimatedValue, dto.LastContactDate,
            dto.AssignedToUserId ?? dto.AssignedToId ?? dto.AssignedTo, dto.Notes);
        return entity;
    }

    private static void MapToEntity(UpdateLeadDto dto, Lead entity)
    {
        // LeadCode is immutable once issued.
        ApplyFields(entity, dto.FirstName, dto.LastName, dto.CompanyName, dto.ContactPerson, dto.Email, dto.Phone,
            dto.Address, dto.City, dto.State, dto.Country, dto.PostalCode, dto.Industry,
            Coalesce(dto.LeadSource, dto.Source) ?? SourceOf(entity), dto.Priority, dto.EstimatedValue,
            // Last contact is maintained from logged activities, not typed on the edit form.
            entity.LastContactDate ?? dto.LastContactDate,
            dto.AssignedToUserId ?? dto.AssignedToId ?? dto.AssignedTo, dto.Notes);
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    private static void ApplyFields(Lead entity, string firstName, string lastName, string? companyName, string? contactPerson,
        string? email, string? phone, string? address, string? city, string? state, string? country, string? postalCode,
        string? industry, string? source, string? priority, decimal? estimatedValue, DateTime? lastContactDate,
        Guid? ownerId, string? notes)
    {
        entity.FirstName = firstName.Trim();
        entity.LastName = (lastName ?? string.Empty).Trim();
        entity.CompanyName = companyName;
        entity.ContactPerson = contactPerson;
        entity.Email = email?.Trim().ToLowerInvariant();
        entity.Phone = phone?.Trim();
        entity.Address = address;
        entity.City = city;
        entity.State = state;
        entity.Country = country;
        entity.PostalCode = postalCode;
        entity.Industry = industry;
        // Source and owner are each stored in legacy duplicate columns; keep them in sync.
        entity.LeadSource = entity.Source = source ?? string.Empty;
        entity.Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority;
        entity.EstimatedValue = estimatedValue;
        entity.LastContactDate = lastContactDate;
        entity.AssignedToUserId = entity.AssignedToId = entity.AssignedTo = ownerId;
        entity.Notes = notes;
        // Score is always calculated; a client-supplied value is ignored.
        entity.LeadScore = LeadScoring.Score(entity, DateTime.UtcNow);
    }

    private static string? Coalesce(params string?[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

    private static string SourceOf(Lead entity) => Coalesce(entity.LeadSource, entity.Source) ?? string.Empty;

    private static Guid? OwnerOf(Lead entity) => entity.AssignedToUserId ?? entity.AssignedToId ?? entity.AssignedTo;
}
