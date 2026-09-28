using ERP.Domain;

namespace ERP.Domain.Core;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
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
    
    // Additional properties expected by repositories
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
    public string? PasswordSalt { get; set; }
    public string? Status { get; set; }
    public bool IsTenantOwner { get; set; } = false;
    
    // Security and authentication properties
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
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public virtual Company? Company { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}