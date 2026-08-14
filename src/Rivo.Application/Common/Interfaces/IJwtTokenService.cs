using Rivo.Domain.Entities.Users;

namespace Rivo.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshTokenValue();
    TimeSpan RefreshTokenLifetime { get; }
}
