using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using ERP.Infrastructure.Repositories;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Domain.CRM;

namespace ERP.Application.CRM;

public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _repository;

    public QuoteService(IQuoteRepository repository)
    {
        _repository = repository;
    }

    public async Task<QuoteDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<QuoteDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateQuoteDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateQuoteDto dto)
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

    public async Task<IEnumerable<QuoteDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<QuoteDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<QuoteDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<bool> SendQuoteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Sent";
        entity.SentDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> ConvertToSalesOrderAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.Status = "Converted";
        entity.ConvertedDate = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    private static QuoteDto MapToDto(Quote entity)
    {
        return new QuoteDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CompanyId = entity.CompanyId,
            QuoteNumber = entity.QuoteNumber,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer?.CompanyName ?? string.Empty,
            QuoteDate = entity.QuoteDate,
            ValidUntil = entity.ValidUntil,
            Status = entity.Status,
            SubTotal = entity.SubTotal,
            TaxAmount = entity.TaxAmount,
            TotalAmount = entity.TotalAmount,
            Currency = entity.Currency,
            Notes = entity.Notes,
            Terms = entity.Terms,
            SentDate = entity.SentDate,
            ConvertedDate = entity.ConvertedDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Quote MapToEntity(CreateQuoteDto dto)
    {
        return new Quote
        {
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId,
            QuoteNumber = dto.QuoteNumber,
            CustomerId = dto.CustomerId,
            QuoteDate = dto.QuoteDate ?? DateTime.UtcNow,
            ValidUntil = dto.ValidUntil ?? DateTime.UtcNow.AddDays(30),
            Status = dto.Status ?? "Draft",
            SubTotal = dto.SubTotal ?? 0m,
            TaxAmount = dto.TaxAmount ?? 0m,
            TotalAmount = dto.TotalAmount ?? 0m,
            Currency = dto.Currency,
            Notes = dto.Notes,
            Terms = dto.Terms,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateQuoteDto dto, Quote entity)
    {
        entity.CustomerId = dto.CustomerId;
        entity.QuoteDate = dto.QuoteDate ?? entity.QuoteDate;
        entity.ValidUntil = dto.ValidUntil ?? entity.ValidUntil;
        entity.Status = dto.Status ?? entity.Status;
        entity.SubTotal = dto.SubTotal ?? entity.SubTotal;
        entity.TaxAmount = dto.TaxAmount ?? entity.TaxAmount;
        entity.TotalAmount = dto.TotalAmount ?? entity.TotalAmount;
        entity.Currency = dto.Currency;
        entity.Notes = dto.Notes;
        entity.Terms = dto.Terms;
        entity.UpdatedBy = dto.UpdatedBy;
    }
}