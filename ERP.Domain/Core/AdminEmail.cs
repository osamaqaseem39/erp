namespace ERP.Domain.Core;

/// <summary>Tenant admin notification email directory (reference User module — Admin Email).</summary>
public class AdminEmail : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    /// <summary>e.g. Notification, Finance, Support.</summary>
    public string Type { get; set; } = "Notification";
}
