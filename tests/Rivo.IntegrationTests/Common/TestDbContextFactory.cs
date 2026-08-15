using Microsoft.EntityFrameworkCore;
using Rivo.Application.Common.Interfaces;
using Rivo.Infrastructure.Persistence;

namespace Rivo.IntegrationTests.Common;

/// <summary>
/// EF Core InMemory-провайдер — реальный DbContext (модель, конфигурации, tenant query filter,
/// SaveChanges-хуки) прогоняется целиком, только без реальной Postgres.
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(ICurrentTenantService? currentTenant = null, IDateTimeService? dateTime = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(
            options,
            currentTenant ?? new FakeCurrentTenantService(),
            dateTime ?? new FakeDateTimeService());
    }
}
