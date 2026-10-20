using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Core;

public class TenantDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? TenantName { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    /// <summary>Server-assigned logical DB name. Not accepted on create/update.</summary>
    public string DatabaseName { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;
}

public class CreateTenantDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? TenantName { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public new bool IsActive { get; set; } = true;
}

public class UpdateTenantDto : UpdateDto
{
    public string? Name { get; set; }
    public string? TenantName { get; set; }
    public string? Domain { get; set; }
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Minimal tenant fields for the public login tenant picker.
/// </summary>
public class LoginTenantOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}
