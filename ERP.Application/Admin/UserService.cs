using ERP.Application.Core.Auth;
using ERP.Application.Core.Settings;
using ERP.Shared.Exceptions;
using ERP.Application.Interfaces.Admin;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Admin;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace ERP.Application.Admin;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ERPDbContext _context;
    private readonly ISecurityPolicyProvider _policies;
    private readonly ISessionStore? _sessions;

    public UserService(
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        ERPDbContext context,
        ISecurityPolicyProvider? policies = null,
        ISessionStore? sessions = null)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _context = context;
        _policies = policies ?? new SecurityPolicyProvider(context);
        _sessions = sessions;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;
        return await MapToDtoAsync(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = (await _userRepository.GetAllAsync()).ToList();
        var rolesByUser = await LoadRolesByUserAsync(users.Select(u => u.Id));
        var dtos = users.Select(u => MapToDto(u, rolesByUser.GetValueOrDefault(u.Id))).ToList();
        await AttachEmployeesAsync(dtos);
        return dtos;
    }

    public async Task<Guid> CreateAsync(CreateUserDto dto)
    {
        if (dto.RoleIds == null || dto.RoleIds.Count == 0)
            throw new ValidationException("Assign at least one role (user type).",
                new List<string> { "RoleIds is required." });

        var email = NormalizeEmail(dto.Email);
        var username = NormalizeUsername(dto.Username, email);
        await EnsureEmailUniqueAsync(email);
        await EnsureUsernameUniqueAsync(username);

        (await _policies.GetAsync(_userRepository.TenantId)).EnsurePasswordAllowed(dto.Password, "password");
        var (passwordHash, passwordSalt) = PasswordHelper.HashPassword(dto.Password);

        // Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            // TenantId is set by the repository from the signed-in caller, whatever the request says.
            CompanyId = dto.CompanyId, // May be null for tenant owner or if caller allows
            Username = username,
            Email = email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FullName = $"{dto.FirstName} {dto.LastName}",
            PrimaryEmail = email,
            PrimaryPhone = Clean(dto.Phone) ?? string.Empty,
            SecondaryPhone = Clean(dto.SecondaryPhone) ?? string.Empty,
            MobilePhone = Clean(dto.MobilePhone) ?? string.Empty,
            AddressLine1 = Clean(dto.AddressLine1),
            City = Clean(dto.City),
            ProfilePicture = CleanUrl(dto.ProfilePicture),
            EmployeeId = await ResolveEmployeeIdAsync(dto.EmployeeId),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsActive = dto.IsActive,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy ?? "System"
        };

        // Set default values for required fields
        user.Salutation ??= string.Empty;
        user.Gender ??= string.Empty;
        user.JobTitle ??= string.Empty;
        user.Department ??= string.Empty;
        user.Nationality ??= string.Empty;
        user.SecondaryEmail ??= string.Empty;
        user.Fax ??= string.Empty;
        user.LinkedIn ??= string.Empty;
        user.Twitter ??= string.Empty;
        user.Facebook ??= string.Empty;
        user.Instagram ??= string.Empty;
        user.PreferredCommunicationMethod ??= string.Empty;
        user.LanguagePreference ??= string.Empty;
        user.TimeZone ??= string.Empty;

        // Create user
        var userId = await _userRepository.CreateAsync(user);

        var created = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User was created but could not be reloaded.");
        await ReplaceRolesAsync(created, dto.RoleIds, dto.CreatedBy ?? "System");

        return userId;
    }

    public async Task<bool> UpdateAsync(UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id);
        if (user == null) return false;

        if (dto.RoleIds == null || dto.RoleIds.Count == 0)
            throw new ValidationException("Assign at least one role (user type).",
                new List<string> { "RoleIds is required." });

        var email = NormalizeEmail(dto.Email);
        var username = NormalizeUsername(dto.Username, email);
        await EnsureEmailUniqueAsync(email, excludeUserId: user.Id);
        await EnsureUsernameUniqueAsync(username, excludeUserId: user.Id);

        // Update user properties
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.FullName = $"{dto.FirstName} {dto.LastName}";
        user.Username = username;
        user.Email = email;
        user.PrimaryEmail = email;
        user.PrimaryPhone = Clean(dto.Phone) ?? string.Empty;
        user.SecondaryPhone = Clean(dto.SecondaryPhone) ?? string.Empty;
        user.MobilePhone = Clean(dto.MobilePhone) ?? string.Empty;
        user.AddressLine1 = Clean(dto.AddressLine1);
        user.City = Clean(dto.City);
        user.ProfilePicture = CleanUrl(dto.ProfilePicture);
        user.EmployeeId = await ResolveEmployeeIdAsync(dto.EmployeeId, excludeUserId: user.Id);
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = dto.UpdatedBy;

        var ok = await _userRepository.UpdateAsync(user);
        if (!ok) return false;

        await ReplaceRolesAsync(user, dto.RoleIds, dto.UpdatedBy ?? "System");
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        return await _userRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UserDto>> GetByRoleAsync(Guid roleId)
    {
        var users = (await _userRepository.GetByRoleAsync(roleId)).ToList();
        var rolesByUser = await LoadRolesByUserAsync(users.Select(u => u.Id));
        var dtos = users.Select(u => MapToDto(u, rolesByUser.GetValueOrDefault(u.Id))).ToList();
        await AttachEmployeesAsync(dtos);
        return dtos;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return null;
        return await MapToDtoAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(ResetUserPasswordDto dto)
    {
        // Scoped to the caller's organization: another tenant's user is "not found".
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null) return false;

        (await _policies.GetAsync(user.TenantId)).EnsurePasswordAllowed(dto.NewPassword);
        var (hash, salt) = PasswordHelper.HashPassword(dto.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        await _context.SaveChangesAsync();
        // The user signs in again everywhere with the new password.
        if (_sessions != null) await _sessions.EndAllAsync(user.Id, SessionStore.ReasonPasswordChanged);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;
        if (user.IsActive) return true;
        user.IsActive = true;
        user.IsLocked = false;
        user.LockedUntil = null;
        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;
        if (!user.IsActive) return true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        if (_sessions != null)
            await _sessions.EndAllAsync(user.Id, SessionStore.ReasonSignOutEverywhere);
        return true;
    }

    private async Task ReplaceRolesAsync(User user, List<Guid> roleIds, string actor)
    {
        var distinct = roleIds.Distinct().ToList();
        var validRoleIds = await _context.Roles
            .Where(r => distinct.Contains(r.Id) && r.TenantId == user.TenantId && r.IsActive)
            .Select(r => r.Id)
            .ToListAsync();
        if (validRoleIds.Count != distinct.Count)
            throw new ValidationException("One or more roles don't exist in your organization.");

        var existing = await _context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();
        _context.UserRoles.RemoveRange(existing);
        foreach (var roleId in validRoleIds)
        {
            _context.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                TenantId = user.TenantId,
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actor
            });
        }
        await _context.SaveChangesAsync();
    }

    private async Task<UserDto> MapToDtoAsync(User user)
    {
        var map = await LoadRolesByUserAsync(new[] { user.Id });
        var dto = MapToDto(user, map.GetValueOrDefault(user.Id));
        await AttachEmployeesAsync(new List<UserDto> { dto });
        return dto;
    }

    private async Task<Dictionary<Guid, List<(Guid Id, string Name)>>> LoadRolesByUserAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new();

        var rows = await (
            from ur in _context.UserRoles.AsNoTracking()
            join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ids.Contains(ur.UserId)
            select new { ur.UserId, r.Id, r.RoleName }
        ).ToListAsync();

        return rows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => (x.Id, x.RoleName)).ToList());
    }

    private static UserDto MapToDto(User user, List<(Guid Id, string Name)>? roles)
    {
        roles ??= new List<(Guid Id, string Name)>();
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.PrimaryEmail ?? string.Empty,
            Username = user.Username,
            Phone = user.PrimaryPhone,
            SecondaryPhone = user.SecondaryPhone,
            MobilePhone = user.MobilePhone,
            AddressLine1 = user.AddressLine1,
            City = user.City,
            ProfilePicture = user.ProfilePicture,
            EmployeeId = user.EmployeeId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy,
            RoleIds = roles.Select(r => r.Id).ToList(),
            RoleNames = roles.Select(r => r.Name).ToList(),
        };
    }

    private async Task AttachEmployeesAsync(IList<UserDto> dtos)
    {
        var ids = dtos.Where(d => d.EmployeeId.HasValue).Select(d => d.EmployeeId!.Value).Distinct().ToList();
        if (ids.Count == 0) return;

        var employees = await _context.Employees.AsNoTracking()
            .Where(e => ids.Contains(e.Id) && !e.IsDeleted)
            .Select(e => new { e.Id, e.EmployeeCode, e.FirstName, e.LastName })
            .ToListAsync();
        var map = employees.ToDictionary(e => e.Id);
        foreach (var dto in dtos)
        {
            if (dto.EmployeeId is Guid eid && map.TryGetValue(eid, out var emp))
            {
                dto.EmployeeCode = emp.EmployeeCode;
                dto.EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim();
            }
        }
    }

    private async Task<Guid?> ResolveEmployeeIdAsync(Guid? employeeId, Guid? excludeUserId = null)
    {
        if (!employeeId.HasValue || employeeId == Guid.Empty) return null;

        var tenantId = _userRepository.TenantId;
        var employee = await _context.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.TenantId == tenantId && !e.IsDeleted);
        if (employee == null)
            throw new ValidationException("Employee not found in your organization.",
                new List<string> { "EmployeeId must refer to an active employee in this organization." });

        var taken = await _context.Users.AsNoTracking()
            .AnyAsync(u => u.EmployeeId == employeeId && u.TenantId == tenantId &&
                           (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
        if (taken)
            throw new ValidationException("That employee is already linked to another user.",
                new List<string> { "EmployeeId must be unique per user." });

        return employee.Id;
    }

    private async Task EnsureEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null && existing.Id != excludeUserId)
            throw new ValidationException("A user with this email already exists in your organization.",
                new List<string> { "Email must be unique per organization." });
    }

    private async Task EnsureUsernameUniqueAsync(string username, Guid? excludeUserId = null)
    {
        var existing = await _userRepository.GetByUsernameAsync(username);
        if (existing != null && existing.Id != excludeUserId)
            throw new ValidationException("A user with this username already exists in your organization.",
                new List<string> { "Username must be unique per organization." });
    }

    private static string NormalizeEmail(string? email)
    {
        var value = Clean(email);
        if (string.IsNullOrEmpty(value) || !value.Contains('@'))
            throw new ValidationException("Enter a valid email address.",
                new List<string> { "Email is required and must be valid." });
        return value.ToLowerInvariant();
    }

    private static string NormalizeUsername(string? username, string email)
    {
        var value = Clean(username) ?? email;
        if (value.Length > 100) value = value[..100];
        return value;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? CleanUrl(string? value)
    {
        var url = Clean(value);
        if (url == null) return null;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ValidationException("Profile picture must be an http(s) URL.",
                new List<string> { "ProfilePicture must be a valid URL." });
        return uri.ToString();
    }
}

