using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class UserDto : BaseDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public new bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Notes { get; set; }
    
    // Additional properties
    public Guid? CompanyId { get; set; }
    public string? Salutation { get; set; }
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Nationality { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? PreferredCommunicationMethod { get; set; }
    public string? LanguagePreference { get; set; }
    public string? TimeZone { get; set; }
    public string? Status { get; set; }
    
    // Security properties
    public DateTime? LastLoginDate { get; set; }
    public bool IsLocked { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpires { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    
    // Navigation properties
    public string CompanyName { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = new List<string>();
}

public class CreateUserDto : CreateDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; } = true;
    public string? ProfilePicture { get; set; }
    public string? Notes { get; set; }
    
    // Additional properties
    public Guid? CompanyId { get; set; }
    public string? Salutation { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Nationality { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? PreferredCommunicationMethod { get; set; }
    public string? LanguagePreference { get; set; }
    public string? TimeZone { get; set; }
    public string? Status { get; set; }
    
    public List<Guid> RoleIds { get; set; } = new List<Guid>();
}

public class UpdateUserDto : UpdateDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Notes { get; set; }
    
    // Additional properties
    public Guid? CompanyId { get; set; }
    public string? Salutation { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Nationality { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? PreferredCommunicationMethod { get; set; }
    public string? LanguagePreference { get; set; }
    public string? TimeZone { get; set; }
    public string? Status { get; set; }
    
    public List<Guid> RoleIds { get; set; } = new List<Guid>();
}

public class LoginDto
{
    public string PrimaryEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}