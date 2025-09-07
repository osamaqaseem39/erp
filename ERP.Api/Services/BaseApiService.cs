using ERP.Application.Core;
using ERP.Shared.DTOs;
using ERP.Shared.Exceptions;

namespace ERP.Api.Services;

public abstract class BaseApiService<TDto, TCreateDto, TUpdateDto> : IApiService<TDto, TCreateDto, TUpdateDto>
    where TDto : BaseDto 
    where TCreateDto : CreateDto 
    where TUpdateDto : UpdateDto
{
    protected readonly ILogger<BaseApiService<TDto, TCreateDto, TUpdateDto>> _logger;

    protected BaseApiService(ILogger<BaseApiService<TDto, TCreateDto, TUpdateDto>> logger)
    {
        _logger = logger;
    }

    public abstract Task<TDto?> GetByIdAsync(Guid id);
    public abstract Task<IEnumerable<TDto>> GetAllAsync();
    public abstract Task<Guid> CreateAsync(TCreateDto dto);
    public abstract Task<bool> UpdateAsync(TUpdateDto dto);
    public abstract Task<bool> DeleteAsync(Guid id);

    protected void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Id cannot be empty");
    }

    protected void ValidateDto(TCreateDto dto)
    {
        if (dto == null)
            throw new ValidationException("DTO cannot be null");
    }

    protected void ValidateUpdateDto(TUpdateDto dto)
    {
        if (dto == null)
            throw new ValidationException("Update DTO cannot be null");
        
        ValidateId(dto.Id);
    }
}