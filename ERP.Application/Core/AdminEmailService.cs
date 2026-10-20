using System.Net.Mail;
using ERP.Domain.Core;
using ERP.Infrastructure.Repositories.Core;
using ERP.Shared.DTOs.Core;
using ERP.Shared.Exceptions;

namespace ERP.Application.Core;

public interface IAdminEmailService
{
    Task<IReadOnlyList<AdminEmailDto>> GetAllAsync(Guid tenantId, bool? isActive = null);
    Task<AdminEmailDto?> GetByIdAsync(Guid id, Guid tenantId);
    Task<AdminEmailDto> CreateAsync(CreateAdminEmailDto dto, Guid tenantId, Guid userId);
    Task<AdminEmailDto> UpdateAsync(Guid id, UpdateAdminEmailDto dto, Guid tenantId, Guid userId);
    Task DeleteAsync(Guid id, Guid tenantId, Guid userId);
}

public class AdminEmailService : IAdminEmailService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Notification", "Finance", "Support", "Sales", "Other"
    };

    private readonly IAdminEmailRepository _repo;

    public AdminEmailService(IAdminEmailRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<AdminEmailDto>> GetAllAsync(Guid tenantId, bool? isActive = null)
    {
        var rows = await _repo.GetAllAsync(tenantId, isActive);
        return rows.Select(Map).ToList();
    }

    public async Task<AdminEmailDto?> GetByIdAsync(Guid id, Guid tenantId)
    {
        var row = await _repo.GetByIdAsync(id, tenantId);
        return row == null ? null : Map(row);
    }

    public async Task<AdminEmailDto> CreateAsync(CreateAdminEmailDto dto, Guid tenantId, Guid userId)
    {
        var email = NormalizeEmail(dto.Email);
        var type = NormalizeType(dto.Type);
        await EnsureUniqueAsync(tenantId, email, excludeId: null);

        var entity = new AdminEmail
        {
            TenantId = tenantId,
            Email = email,
            Type = type,
            IsActive = true,
            CreatedBy = userId.ToString(),
        };
        await _repo.AddAsync(entity);
        return Map(entity);
    }

    public async Task<AdminEmailDto> UpdateAsync(Guid id, UpdateAdminEmailDto dto, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId)
            ?? throw new NotFoundException("Admin email not found.");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = NormalizeEmail(dto.Email);
            await EnsureUniqueAsync(tenantId, email, excludeId: id);
            entity.Email = email;
        }
        if (!string.IsNullOrWhiteSpace(dto.Type))
            entity.Type = NormalizeType(dto.Type);
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateAsync(entity);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, Guid tenantId, Guid userId)
    {
        var entity = await _repo.GetByIdAsync(id, tenantId)
            ?? throw new NotFoundException("Admin email not found.");
        entity.IsActive = false;
        entity.UpdatedBy = userId.ToString();
        await _repo.UpdateAsync(entity);
    }

    private async Task EnsureUniqueAsync(Guid tenantId, string email, Guid? excludeId)
    {
        var existing = await _repo.FindByEmailAsync(tenantId, email);
        if (existing != null && existing.IsActive && existing.Id != excludeId)
            throw new ValidationException("An active admin email with this address already exists.",
                new List<string> { "Email must be unique per organization." });
    }

    private static string NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email.Trim(), out var parsed))
            throw new ValidationException("Enter a valid email address.",
                new List<string> { "Email is required and must be valid." });
        return parsed.Address;
    }

    private static string NormalizeType(string? type)
    {
        var value = string.IsNullOrWhiteSpace(type) ? "Notification" : type.Trim();
        if (!AllowedTypes.Contains(value))
            throw new ValidationException("Choose a valid email type.",
                new List<string> { $"Type must be one of: {string.Join(", ", AllowedTypes)}." });
        return AllowedTypes.First(t => t.Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    private static AdminEmailDto Map(AdminEmail e) => new()
    {
        Id = e.Id,
        Email = e.Email,
        Type = e.Type,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
    };
}
