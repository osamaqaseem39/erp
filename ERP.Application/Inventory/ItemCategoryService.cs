using ERP.Application.Interfaces.Inventory;
using ERP.Shared.DTOs.Inventory;
using System;
using System.Linq;

namespace ERP.Application.Inventory;

/// <summary>DI stub for IItemCategoryService. Replace with real business logic incrementally.</summary>
public class ItemCategoryService : IItemCategoryService
{
    public Task<ItemCategoryDto?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<ItemCategoryDto?>(null);
    }

    public Task<IEnumerable<ItemCategoryDto>> GetAllAsync()
    {
        return Task.FromResult(Enumerable.Empty<ItemCategoryDto>());
    }

    public Task<Guid> CreateAsync(CreateItemCategoryDto dto)
    {
        return Task.FromResult(Guid.Empty);
    }

    public Task<bool> UpdateAsync(UpdateItemCategoryDto dto)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<ItemCategoryDto>> GetActiveAsync()
    {
        return Task.FromResult(Enumerable.Empty<ItemCategoryDto>());
    }

    public Task<IEnumerable<ItemCategoryDto>> GetByParentAsync(Guid? parentId)
    {
        return Task.FromResult(Enumerable.Empty<ItemCategoryDto>());
    }

}
