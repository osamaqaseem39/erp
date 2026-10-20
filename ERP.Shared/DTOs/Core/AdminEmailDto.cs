namespace ERP.Shared.DTOs.Core;

public class AdminEmailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateAdminEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = "Notification";
}

public class UpdateAdminEmailDto
{
    public string? Email { get; set; }
    public string? Type { get; set; }
    public bool? IsActive { get; set; }
}
