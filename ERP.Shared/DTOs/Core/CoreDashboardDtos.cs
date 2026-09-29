namespace ERP.Shared.DTOs.Core;

/// <summary>One dashboard widget's data, or the reason it couldn't be computed. Widgets fail independently.</summary>
public class WidgetResult<T>
{
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class UserActivityWidget
{
    public int TotalUsers { get; set; }
    public int ActiveLast7Days { get; set; }
    public int ActivePrevious7Days { get; set; }
    public int LockedAccounts { get; set; }
}

public class SecurityWidget
{
    public int FailedSignInsLast24h { get; set; }
    public int FailedSignInsPrevious24h { get; set; }
    public int Lockouts7Days { get; set; }
}

public class LogEntrySummary
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Level { get; set; }
}

public class ErrorsWidget
{
    public int ErrorsLast24h { get; set; }
    public int ErrorsPrevious24h { get; set; }
    public List<LogEntrySummary> Latest { get; set; } = new();
}

public class ConfigChangeSummary
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}

public class SetupChecklistItem
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Done { get; set; }
    /// <summary>Where the user completes this step.</summary>
    public string Link { get; set; } = string.Empty;
}

public class TenantStatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CoreDashboardDto
{
    public WidgetResult<UserActivityWidget> Users { get; set; } = new();
    public WidgetResult<SecurityWidget> Security { get; set; } = new();
    public WidgetResult<ErrorsWidget> Errors { get; set; } = new();
    public WidgetResult<List<ConfigChangeSummary>> ConfigChanges { get; set; } = new();
    public WidgetResult<List<SetupChecklistItem>> Setup { get; set; } = new();
    /// <summary>SuperAdmin only; null for everyone else.</summary>
    public WidgetResult<List<TenantStatusCount>>? Tenants { get; set; }
}
