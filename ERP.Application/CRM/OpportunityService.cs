using ERP.Infrastructure.Numbering;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Domain.CRM;

namespace ERP.Application.CRM;

public class OpportunityService : IOpportunityService
{
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
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateOpportunityDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
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

    public async Task<Guid> ConvertToQuoteAsync(Guid opportunityId)
    {
        var entity = await _repository.GetByIdAsync(opportunityId);
        if (entity == null) return Guid.Empty;

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
            CreatedBy = entity.CreatedBy
        };

        var quoteId = await _quoteRepository.CreateAsync(quote);

        entity.Status = "Converted";
        entity.ConvertedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);

        return quoteId;
    }

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
            Status = dto.Status ?? "Open",
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
        entity.Description = dto.Description;
        entity.Stage = dto.Stage ?? "Prospecting";
        entity.Status = dto.Status ?? "Open";
            entity.Probability = dto.Probability ?? 0m;
            entity.Amount = dto.Amount ?? 0m;
        entity.Currency = dto.Currency ?? "USD";
        entity.ExpectedCloseDate = dto.ExpectedCloseDate;
        entity.AssignedTo = dto.AssignedTo;
        entity.AssignedToUserId = dto.AssignedToUserId;
        entity.Source = dto.Source;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
    }

    public async Task<bool> UpdateStageAsync(Guid id, string stage, decimal probability)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Stage = stage;
        entity.Probability = probability;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> CloseAsync(Guid id, string reason)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Closed";
        entity.CloseReason = reason;
        entity.ClosedDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }
}