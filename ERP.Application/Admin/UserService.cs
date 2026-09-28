using ERP.Application.Interfaces.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace ERP.Application.Admin;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ERPDbContext _context;

    public UserService(
        IUserRepository userRepository, 
        IUserRoleRepository userRoleRepository,
        ERPDbContext context)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _context = context;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateUserDto dto)
    {
        // Check if user with email already exists
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {dto.Email} already exists.");
        }

        var (passwordHash, passwordSalt) = PasswordHelper.HashPassword(dto.Password);

        // Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            CompanyId = dto.CompanyId, // May be null for tenant owner or if caller allows
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FullName = $"{dto.FirstName} {dto.LastName}",
            PrimaryEmail = dto.Email,
            PrimaryPhone = dto.Phone ?? string.Empty,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsActive = dto.IsActive,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy ?? "System"
        };

        // Set default values for required fields
        user.Salutation ??= string.Empty;
        user.Gender ??= string.Empty;
        user.JobTitle ??= string.Empty;
        user.Department ??= string.Empty;
        user.Nationality ??= string.Empty;
        user.SecondaryEmail ??= string.Empty;
        user.SecondaryPhone ??= string.Empty;
        user.MobilePhone ??= string.Empty;
        user.Fax ??= string.Empty;
        user.LinkedIn ??= string.Empty;
        user.Twitter ??= string.Empty;
        user.Facebook ??= string.Empty;
        user.Instagram ??= string.Empty;
        user.PreferredCommunicationMethod ??= string.Empty;
        user.LanguagePreference ??= string.Empty;
        user.TimeZone ??= string.Empty;

        // Create user
        var userId = await _userRepository.CreateAsync(user);

        // Assign roles if provided
        if (dto.RoleIds != null && dto.RoleIds.Any())
        {
            foreach (var roleId in dto.RoleIds)
            {
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    TenantId = dto.TenantId,
                    UserId = userId,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy ?? "System"
                };
                await _userRoleRepository.AddAsync(userRole);
            }
        }

        return userId;
    }

    public async Task<bool> UpdateAsync(UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id);
        if (user == null) return false;

        // Update user properties
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.FullName = $"{dto.FirstName} {dto.LastName}";
        user.PrimaryEmail = dto.Email;
        user.PrimaryPhone = dto.Phone ?? string.Empty;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = dto.UpdatedBy;

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        return await _userRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UserDto>> GetByRoleAsync(Guid roleId)
    {
        var users = await _userRepository.GetByRoleAsync(roleId);
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return null;
        return MapToDto(user);
    }

    public async Task<bool> ResetPasswordAsync(ResetUserPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user == null) return false;

        var (hash, salt) = PasswordHelper.HashPassword(dto.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.RefreshToken = null;
        user.RefreshTokenExpires = null;
        await _context.SaveChangesAsync();
        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.PrimaryEmail ?? string.Empty,
            Phone = user.PrimaryPhone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }
}

