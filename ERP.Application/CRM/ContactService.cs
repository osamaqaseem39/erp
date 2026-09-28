using ERP.Application.Interfaces.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

namespace ERP.Application.CRM;

public class ContactService : IContactService
{
    private readonly IContactRepository _repository;

    public ContactService(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContactDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ContactDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateContactDto dto)
    {
        var entity = MapToEntity(dto);
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateContactDto dto)
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

    public async Task<IEnumerable<ContactDto>> GetByCustomerAsync(Guid customerId)
    {
        var entities = await _repository.GetByCustomerAsync(customerId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<ContactDto>> SearchAsync(string searchTerm)
    {
        var entities = await _repository.SearchAsync(searchTerm);
        return entities.Select(MapToDto);
    }

    private static ContactDto MapToDto(Contact entity)
    {
        return new ContactDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CustomerId = entity.CustomerId ?? Guid.Empty,
            CustomerName = entity.Customer?.CompanyName,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            FullName = $"{entity.FirstName} {entity.LastName}".Trim(),
            JobTitle = entity.JobTitle,
            Department = entity.Department,
            Email = entity.Email,
            Phone = entity.Phone,
            IsPrimary = entity.IsPrimary,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            IsActive = entity.IsActive
        };
    }

    private static Contact MapToEntity(CreateContactDto dto)
    {
        return new Contact
        {
            TenantId = dto.TenantId,
            CustomerId = dto.CustomerId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            Email = dto.Email ?? string.Empty,
            Phone = dto.Phone,
            IsPrimary = dto.IsPrimary ?? false,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy
        };
    }

    private static void MapToEntity(UpdateContactDto dto, Contact entity)
    {
        if (!string.IsNullOrEmpty(dto.FirstName)) entity.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName)) entity.LastName = dto.LastName;
        entity.JobTitle = dto.JobTitle;
        entity.Department = dto.Department;
        entity.Email = dto.Email ?? entity.Email;
        entity.Phone = dto.Phone;
        if (dto.IsPrimary.HasValue) entity.IsPrimary = dto.IsPrimary.Value;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
