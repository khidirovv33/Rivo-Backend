using Rivo.Application.Common.Models;
using Rivo.Application.Users.Dtos;

namespace Rivo.Application.Users.Interfaces;

public interface IUsersService
{
    Task<UserDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<UserDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequestDto request, Guid createdBy, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid tenantId, Guid id, UpdateUserRequestDto request, Guid updatedBy, CancellationToken cancellationToken = default);
    Task BlockAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task UnblockAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
}
