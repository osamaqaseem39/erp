using ERP.Application.Interfaces.Core;
using IPermissionService = ERP.Application.Interfaces.Core.IPermissionService;
using ERP.Domain.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces.Core;
using ERP.Infrastructure.Interfaces.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.Constants;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;
using ERP.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Core;

public class SetupService : ISetupService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IPermissionService _permissionService;
    private readonly ERPDbContext _context;

    public SetupService(
        ITenantRepository tenantRepository,
        ICompanyRepository companyRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        IPermissionService permissionService,
        ERPDbContext context)
    {
        _tenantRepository = tenantRepository;
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _permissionService = permissionService;
        _context = context;
    }

    public async Task<bool> IsSuperAdminExistsAsync()
    {
        return await _context.UserRoles.AnyAsync(ur =>
            _context.Roles.Any(r =>
                r.Id == ur.RoleId &&
                r.RoleName == AppConstants.Roles.SUPER_ADMIN));
    }

    public async Task<SuperAdminSetupResponseDto> SetupSuperAdminAsync(SuperAdminSetupDto dto)
    {
        // Check if super admin already exists
        if (await IsSuperAdminExistsAsync())
        {
            throw new ValidationException("Super admin already exists. Setup can only be performed once.");
        }

        // Validate email is not already in use
        // Setup runs before anyone can sign in, so it uses the context directly (no tenant in the request).
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.PrimaryEmail == dto.Email);
        if (existingUser != null)
        {
            throw new ValidationException($"Email {dto.Email} is already registered.");
        }

        // Validate domain is not already in use
        var existingTenant = await _tenantRepository.GetByDomainAsync(dto.Domain);
        if (existingTenant != null)
        {
            throw new ValidationException($"Domain {dto.Domain} is already in use.");
        }

        // Create tenant
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            TenantId = tenantId, // Self-reference: Tenant's TenantId should equal its Id
            TenantName = dto.TenantName,
            Domain = dto.Domain,
            Industry = dto.Industry ?? string.Empty,
            Country = dto.Country ?? string.Empty,
            Status = "Active",
            DatabaseName = $"ERP_{dto.Domain.Replace(".", "_")}",
            ConnectionString = $"Server=localhost;Database=ERP_{dto.Domain.Replace(".", "_")};Trusted_Connection=true;TrustServerCertificate=true;",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _tenantRepository.CreateAsync(tenant);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            TenantId = tenant.Id,
            CompanyName = dto.TenantName,
            LegalName = dto.TenantName,
            Country = dto.Country ?? string.Empty,
            Email = dto.Email,
            Phone = dto.Phone ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
        await _companyRepository.CreateAsync(company);

        // Create SuperAdmin role if it doesn't exist
        var superAdminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == AppConstants.Roles.SUPER_ADMIN && r.TenantId == tenant.Id);
        
        if (superAdminRole == null)
        {
            superAdminRole = new Role
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                RoleName = AppConstants.Roles.SUPER_ADMIN,
                Description = "Super Administrator with full system access",
                IsSystemRole = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            _context.Roles.Add(superAdminRole);
            await _context.SaveChangesAsync();
        }

        // Create super admin user (sp_CreateUser requires all NOT NULL columns)
        var (passwordHash, passwordSalt) = PasswordHelper.HashPassword(dto.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            CompanyId = companyId,
            Salutation = string.Empty,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FullName = $"{dto.FirstName} {dto.LastName}",
            Gender = string.Empty,
            JobTitle = string.Empty,
            Department = string.Empty,
            Nationality = string.Empty,
            PrimaryEmail = dto.Email,
            SecondaryEmail = string.Empty,
            PrimaryPhone = dto.Phone ?? string.Empty,
            SecondaryPhone = string.Empty,
            MobilePhone = string.Empty,
            Fax = string.Empty,
            LinkedIn = string.Empty,
            Twitter = string.Empty,
            Facebook = string.Empty,
            Instagram = string.Empty,
            PreferredCommunicationMethod = string.Empty,
            LanguagePreference = "en",
            TimeZone = "UTC",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsTenantOwner = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        // Assign SuperAdmin role to user
        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = userId,
            RoleId = superAdminRole.Id,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _userRoleRepository.AddAsync(userRole);

        await _permissionService.SeedDefaultPermissionsAsync(tenant.Id, "System");
        await _permissionService.AssignAllPermissionsToRoleAsync(superAdminRole.Id, tenant.Id);

        return new SuperAdminSetupResponseDto
        {
            Success = true,
            Message = "Super admin setup completed successfully",
            TenantId = tenant.Id,
            UserId = userId,
            Email = dto.Email
        };
    }
}

