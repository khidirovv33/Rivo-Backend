using Rivo.Application.Common.Models;
using Rivo.Application.Returns.Dtos;

namespace Rivo.Application.Returns.Interfaces;

public interface IReturnsService
{
    Task<ReturnDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<ReturnDto>> GetPagedAsync(Guid tenantId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<ReturnDto> CreateAsync(Guid tenantId, Guid processedByUserId, CreateReturnRequestDto request, CancellationToken cancellationToken = default);
}
