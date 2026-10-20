using ERP.Application.Common;
using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Infrastructure.Numbering;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

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

    public async Task<OpportunityDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<OpportunityDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateOpportunityDto dto)
    {
        var entity = MapToEntity(dto);
        // ERP-FIX-5: always start Open
        entity.Status = "Open";
        if (string.IsNullOrWhiteSpace(entity.OpportunityCode))
            entity.OpportunityCode = await _numbers.NextAsync("crm.opportunity");
        if (string.IsNullOrWhiteSpace(entity.OpportunityName))
            entity.OpportunityName = entity.Title;
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateOpportunityDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        var previousStatus = entity.Status;
        MapToEntity(dto, entity);
        // ERP-FIX-5: status only via win/lose/cancel/convert
        entity.Status = previousStatus;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByAssignedToAsync(Guid assignedTo)
    {
        var entities = await _repository.GetByAssignedToAsync(assignedTo);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByStageAsync(string stage)
    {
        var entities = await _repository.GetByStageAsync(stage);
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
        entity.CloseReason = reason;
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

    public async Task<bool> UpdateStageAsync(Guid id, string stage, decimal probability)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        if (!IsOpen(entity.Status)) return false;

        entity.Stage = stage;
        entity.Probability = probability;
        return await _repository.UpdateAsync(entity);
    }

    private async Task<Opportunity?> GetScopedAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.TenantId != tenantId) return null;
        return entity;
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
            CustomerName = entity.Customer?.CompanyName ?? string.Empty,
            Description = entity.Description,
            Stage = entity.Stage,
            Status = entity.Status,
            Probability = entity.Probability,
            Amount = entity.Amount,
            Currency = entity.Currency,
            ExpectedCloseDate = entity.ExpectedCloseDate,
            AssignedTo = entity.AssignedTo,
            AssignedToUserId = entity.AssignedToUserId,
            AssignedToName = entity.AssignedToUser?.FirstName + " " + entity.AssignedToUser?.LastName ?? string.Empty,
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
        return new Opportunity
        {
            TenantId = dto.TenantId,
            OpportunityCode = dto.OpportunityCode ?? string.Empty,
            OpportunityName = dto.OpportunityName ?? string.Empty,
            Title = dto.Title ?? string.Empty,
            CustomerId = dto.CustomerId,
            Description = dto.Description,
            Stage = dto.Stage ?? "Prospecting",
            Status = "Open",
            Probability = dto.Probability ?? 0m,
            Amount = dto.Amount ?? 0m,
            Currency = dto.Currency ?? "USD",
            ExpectedCloseDate = dto.ExpectedCloseDate,
            AssignedTo = dto.AssignedTo,
            AssignedToUserId = dto.AssignedToUserId,
            Source = dto.Source,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateOpportunityDto dto, Opportunity entity)
    {
        entity.CustomerId = dto.CustomerId;
        entity.Title = dto.Title ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(dto.OpportunityName))
            entity.OpportunityName = dto.OpportunityName;
        entity.Description = dto.Description;
        entity.Stage = dto.Stage ?? entity.Stage;
        entity.Probability = dto.Probability ?? entity.Probability;
        entity.Amount = dto.Amount ?? entity.Amount;
        entity.Currency = dto.Currency ?? entity.Currency;
        entity.ExpectedCloseDate = dto.ExpectedCloseDate;
        entity.AssignedTo = dto.AssignedTo;
        entity.AssignedToUserId = dto.AssignedToUserId;
        entity.Source = dto.Source;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}
