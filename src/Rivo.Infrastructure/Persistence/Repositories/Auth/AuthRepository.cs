using Microsoft.EntityFrameworkCore;
using Rivo.Application.Auth.Interfaces;
using Rivo.Domain.Entities.Auth;

namespace Rivo.Infrastructure.Persistence.Repositories.Auth;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default) =>
        _context.RefreshTokens.Include(rt => rt.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public void UpdateRefreshToken(RefreshToken refreshToken) => _context.RefreshTokens.Update(refreshToken);
}
