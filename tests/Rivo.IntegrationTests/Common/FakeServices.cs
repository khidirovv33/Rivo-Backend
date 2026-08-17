using Rivo.Application.Common.Interfaces;

namespace Rivo.IntegrationTests.Common;

public class FakeCurrentTenantService : ICurrentTenantService
{
    public Guid? TenantId { get; set; } = Guid.NewGuid();
}

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();

    public string? Email => "tester@rivo.local";

    public string? RoleName => "Owner";

    public string? IpAddress => "127.0.0.1";

    public bool IsAuthenticated => true;
}

public class FakeDateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
