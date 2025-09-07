using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class ResetUserPasswordDto
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}