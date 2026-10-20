using ERP.Application.Interfaces.Inventory;
using ERP.Domain.SCM;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.SCM;
using ERP.Shared.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Inventory;

public class ItemCategoryService : IItemCategoryService
{
    private readonly IItemCategoryRepository _repository;
    private readonly ERPDbContext _db;

    public ItemCategoryService(IItemCategoryRepository repository, ERPDbContext db)
    {
        _repository = repository;
        _db = db;
    }

    public async Task<ItemCategoryDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var e = await _repository.GetByIdAsync(id, tenantId);
        return e == null ? null : Map(e);
    }

    public async Task<IEnumerable<ItemCategoryDto>> GetAllAsync(Guid tenantId)
        => (await _repository.GetAllAsync(tenantId)).Select(Map);

    public async Task<IEnumerable<ItemCategoryDto>> GetActiveAsync(Guid tenantId)
        => (await _repository.GetActiveAsync(tenantId)).Select(Map);

    public async Task<IEnumerable<ItemCategoryDto>> GetByParentAsync(Guid? parentId, Guid tenantId)
        => (await _repository.GetByParentAsync(tenantId, parentId)).Select(Map);

    public async Task<Guid> CreateAsync(CreateItemCategoryDto dto, Guid tenantId, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return Guid.Empty;
        var code = dto.Code.Trim();
        if (await _repository.CodeExistsAsync(tenantId, code))
            return Guid.Empty;
        if (dto.ParentCategoryId.HasValue
            && await _repository.GetByIdAsync(dto.ParentCategoryId.Value, tenantId) == null)
            return Guid.Empty;

        var entity = new ItemCategory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CategoryCode = code,
            CategoryName = dto.Name.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = dto.IsActive,
            CreatedBy = userId == Guid.Empty ? dto.CreatedBy : userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(UpdateItemCategoryDto dto, Guid tenantId, Guid userId)
    {
        var tracked = await _db.ItemCategories
            .FirstOrDefaultAsync(c => c.Id == dto.Id && c.TenantId == tenantId);
        if (tracked == null) return false;

        var code = string.IsNullOrWhiteSpace(dto.Code) ? tracked.CategoryCode : dto.Code.Trim();
        if (await _repository.CodeExistsAsync(tenantId, code, dto.Id))
            return false;
        if (dto.ParentCategoryId == dto.Id) return false;
        if (dto.ParentCategoryId.HasValue
            && await _repository.GetByIdAsync(dto.ParentCategoryId.Value, tenantId) == null)
            return false;

        tracked.CategoryCode = code;
        tracked.CategoryName = string.IsNullOrWhiteSpace(dto.Name) ? tracked.CategoryName : dto.Name.Trim();
        tracked.Name = tracked.CategoryName;
        tracked.Description = dto.Description;
        tracked.ParentCategoryId = dto.ParentCategoryId;
        tracked.IsActive = dto.IsActive;
        tracked.UpdatedBy = userId == Guid.Empty ? dto.UpdatedBy : userId.ToString();
        tracked.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> DeleteAsync(Guid id, Guid tenantId) => _repository.DeleteAsync(id, tenantId);

    private static ItemCategoryDto Map(ItemCategory e) => new()
    {
        Id = e.Id,
        TenantId = e.TenantId,
        Code = e.CategoryCode,
        Name = string.IsNullOrWhiteSpace(e.CategoryName) ? e.Name : e.CategoryName,
        Description = e.Description,
        ParentCategoryId = e.ParentCategoryId,
        ParentCategoryName = e.ParentCategory == null
            ? null
            : (string.IsNullOrWhiteSpace(e.ParentCategory.CategoryName) ? e.ParentCategory.Name : e.ParentCategory.CategoryName),
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        CreatedBy = e.CreatedBy,
        UpdatedBy = e.UpdatedBy
    };
}
