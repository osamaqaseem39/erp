using ERP.Shared.Abstractions;

namespace ERP.Api.Services;

public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Items.TryGetValue("TenantId", out var tenantId) == true && tenantId is Guid id)
        {
            return id;
        }

        return null;
    }
}
