using ERP.Application.Core;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.DTOs.Core;
using HotChocolate;
using IPermissionService = ERP.Application.Interfaces.Core.IPermissionService;
using IRoleService = ERP.Application.Interfaces.Admin.IRoleService;
using AuditLogDto = ERP.Shared.DTOs.Admin.AuditLogDto;
using PermissionDto = ERP.Shared.DTOs.Core.PermissionDto;
using RoleDto = ERP.Shared.DTOs.Admin.RoleDto;
using UserDto = ERP.Shared.DTOs.Admin.UserDto;

namespace ERP.Api.GraphQL.Admin;

[ExtendObjectType("Query")]
public class AdminQueries
{
    // User Queries
    public async Task<UserDto?> GetUser(
        [Service] IUserService service,
        Guid userId)
    {
        return await service.GetByIdAsync(userId);
    }

    public async Task<IEnumerable<UserDto>> GetUsers(
        [Service] IUserService service,
        Guid? roleId = null)
    {
        if (roleId.HasValue)
            return await service.GetByRoleAsync(roleId.Value);
        return await service.GetAllAsync();
    }

    public async Task<UserDto?> GetUserByEmail(
        [Service] IUserService service,
        string email)
    {
        return await service.GetByEmailAsync(email);
    }

    // Role Queries
    public async Task<RoleDto?> GetRole(
        [Service] IRoleService service,
        Guid roleId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(roleId, tenantId);
    }

    public async Task<IEnumerable<RoleDto>> GetRoles(
        [Service] IRoleService service,
        Guid tenantId,
        bool? isActive = null)
    {
        return await service.GetAllAsync(tenantId, isActive);
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissions(
        [Service] IRoleService service,
        Guid roleId,
        Guid tenantId)
        => await service.GetPermissionsAsync(roleId, tenantId);

    public async Task<IEnumerable<PermissionDto>> GetPermissions(
        [Service] IPermissionService permissionService,
        Guid tenantId,
        bool activeOnly = true)
        => await permissionService.GetAllAsync(tenantId, activeOnly);

    // Admin notification emails
    public async Task<AdminEmailDto?> GetAdminEmail(
        [Service] IAdminEmailService service,
        Guid adminEmailId,
        Guid tenantId)
        => await service.GetByIdAsync(adminEmailId, tenantId);

    public async Task<IEnumerable<AdminEmailDto>> GetAdminEmails(
        [Service] IAdminEmailService service,
        Guid tenantId,
        bool? isActive = null)
        => await service.GetAllAsync(tenantId, isActive);

    // System Setting Queries
    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<SystemSettingDto?> GetSystemSetting(
        [Service] ISystemSettingService service,
        Guid settingId,
        Guid tenantId)
    {
        return await service.GetByIdAsync(settingId, tenantId);
    }

    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<IEnumerable<SystemSettingDto>> GetSystemSettings(
        [Service] ISystemSettingService service,
        Guid tenantId,
        string? category = null)
    {
        if (!string.IsNullOrEmpty(category))
            return await service.GetByCategoryAsync(category, tenantId);
        return await service.GetAllAsync(tenantId);
    }

    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<SystemSettingDto?> GetSystemSettingByKey(
        [Service] ISystemSettingService service,
        string key,
        Guid tenantId)
    {
        return await service.GetByKeyAsync(key, tenantId);
    }

    // Audit Log Queries
    public async Task<AuditLogDto?> GetAuditLog(
        [Service] IAuditLogService service,
        Guid auditLogId)
    {
        return await service.GetByIdAsync(auditLogId);
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogs(
        [Service] IAuditLogService service,
        Guid? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (userId.HasValue)
            return await service.GetByUserAsync(userId.Value);
        if (!string.IsNullOrEmpty(action))
            return await service.GetByActionAsync(action);
        if (fromDate.HasValue && toDate.HasValue)
            return await service.GetByDateRangeAsync(fromDate.Value, toDate.Value);
        return await service.GetAllAsync();
    }

    // System Log Queries
    public async Task<SystemLogDto?> GetSystemLog(
        [Service] ISystemLogService service,
        Guid logId)
    {
        return await service.GetByIdAsync(logId);
    }

    public async Task<IEnumerable<SystemLogDto>> GetSystemLogs(
        [Service] ISystemLogService service,
        string? level = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (!string.IsNullOrEmpty(level))
            return await service.GetByLevelAsync(level);
        if (userId.HasValue)
            return await service.GetByUserAsync(userId.Value);
        if (fromDate.HasValue && toDate.HasValue)
            return await service.GetByDateRangeAsync(fromDate.Value, toDate.Value);
        return await service.GetAllAsync();
    }
}

