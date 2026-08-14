using Microsoft.AspNetCore.Http;
using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.Multitenancy;

public class TenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
