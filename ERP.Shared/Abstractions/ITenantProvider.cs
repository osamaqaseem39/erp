namespace ERP.Shared.Abstractions;

public interface ITenantProvider
{
    Guid? GetTenantId();
}
