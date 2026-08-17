using Microsoft.EntityFrameworkCore;
using Rivo.Application.Users.Interfaces;
using Rivo.Domain.Entities.Users;

namespace Rivo.Infrastructure.Persistence.Repositories.Users;

public class UsersRepository : IUsersRepository
{
    private readonly ApplicationDbContext _context;

    public UsersRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.Include(u => u.Role).IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        // Login/registration must find a user before their tenant is known — bypass the tenant filter deliberately.
        _context.Users.Include(u => u.Role).IgnoreQueryFilters()
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email, cancellationToken);

    public Task<List<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Users.Include(u => u.Role).Where(u => u.TenantId == tenantId).ToListAsync(cancellationToken);

    public async Task<(List<User> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Include(u => u.Role).Where(u => u.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user) => _context.Users.Update(user);

    public void Remove(User user) => _context.Users.Remove(user);
}
