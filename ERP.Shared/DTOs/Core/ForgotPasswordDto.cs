namespace ERP.Shared.DTOs.Core;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class ForgotPasswordResultDto
{
    /// <summary>Always true when the request was accepted (does not reveal whether the email exists).</summary>
    public bool Accepted { get; set; } = true;
    public string Message { get; set; } = "If an account exists for that email, a reset link has been sent.";
}

public class AuthSessionDto
{
    public Guid SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime IdleExpiresAt { get; set; }
    public DateTime SessionExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsCurrent { get; set; }
}
