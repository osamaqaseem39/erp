namespace ERP.Shared.Constants;

public static class AppConstants
{
    public const string JWT_SECRET_KEY = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
    public const string JWT_ISSUER = "ERP.SaaS";
    public const string JWT_AUDIENCE = "ERP.SaaS.Users";
    public const int JWT_EXPIRY_MINUTES = 60;
    public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;
    
    public const string REDIS_CONNECTION_STRING = "localhost:6379";
    public const string SQL_CONNECTION_STRING = "Server=HELLOWORLD\\SQLEXPRESS;Database=erp;Trusted_Connection=True;TrustServerCertificate=True;";
    
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