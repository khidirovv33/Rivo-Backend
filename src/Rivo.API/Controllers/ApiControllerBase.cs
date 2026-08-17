using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid TenantId =>
        Guid.TryParse(User.FindFirstValue("tenant_id"), out var id)
            ? id
            : throw new UnauthorizedAccessException("Request is missing a valid tenant claim.");

    protected Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessException("Request is missing a valid user claim.");
}
