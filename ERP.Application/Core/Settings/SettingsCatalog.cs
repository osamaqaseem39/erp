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
    public const string Finance = "finance";
    public const string Crm = "crm";
    public const string Inventory = "inventory";
    public const string Procurement = "procurement";
    public const string Sales = "sales";

    public static readonly IReadOnlyList<string> Categories =
        [Preferences, Security, Email, Finance, Crm, Inventory, Procurement, Sales];

    public const string RequireSalesmanOnCustomerKey = "crm.requireSalesmanOnCustomer";
    public const string RequireBinKey = "inventory.requireBin";
    public const string ThreeWayMatchKey = "procurement.threeWayMatch";
    public const string ThreeWayMatchToleranceKey = "procurement.threeWayMatchTolerancePercent";
    public const string AllowUnpostKey = "sales.allowUnpost";
    public const string EnforceBudgetKey = "finance.enforceBudget";
    public const string RequireOpenPeriodKey = "finance.requireOpenPeriod";

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

        // Security: enforced by sign-in, sessions and password changes (SecurityPolicy).
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

        // Finance / CoA setup (Accounts module)
        new("finance.coa.maxDepth", Finance, "Chart of accounts max depth", SettingType.Number, "4",
            "Maximum hierarchy levels (1–8). Level 4 is the usual posting leaf.", Min: 1, Max: 8, Required: true),
        new("finance.coa.valueDecimalPlaces", Finance, "Amount decimal places", SettingType.Number, "4",
            "Decimals used to validate voucher and journal amounts (1–8).", Min: 1, Max: 8, Required: true),
        new("finance.coa.postingLeafOnly", Finance, "Posting leaf accounts only", SettingType.Boolean, "true",
            "When true, only accounts without children may have posting allowed."),
        new("finance.coa.requireCostCenterOnExpense", Finance, "Require cost center on expense", SettingType.Boolean, "false",
            "When true, expense debit lines on vouchers must include a cost center."),
        new("finance.voucher.allowPostWithoutApprove", Finance, "Allow post without approve", SettingType.Boolean, "true",
            "When true, journals may post from Draft; when false, only from Approved."),
        // Last-used typed-voucher defaults (Default Voucher UX) — written on CPV/CRV/BPV/BRV create.
        new(LastCashOrBankAccountIdKey, Finance, "Last cash/bank account", SettingType.Text, null,
            "Remembered after creating a cash/bank voucher; used when the next voucher omits CashOrBankAccountId.", Max: 36),
        new(LastCostCenterIdKey, Finance, "Last cost center", SettingType.Text, null,
            "Remembered from the first offset line with a cost center on a cash/bank voucher.", Max: 36),
        new(EnforceBudgetKey, Finance, "Enforce budget on post", SettingType.Boolean, "false",
            "When true, journal/PO amounts over budget are blocked."),
        new(RequireOpenPeriodKey, Finance, "Require open accounting period", SettingType.Boolean, "true",
            "When true, posts are blocked into closed accounting periods."),

        // Customer-depth feature toggles
        new(RequireSalesmanOnCustomerKey, Crm, "Require salesman on customer", SettingType.Boolean, "false",
            "When true, customer create/update must include SalesmanId."),
        new(RequireBinKey, Inventory, "Require warehouse bin", SettingType.Boolean, "false",
            "When true, stock moves must include WarehouseLocationId."),
        new(ThreeWayMatchKey, Procurement, "Three-way match on PI approve", SettingType.Boolean, "true",
            "When true, purchase invoice approve requires PO+GRN qty/price within tolerance."),
        new(ThreeWayMatchToleranceKey, Procurement, "Three-way match tolerance %", SettingType.Number, "2",
            "Allowed variance percent between PO, GRN and PI.", Min: 0, Max: 100),
        new(AllowUnpostKey, Sales, "Allow sales invoice unpost", SettingType.Boolean, "false",
            "When true, posted SI/CN may reopen to Draft if unpaid and period open; otherwise void-only."),
    ];

    /// <summary>Settings key for last typed-voucher cash/bank CoA.</summary>
    public const string LastCashOrBankAccountIdKey = "finance.voucher.lastCashOrBankAccountId";
    /// <summary>Settings key for last typed-voucher cost center.</summary>
    public const string LastCostCenterIdKey = "finance.voucher.lastCostCenterId";

    private static readonly Dictionary<string, SettingDefinition> ByKey = All.ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);

    public static SettingDefinition? Find(string key) => ByKey.GetValueOrDefault(key);

    public static IEnumerable<SettingDefinition> InCategory(string category) =>
        All.Where(d => string.Equals(d.Category, category, StringComparison.OrdinalIgnoreCase));
}
