namespace ERP.Domain.Core;

public class City : BaseEntity
{
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
}
