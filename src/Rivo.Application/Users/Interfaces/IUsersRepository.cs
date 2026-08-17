using Rivo.Domain.Entities.Users;

namespace Rivo.Application.Users.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<(List<User> Items, int TotalCount)> GetPagedAsync(Guid tenantId, int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Remove(User user);
}
