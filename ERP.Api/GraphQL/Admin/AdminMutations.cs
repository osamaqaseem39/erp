using HotChocolate;
using HotChocolate.Authorization;
using ERP.Application.Core;
using ERP.Application.Interfaces.Admin;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.DTOs.Core;
using CreateRoleDto = ERP.Shared.DTOs.Admin.CreateRoleDto;
using CreateUserDto = ERP.Shared.DTOs.Admin.CreateUserDto;
using RoleDto = ERP.Shared.DTOs.Admin.RoleDto;
using UpdateRoleDto = ERP.Shared.DTOs.Admin.UpdateRoleDto;
using UpdateUserDto = ERP.Shared.DTOs.Admin.UpdateUserDto;
using UserDto = ERP.Shared.DTOs.Admin.UserDto;

namespace ERP.Api.GraphQL.Admin;

[ExtendObjectType("Mutation")]
[Authorize]
public class AdminMutations
{
    // User Mutations
    public async Task<UserDto> AddUser(
        [Service] IUserService service,
        CreateUserDto input)
    {
        var id = await service.CreateAsync(input);
        return await service.GetByIdAsync(id) ?? throw new Exception("Failed to create user");
    }

    public async Task<UserDto> UpdateUser(
        [Service] IUserService service,
        UpdateUserDto input)
    {
        var success = await service.UpdateAsync(input);
        if (!success)
            throw new Exception("Failed to update user");
        return await service.GetByIdAsync(input.Id) ?? throw new Exception("User not found");
    }

    public async Task<bool> DeleteUser(
        [Service] IUserService service,
        Guid userId)
    {
        return await service.DeleteAsync(userId);
    }

    public async Task<bool> ActivateUser(
        [Service] IUserService service,
        Guid userId)
        => await service.ActivateAsync(userId);

    public async Task<bool> DeactivateUser(
        [Service] IUserService service,
        Guid userId)
        => await service.DeactivateAsync(userId);

    public async Task<bool> ResetUserPassword(
        [Service] IUserService service,
        ResetUserPasswordDto input)
    {
        return await service.ResetPasswordAsync(input);
    }

    // Role Mutations
    public async Task<RoleDto> AddRole(
        [Service] IRoleService service,
        CreateRoleDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    public async Task<RoleDto> UpdateRole(
        [Service] IRoleService service,
        UpdateRoleDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    public async Task<bool> DeleteRole(
        [Service] IRoleService service,
        Guid roleId,
        Guid tenantId)
    {
        return await service.DeleteAsync(roleId, tenantId);
    }

    public async Task<bool> AssignRolePermissions(
        [Service] IRoleService service,
        Guid roleId,
        AssignPermissionsDto input,
        Guid tenantId,
        Guid userId)
    {
        input.RoleId = roleId;
        return await service.AssignPermissionsAsync(roleId, input, tenantId, userId);
    }

    // System Setting Mutations
    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<SystemSettingDto> AddSystemSetting(
        [Service] ISystemSettingService service,
        CreateSystemSettingDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.CreateAsync(input, tenantId, userId);
    }

    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<SystemSettingDto> UpdateSystemSetting(
        [Service] ISystemSettingService service,
        UpdateSystemSettingDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpdateAsync(input, tenantId, userId);
    }

    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<SystemSettingDto> UpsertSystemSetting(
        [Service] ISystemSettingService service,
        UpsertSystemSettingDto input,
        Guid tenantId,
        Guid userId)
    {
        return await service.UpsertAsync(input, tenantId, userId);
    }

    [HotChocolate.Authorization.Authorize(Roles = new[] { "SuperAdmin", "SUPER_ADMIN" })]
    public async Task<bool> DeleteSystemSetting(
        [Service] ISystemSettingService service,
        Guid settingId,
        Guid tenantId)
    {
        return await service.DeleteAsync(settingId, tenantId);
    }

    // Admin notification emails
    public async Task<AdminEmailDto> AddAdminEmail(
        [Service] IAdminEmailService service,
        CreateAdminEmailDto input,
        Guid tenantId,
        Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<AdminEmailDto> UpdateAdminEmail(
        [Service] IAdminEmailService service,
        Guid adminEmailId,
        UpdateAdminEmailDto input,
        Guid tenantId,
        Guid userId)
        => await service.UpdateAsync(adminEmailId, input, tenantId, userId);

    public async Task<bool> DeleteAdminEmail(
        [Service] IAdminEmailService service,
        Guid adminEmailId,
        Guid tenantId,
        Guid userId)
    {
        await service.DeleteAsync(adminEmailId, tenantId, userId);
        return true;
    }
}

