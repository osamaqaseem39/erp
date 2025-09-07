using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class EmployeeAddressDto : BaseDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string AddressType { get; set; } = string.Empty; // Home, Office, Emergency
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
}

public class CreateEmployeeAddressDto : CreateDto
{
    public Guid EmployeeId { get; set; }
    public string AddressType { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool? IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateEmployeeAddressDto : UpdateDto
{
    public string? AddressType { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool? IsPrimary { get; set; }
    public string? Notes { get; set; }
}