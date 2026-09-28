namespace ERP.Application.Core;

public static class DefaultPermissions
{
    public record PermissionDefinition(
        string Name,
        string Module,
        string ModuleName,
        string Action,
        string ActionName,
        string Description);

    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new("core.users.view", "Core", "Core", "View", "View Users", "View users"),
        new("core.users.create", "Core", "Core", "Create", "Create Users", "Create users"),
        new("core.users.update", "Core", "Core", "Update", "Update Users", "Update users"),
        new("core.users.delete", "Core", "Core", "Delete", "Delete Users", "Delete users"),
        new("core.roles.view", "Core", "Core", "View", "View Roles", "View roles"),
        new("core.roles.create", "Core", "Core", "Create", "Create Roles", "Create roles"),
        new("core.roles.update", "Core", "Core", "Update", "Update Roles", "Update roles"),
        new("core.roles.delete", "Core", "Core", "Delete", "Delete Roles", "Delete roles"),
        new("core.roles.assign", "Core", "Core", "Assign", "Assign Permissions", "Assign role permissions"),
        new("core.tenants.view", "Core", "Core", "View", "View Tenants", "View tenants"),
        new("core.tenants.create", "Core", "Core", "Create", "Create Tenants", "Create tenants"),
        new("core.tenants.update", "Core", "Core", "Update", "Update Tenants", "Update tenants"),
        new("core.tenants.delete", "Core", "Core", "Delete", "Delete Tenants", "Delete tenants"),
        new("core.settings.view", "Core", "Core", "View", "View Settings", "View system settings"),
        new("core.settings.manage", "Core", "Core", "Manage", "Manage Settings", "Manage system settings"),
        new("core.audit.view", "Core", "Core", "View", "View Audit Logs", "View audit logs"),
        new("core.logs.view", "Core", "Core", "View", "View System Logs", "View system logs"),
    ];
}
