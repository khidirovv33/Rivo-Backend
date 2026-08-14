using Rivo.Application.Auth.Dtos;

namespace Rivo.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResultDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default);
}
