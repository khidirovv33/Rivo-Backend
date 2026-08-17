using Microsoft.EntityFrameworkCore;
using Rivo.Domain.Constants;
using Rivo.Domain.Entities.Permissions;

namespace Rivo.Infrastructure.Persistence.Seed;

/// <summary>Seeds the global (non-tenant) permission catalog on startup. Idempotent — only inserts names that don't exist yet.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.Permissions.Select(p => p.Name).ToListAsync(cancellationToken);
        var existingSet = existingNames.ToHashSet();

        var toAdd = PermissionNames.All()
            .Where(name => !existingSet.Contains(name))
            .Select(name =>
            {
                var parts = name.Split('.', 2);
                return new Permission
                {
                    Name = name,
                    Module = parts[0],
                    Action = parts.Length > 1 ? parts[1] : name
                };
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            context.Permissions.AddRange(toAdd);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
