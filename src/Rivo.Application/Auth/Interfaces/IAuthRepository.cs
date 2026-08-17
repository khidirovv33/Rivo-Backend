using Rivo.Domain.Entities.Auth;

namespace Rivo.Application.Auth.Interfaces;

public interface IAuthRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    void UpdateRefreshToken(RefreshToken refreshToken);
}
