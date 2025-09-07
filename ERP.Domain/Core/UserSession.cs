using ERP.Domain;

namespace ERP.Domain.Core;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutTime { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string IPAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Expired, Revoked
    public new bool IsActive { get; set; } = true;
    
    // Navigation properties
    public User User { get; set; } = null!;
}