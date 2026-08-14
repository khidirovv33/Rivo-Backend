namespace Rivo.Application.Common.Interfaces;

/// <summary>Unit-of-work seam so Application services can commit repository changes without depending on EF Core directly.</summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
