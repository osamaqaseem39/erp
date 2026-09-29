using System.Globalization;
using System.Net.Mail;

namespace ERP.Application.Core.Settings;

public enum SettingType
{
    Text,
    Number,
    Boolean,
    Choice,
    Email,
    Secret,
}

public sealed record SettingOption(string Value, string Label);

/// <summary>One tenant setting the application understands. Stored in SystemSettings under <see cref="Key"/>.</summary>
public sealed record SettingDefinition(
    string Key,
    string Category,
    string Label,
    SettingType Type,
    string? Default = null,
    string? Description = null,
    IReadOnlyList<SettingOption>? Options = null,
    int? Min = null,
    int? Max = null,
    bool Required = false,
    bool ReadOnly = false)
{
    public bool IsSecret => Type == SettingType.Secret;

    /// <summary>Returns an error message, or null when the value is valid. Values arrive as strings.</summary>
    public string? Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Required ? $"{Label} is required." : null;

        switch (Type)
        {
            case SettingType.Number:
                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return $"{Label} must be a whole number.";
                if (Min.HasValue && number < Min) return $"{Label} must be at least {Min}.";
                if (Max.HasValue && number > Max) return $"{Label} must be at most {Max}.";
                return null;
            case SettingType.Boolean:
                return value is "true" or "false" ? null : $"{Label} must be true or false.";
            case SettingType.Choice:
                return Options!.Any(o => o.Value == value) ? null : $"Choose a valid option for {Label}.";
            case SettingType.Email:
                return MailAddress.TryCreate(value, out _) ? null : $"{Label} must be a valid email address.";
            default:
                return Max.HasValue && value.Length > Max ? $"{Label} cannot exceed {Max} characters." : null;
        }
    }
}

/// <summary>
/// Every tenant setting, in one place. Pages render from this list and the API validates against it,
/// so there are no free-form keys that nothing reads.
/// </summary>
public static class SettingsCatalog
{
    public const string Preferences = "preferences";
    public const string Security = "security";
    public const string Email = "email";

    public static readonly IReadOnlyList<string> Categories = [Preferences, Security, Email];

    private static readonly SettingOption[] DateFormats =
    [
        new("dd/MM/yyyy", "31/12/2026"),
        new("MM/dd/yyyy", "12/31/2026"),
        new("yyyy-MM-dd", "2026-12-31"),
        new("dd MMM yyyy", "31 Dec 2026"),
    ];

    private static readonly SettingOption[] Locales =
    [
        new("en-US", "English (United States)"),
        new("en-GB", "English (United Kingdom)"),
        new("en-PK", "English (Pakistan)"),
        new("ur-PK", "Urdu (Pakistan)"),
        new("ar-SA", "Arabic (Saudi Arabia)"),
        new("de-DE", "German (Germany)"),
        new("fr-FR", "French (France)"),
    ];

    private static readonly SettingOption[] Weekdays =
    [
        new("0", "Sunday"),
        new("1", "Monday"),
        new("6", "Saturday"),
    ];

    public static readonly IReadOnlyList<SettingDefinition> All =
    [
        // Preferences: how numbers, money and dates are shown to everyone in the tenant.
        new("preferences.locale", Preferences, "Language and region", SettingType.Choice, "en-US",
            "Controls number, currency and date formatting.", Locales, Required: true),
        new("preferences.dateFormat", Preferences, "Date format", SettingType.Choice, "dd MMM yyyy",
            "Used on documents and exports.", DateFormats, Required: true),
        new("preferences.firstDayOfWeek", Preferences, "First day of the week", SettingType.Choice, "1",
            "Used by calendars and weekly reports.", Weekdays, Required: true),

        // Security: stored now; enforced when Checkpoint 2 (authentication) lands.
        new("security.sessionTimeoutMinutes", Security, "Session timeout (minutes)", SettingType.Number, "60",
            "Signed-in sessions expire after this much time.", Min: 5, Max: 1440, Required: true),
        new("security.passwordMinLength", Security, "Minimum password length", SettingType.Number, "8",
            Min: 8, Max: 128, Required: true),
        new("security.lockoutThreshold", Security, "Failed sign-ins before lockout", SettingType.Number, "5",
            Min: 3, Max: 20, Required: true),
        new("security.lockoutMinutes", Security, "Lockout duration (minutes)", SettingType.Number, "30",
            Min: 1, Max: 1440, Required: true),

        // Email: stored now; nothing sends mail yet.
        new("email.smtpHost", Email, "SMTP server", SettingType.Text, null, "For example smtp.office365.com", Max: 200),
        new("email.smtpPort", Email, "SMTP port", SettingType.Number, "587", Min: 1, Max: 65535),
        new("email.useSsl", Email, "Use SSL/TLS", SettingType.Boolean, "true"),
        new("email.smtpUser", Email, "SMTP user name", SettingType.Text, null, Max: 200),
        new("email.smtpPassword", Email, "SMTP password", SettingType.Secret, null,
            "Stored encrypted. It can be replaced but never read back."),
        new("email.fromAddress", Email, "Send from address", SettingType.Email, null, Max: 200),
        new("email.fromName", Email, "Send from name", SettingType.Text, null, Max: 100),
    ];

    private static readonly Dictionary<string, SettingDefinition> ByKey = All.ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);

    public static SettingDefinition? Find(string key) => ByKey.GetValueOrDefault(key);

    public static IEnumerable<SettingDefinition> InCategory(string category) =>
        All.Where(d => string.Equals(d.Category, category, StringComparison.OrdinalIgnoreCase));
}
