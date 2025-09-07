using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Admin;

public class SystemLogDto : BaseDto
{
    public string LogLevel { get; set; } = string.Empty; // Info, Warning, Error, Debug
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? ResponseStatusCode { get; set; }
    public long? ResponseTime { get; set; } // in milliseconds
    public string? AdditionalData { get; set; } // JSON string
}

public class CreateSystemLogDto : CreateDto
{
    public string Level { get; set; } = string.Empty;
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? UserId { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? ResponseStatusCode { get; set; }
    public long? ResponseTime { get; set; }
    public string? AdditionalData { get; set; }
}

public class UpdateSystemLogDto : UpdateDto
{
    public string? LogLevel { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? UserId { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? ResponseStatusCode { get; set; }
    public long? ResponseTime { get; set; }
    public string? AdditionalData { get; set; }
}