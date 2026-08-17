using Microsoft.AspNetCore.Identity;
using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.Identity;

/// <summary>Wraps ASP.NET Core Identity's PBKDF2 hasher — no need for a User instance, so a throwaway one satisfies the generic constraint.</summary>
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object HasherContext = new();

    public string HashPassword(string password) => _hasher.HashPassword(HasherContext, password);

    public bool VerifyPassword(string passwordHash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(HasherContext, passwordHash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
