using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.Inventory;

public class WarehouseDto : BaseDto
{
    public Guid CompanyId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public class CreateWarehouseDto : CreateDto
{
    public Guid CompanyId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateWarehouseDto : UpdateDto
{
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public string? Description { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ContactPerson { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}