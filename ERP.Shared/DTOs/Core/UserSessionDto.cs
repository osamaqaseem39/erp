using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class UserSessionDto : BaseDto
{
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; }
}

public class CreateUserSessionDto : CreateDto
{
    public Guid UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTime? LoginTime { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Status { get; set; } = "Active";
}

public class UpdateUserSessionDto : UpdateDto
{
    public DateTime? LogoutTime { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
}