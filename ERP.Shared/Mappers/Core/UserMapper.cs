using ERP.Domain.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Mappers;

namespace ERP.Shared.Mappers.Core;

public static class UserMapper
{
    public static UserDto ToDto(User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Username = entity.Username,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            PhoneNumber = entity.PhoneNumber,
            IsActive = entity.IsActive,
            IsEmailVerified = entity.IsEmailVerified,
            IsPhoneVerified = entity.IsPhoneVerified,
            LastLoginAt = entity.LastLoginAt,
            EmailVerifiedAt = entity.EmailVerifiedAt,
            PhoneVerifiedAt = entity.PhoneVerifiedAt,
            ProfilePicture = entity.ProfilePicture,
            Notes = entity.Notes,
            CompanyId = entity.CompanyId,
            Salutation = entity.Salutation,
            FullName = entity.FullName,
            Gender = entity.Gender,
            DateOfBirth = entity.DateOfBirth,
            JobTitle = entity.JobTitle,
            Department = entity.Department,
            Nationality = entity.Nationality,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static User ToEntity(CreateUserDto dto, Guid tenantId, string createdBy)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = dto.Password, // Will be hashed by service
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            IsActive = true,
            IsEmailVerified = false,
            IsPhoneVerified = false,
            ProfilePicture = dto.ProfilePicture,
            Notes = dto.Notes,
            CompanyId = dto.CompanyId,
            Salutation = dto.Salutation,
            FullName = dto.FullName,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            Nationality = dto.Nationality,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static void UpdateEntity(UpdateUserDto dto, User entity, string updatedBy)
    {
        if (!string.IsNullOrEmpty(dto.Username))
            entity.Username = dto.Username;
        
        if (!string.IsNullOrEmpty(dto.Email))
            entity.Email = dto.Email;
        
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.PhoneNumber = dto.PhoneNumber;
        entity.ProfilePicture = dto.ProfilePicture;
        entity.Notes = dto.Notes;
        entity.CompanyId = dto.CompanyId;
        entity.Salutation = dto.Salutation;
        entity.FullName = dto.FullName;
        entity.Gender = dto.Gender;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.JobTitle = dto.JobTitle;
        entity.Department = dto.Department;
        entity.Nationality = dto.Nationality;
        
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }
}