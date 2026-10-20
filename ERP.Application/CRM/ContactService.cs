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

    public async Task<ContactDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return null;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<ContactDto>> GetAllAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetAllAsync(tenantId)).Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateContactDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return Guid.Empty;
        if (string.IsNullOrWhiteSpace(dto.FirstName) && string.IsNullOrWhiteSpace(dto.LastName))
            return Guid.Empty;

        var entity = MapToEntity(dto);
        entity.TenantId = tenantId;
        entity.IsActive = true;
        entity.CreatedBy = userId != Guid.Empty ? userId.ToString() : dto.CreatedBy;
        return await _repository.CreateAsync(entity);
    }

    public async Task<bool> UpdateAsync(UpdateContactDto dto, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(dto.Id, tenantId);
        if (entity == null) return false;

        MapToEntity(dto, entity);
        // FIX-5: IsActive only via Activate/Deactivate
        entity.UpdatedBy = userId != Guid.Empty ? userId.ToString() : dto.UpdatedBy;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return false;
        return await _repository.DeleteAsync(id, tenantId);
    }

    public async Task<bool> ActivateAsync(Guid id, Guid tenantId, Guid userId)
    {
        if (tenantId == Guid.Empty) return false;
        var entity = await _repository.GetByIdAsync(id, tenantId);
        if (entity == null) return false;
        if (entity.IsActive) return true;
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
        if (!entity.IsActive) return true;
        entity.IsActive = false;
        entity.UpdatedBy = userId == Guid.Empty ? entity.UpdatedBy : userId.ToString();
        entity.UpdatedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(entity);
    }

    public async Task<IEnumerable<ContactDto>> GetByCustomerAsync(Guid customerId, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.GetByCustomerAsync(customerId, tenantId)).Select(MapToDto);
    }

    public async Task<IEnumerable<ContactDto>> SearchAsync(string searchTerm, Guid tenantId)
    {
        if (tenantId == Guid.Empty) return [];
        return (await _repository.SearchAsync(searchTerm, tenantId)).Select(MapToDto);
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
            CustomerId = dto.CustomerId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            Email = dto.Email ?? string.Empty,
            Phone = dto.Phone,
            IsPrimary = dto.IsPrimary ?? false,
            Notes = dto.Notes
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
    }
}
