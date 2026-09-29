using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERP.Shared.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Shared.Helpers;

public static class JwtHelper
{
    private static JwtSettings _settings = new();

    public static void Configure(JwtSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (string.IsNullOrWhiteSpace(settings.SecretKey) || settings.SecretKey.Length < 32)
            throw new InvalidOperationException("Jwt:SecretKey must be configured and at least 32 characters.");
        _settings = settings;
    }

    public static string GenerateJwtToken(Guid userId, string email, IEnumerable<string> roles, Guid tenantId, Guid? companyId = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new("TenantId", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            claims.Add(new Claim("CompanyId", companyId.Value.ToString()));
        }

        foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!claims.Any(c => c.Type == ClaimTypes.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>Backward-compatible overload. </summary>
    public static string GenerateJwtToken(Guid userId, string email, string role, Guid tenantId)
        => GenerateJwtToken(userId, email, new[] { role }, tenantId);

    public static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
            ValidateLifetime = false,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public static int ExpiryMinutes => _settings.ExpiryMinutes;
    public static int RefreshTokenExpiryDays => _settings.RefreshTokenExpiryDays;
}
