namespace ERP.Shared.DTOs.Core;

public class CountryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateCountryDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdateCountryDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public class CityDto
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateCityDto
{
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateCityDto
{
    public Guid? CountryId { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

public class BranchDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Guid? CityId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBranchDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Guid? CityId { get; set; }
}

public class UpdateBranchDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public Guid? CityId { get; set; }
    public bool? IsActive { get; set; }
}

public class PaymentTermDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePaymentTermDto
{
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public string? Description { get; set; }
}

public class UpdatePaymentTermDto
{
    public string? Name { get; set; }
    public int? Days { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
