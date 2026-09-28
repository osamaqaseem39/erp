namespace ERP.Shared.DTOs.Core;

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}