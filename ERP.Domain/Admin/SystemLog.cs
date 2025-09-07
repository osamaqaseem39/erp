using ERP.Domain;

namespace ERP.Domain.Admin;

public class SystemLog : BaseEntity
{
    public string LogLevel { get; set; } = string.Empty; // Info, Warning, Error, Debug
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? Method { get; set; }
    public Guid? UserId { get; set; }
    public string? IPAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Properties { get; set; } // JSON string
}