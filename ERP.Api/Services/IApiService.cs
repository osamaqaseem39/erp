using ERP.Shared.DTOs;

namespace ERP.Api.Services;

public interface IApiService<TDto, TCreateDto, TUpdateDto> where TDto : BaseDto where TCreateDto : CreateDto where TUpdateDto : UpdateDto
{
    Task<TDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<Guid> CreateAsync(TCreateDto dto);
    Task<bool> UpdateAsync(TUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}