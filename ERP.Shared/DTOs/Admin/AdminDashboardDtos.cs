using ERP.Shared.DTOs.Core;

namespace ERP.Shared.DTOs.Admin;

public class AdminUsersWidget
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Locked { get; set; }
    public int WithoutRole { get; set; }
}

public class AdminRolesWidget
{
    public int Total { get; set; }
    public int SystemRoles { get; set; }
}

public class AdminSignInsWidget
{
    public int SignInsLast7Days { get; set; }
    public int SignInsPrevious7Days { get; set; }
    public int FailedSignInsLast7Days { get; set; }
}

public class AdminDashboardDto
{
    public WidgetResult<AdminUsersWidget> Users { get; set; } = new();
    public WidgetResult<AdminRolesWidget> Roles { get; set; } = new();
    public WidgetResult<AdminSignInsWidget> SignIns { get; set; } = new();
    public WidgetResult<List<ConfigChangeSummary>> RecentPermissionChanges { get; set; } = new();
}
