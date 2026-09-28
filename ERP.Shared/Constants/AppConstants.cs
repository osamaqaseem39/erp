namespace ERP.Shared.Constants;

/// <summary>
/// Non-secret application defaults. Secrets and connection strings come from configuration.
/// </summary>
public static class AppConstants
{
    public const string JWT_ISSUER = "ERP.SaaS";
    public const string JWT_AUDIENCE = "ERP.SaaS.Users";
    public const int JWT_EXPIRY_MINUTES = 60;
    public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;

    public static class Roles
    {
        public const string SUPER_ADMIN = "SuperAdmin";
        public const string ADMIN = "Admin";
        public const string MANAGER = "Manager";
        public const string USER = "User";
    }

    public static class Permissions
    {
        public const string READ = "Read";
        public const string WRITE = "Write";
        public const string DELETE = "Delete";
        public const string MANAGE = "Manage";
    }
}
