using ERP.Shared.DTOs.Admin;

namespace ERP.Application.Interfaces.Admin;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateUserDto dto);
    Task<bool> UpdateAsync(UpdateUserDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<UserDto>> GetByRoleAsync(Guid roleId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<bool> ResetPasswordAsync(ResetUserPasswordDto dto);
}