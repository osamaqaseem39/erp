using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

/// <summary>Open pipeline stages and the win probability each implies by default.</summary>
public static class OpportunityStages
{
    public static readonly IReadOnlyDictionary<string, decimal> Open =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["Prospecting"] = 10,
            ["Qualification"] = 25,
            ["Proposal"] = 50,
            ["Negotiation"] = 75
        };

    public static bool IsOpen(string? stage) => stage != null && Open.ContainsKey(stage.Trim());

    /// <summary>Canonical casing for a known open stage, or Prospecting.</summary>
    public static string Normalize(string? stage) =>
        Open.Keys.FirstOrDefault(k => string.Equals(k, stage?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "Prospecting";
}

public class OpportunityService : IOpportunityService
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> Transitions =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Open"] = new[] { "Won", "Lost", "Cancelled" },
            ["Won"] = new[] { "Converted" },
            ["Lost"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>(),
            ["Converted"] = Array.Empty<string>()
        };

    private readonly IDocumentNumberService _numbers;
    private readonly IOpportunityRepository _repository;
    private readonly IQuoteRepository _quoteRepository;

    public OpportunityService(IOpportunityRepository repository, IQuoteRepository quoteRepository, IDocumentNumberService numbers)
    {
        _repository = repository;
        _quoteRepository = quoteRepository;
        _numbers = numbers;
    }

    public async Task<OpportunityDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<OpportunityDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetAllAsync(tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateOpportunityDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        // ERP-FIX-5: always start Open
        entity.Status = "Open";
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        // Codes are always issued by the tenant sequence (OP-2026-00001); a client value is ignored.
        entity.OpportunityCode = await _numbers.NextAsync("crm.opportunity");
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateOpportunityDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via win/lose/cancel/convert
        entity.Status = previousStatus;
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByCustomerAsync(customerId, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByStatusAsync(string status, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStatusAsync(status, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByAssignedToAsync(assignedTo, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByStageAsync(string stage, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        var entities = await _repository.GetByStageAsync(stage, tenantId);
        return entities.Select(MapToDto);
    }

    public async Task<bool> MarkWonAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(entity), "Won", Transitions))
            return false;

        entity.Status = "Won";
        entity.Stage = "Closed Won";
        entity.Probability = 100;
        entity.ActualValue ??= entity.Amount;
        entity.ActualCloseDate = DateTime.UtcNow;
        entity.ClosedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> MarkLostAsync(Guid id, Guid tenantId, Guid userId, string? reason = null)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(entity), "Lost", Transitions))
            return false;

        entity.Status = "Lost";
        entity.Stage = "Closed Lost";
        entity.Probability = 0;
        entity.CloseReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        entity.ActualCloseDate = DateTime.UtcNow;
        entity.ClosedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CancelAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(id, tenantId);
        if (entity == null) return false;
        if (!DocumentWorkflow.CanTransition(StatusOf(entity), "Cancelled", Transitions))
            return false;

        entity.Status = "Cancelled";
        entity.ClosedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        return await _repository.UpdateAsync(entity);
    }

    public Task<bool> CloseAsync(Guid id, Guid tenantId, Guid userId, string? reason = null) =>
        MarkLostAsync(id, tenantId, userId, reason);

    public async Task<Guid> ConvertToQuoteAsync(Guid opportunityId, Guid tenantId, Guid userId)
    {
        var entity = await GetScopedAsync(opportunityId, tenantId);
        if (entity == null) return Guid.Empty;
        if (!DocumentWorkflow.CanTransition(StatusOf(entity), "Converted", Transitions))
            return Guid.Empty;

        var quote = new Quote
        {
            TenantId = entity.TenantId,
            CompanyId = entity.Customer?.CompanyId ?? Guid.Empty,
            QuoteNumber = await _numbers.NextAsync("crm.quote"),
            CustomerId = entity.CustomerId,
            OpportunityId = entity.Id,
            QuoteDate = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(30),
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            Status = "Draft",
            SubTotal = entity.Amount,
            TaxAmount = 0,
            TotalAmount = entity.Amount,
            Currency = entity.Currency,
            Notes = entity.Notes,
            AssignedToUserId = entity.AssignedToUserId ?? entity.AssignedTo,
            CreatedBy = userId.ToString()
        };

        var quoteId = await _quoteRepository.CreateAsync(quote);

        entity.Status = "Converted";
        entity.ConvertedDate = DateTime.UtcNow;
        entity.UpdatedBy = userId.ToString();
        await _repository.UpdateAsync(entity);

        return quoteId;
    }

    public async Task<bool> UpdateStageAsync(Guid id, string stage, decimal probability, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        // Closed stages are reached only through win/lose.
        if (!OpportunityStages.IsOpen(stage) || probability is < 0 or > 100) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        entity.Stage = OpportunityStages.Normalize(stage);
        entity.Probability = probability;
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private async Task<Opportunity?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        return await _repository.GetByIdAsync(id, tenantId);
    }

    private static bool IsOpen(string? status) =>
        string.Equals(StatusOf(status), "Open", StringComparison.OrdinalIgnoreCase);

    private static string StatusOf(Opportunity entity) => StatusOf(entity.Status);

    private static string StatusOf(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "Open" : status.Trim();

    private static OpportunityDto MapToDto(Opportunity entity)
    {
        return new OpportunityDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OpportunityCode = entity.OpportunityCode,
            OpportunityName = entity.OpportunityName,
            Title = entity.Title,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer == null ? null
                : string.IsNullOrWhiteSpace(entity.Customer.CustomerName) ? entity.Customer.CompanyName : entity.Customer.CustomerName,
            LeadId = entity.LeadId,
            LeadName = entity.Lead == null ? null
                : string.IsNullOrWhiteSpace(entity.Lead.CompanyName) ? $"{entity.Lead.FirstName} {entity.Lead.LastName}".Trim() : entity.Lead.CompanyName,
            Description = entity.Description,
            Stage = entity.Stage,
            Status = entity.Status,
            Probability = entity.Probability,
            Amount = entity.Amount,
            EstimatedValue = entity.Amount,
            WeightedValue = Math.Round(entity.Amount * entity.Probability / 100m, 2),
            ActualValue = entity.ActualValue,
            Currency = entity.Currency,
            ExpectedCloseDate = entity.ExpectedCloseDate,
            ActualCloseDate = entity.ActualCloseDate,
            CloseReason = entity.CloseReason,
            AssignedTo = OwnerOf(entity),
            AssignedToUserId = OwnerOf(entity),
            AssignedToName = OwnerName(entity),
            AssignedToUserName = OwnerName(entity),
            Source = entity.Source,
            Notes = entity.Notes,
            ConvertedDate = entity.ConvertedDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Opportunity MapToEntity(CreateOpportunityDto dto)
    {
        var entity = new Opportunity
        {
            TenantId = dto.TenantId,
            LeadId = dto.LeadId,
            Status = "Open",
            CreatedBy = dto.CreatedBy
        };
        ApplyFields(entity, dto.OpportunityName, dto.Title, dto.CustomerId, dto.Description, dto.Stage, dto.Probability,
            dto.Amount ?? dto.EstimatedValue, dto.Currency, dto.ExpectedCloseDate, dto.AssignedToUserId ?? dto.AssignedTo,
            dto.Source, dto.Notes);
        return entity;
    }

    private static void MapToEntity(UpdateOpportunityDto dto, Opportunity entity)
    {
        // OpportunityCode, LeadId and status are fixed by the workflow and never edited here.
        ApplyFields(entity, dto.OpportunityName ?? entity.OpportunityName, dto.Title, dto.CustomerId ?? entity.CustomerId,
            dto.Description, dto.Stage ?? entity.Stage, dto.Probability,
            dto.Amount ?? dto.EstimatedValue ?? entity.Amount, dto.Currency ?? entity.Currency, dto.ExpectedCloseDate,
            dto.AssignedToUserId ?? dto.AssignedTo, dto.Source, dto.Notes);
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    private static void ApplyFields(Opportunity entity, string? name, string? title, Guid? customerId, string? description,
        string? stage, decimal? probability, decimal? amount, string? currency, DateTime? expectedCloseDate, Guid? ownerId,
        string? source, string? notes)
    {
        var newStage = OpportunityStages.Normalize(stage);
        var stageChanged = !string.Equals(entity.Stage, newStage, StringComparison.OrdinalIgnoreCase);
        entity.OpportunityName = (string.IsNullOrWhiteSpace(name) ? title : name)?.Trim() ?? string.Empty;
        entity.Title = string.IsNullOrWhiteSpace(title) ? entity.OpportunityName : title.Trim();
        entity.CustomerId = customerId;
        entity.Description = description;
        entity.Stage = newStage;
        // No probability given → the stage default when the stage moved (or none was ever set).
        entity.Probability = probability
            ?? (stageChanged || entity.Probability == 0 ? OpportunityStages.Open[newStage] : entity.Probability);
        entity.Amount = amount ?? 0m;
        entity.EstimatedValue = entity.Amount;
        entity.Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
        entity.ExpectedCloseDate = expectedCloseDate;
        // Owner is stored in two legacy columns; keep them in sync.
        entity.AssignedToUserId = entity.AssignedTo = ownerId;
        entity.Source = source;
        entity.Notes = notes;
    }

    private static Guid? OwnerOf(Opportunity entity) => entity.AssignedToUserId ?? entity.AssignedTo;

    private static string? OwnerName(Opportunity entity) =>
        entity.AssignedToUser == null ? null : $"{entity.AssignedToUser.FirstName} {entity.AssignedToUser.LastName}".Trim();
}
