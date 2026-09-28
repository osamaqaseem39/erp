namespace ERP.Shared.DTOs.Core;

public class SuperAdminSetupDto
{
    // Tenant Information
    public string TenantName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    
    // Super Admin User Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class SuperAdminSetupResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
}

