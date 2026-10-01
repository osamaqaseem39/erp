using ERP.Domain;

namespace ERP.Domain.Core;

/// <summary>
/// One refresh token. Only its SHA-256 hash is stored. Every token belongs to a sign-in session
/// (<see cref="SessionId"/>); using a token replaces it with a new one in the same session, and using
/// an already-replaced token again ends the whole session (it was probably stolen).
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    /// <summary>Idle limit: the tenant's session timeout after this token was issued.</summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>Hard limit for the whole session, set at sign-in.</summary>
    public DateTime SessionExpiresAt { get; set; }
    public DateTime? ReplacedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
}
