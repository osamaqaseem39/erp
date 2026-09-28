namespace ERP.Shared.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ERP.SaaS";
    public string Audience { get; set; } = "ERP.SaaS.Users";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
