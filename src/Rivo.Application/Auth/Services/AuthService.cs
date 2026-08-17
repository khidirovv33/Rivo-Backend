using Microsoft.Extensions.Logging;
using Rivo.Application.Auth.Dtos;
using Rivo.Application.Auth.Interfaces;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Tenancy.Interfaces;
using Rivo.Application.Users.Interfaces;
using Rivo.Domain.Constants;
using Rivo.Domain.Entities.Auth;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Entities.Tenancy;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRolesRepository _rolesRepository;
    private readonly IPermissionsRepository _permissionsRepository;
    private readonly IAuthRepository _authRepository;
    private readonly ITenantsRepository _tenantsRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeService _dateTime;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsersRepository usersRepository,
        IRolesRepository rolesRepository,
        IPermissionsRepository permissionsRepository,
        IAuthRepository authRepository,
        ITenantsRepository tenantsRepository,
        IPasswordHasherService passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IApplicationDbContext dbContext,
        IDateTimeService dateTime,
        ILogger<AuthService> logger)
    {
        _usersRepository = usersRepository;
        _rolesRepository = rolesRepository;
        _permissionsRepository = permissionsRepository;
        _authRepository = authRepository;
        _tenantsRepository = tenantsRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _dbContext = dbContext;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _usersRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Email)] = new[] { "A user with this email already exists." }
            });
        }

        var tenant = new Tenant { CompanyName = request.CompanyName };
        await _tenantsRepository.AddAsync(tenant, cancellationToken);

        var allPermissions = await _permissionsRepository.GetAllAsync(cancellationToken);
        var permissionsByName = allPermissions.ToDictionary(p => p.Name);

        var roles = new List<Role>();
        foreach (var roleName in RoleNames.All)
        {
            roles.Add(new Role
            {
                TenantId = tenant.Id,
                Name = roleName,
                IsSystemRole = true
            });
        }
        await _rolesRepository.AddRangeAsync(roles, cancellationToken);

        var ownerRole = roles.First(r => r.Name == RoleNames.Owner);
        var user = new User
        {
            TenantId = tenant.Id,
            FullName = request.FullName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            RoleId = ownerRole.Id,
            Status = UserStatus.PendingVerification,
            EmailVerificationToken = Guid.NewGuid().ToString("N")
        };
        await _usersRepository.AddAsync(user, cancellationToken);

        // Persist tenant/roles/user first so their generated Ids are available for the role-permission links below.
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var role in roles)
        {
            if (!DefaultRolePermissions.Map.TryGetValue(role.Name, out var permissionNames))
            {
                continue;
            }

            var permissionIds = permissionNames
                .Where(permissionsByName.ContainsKey)
                .Select(name => permissionsByName[name].Id);

            await _permissionsRepository.ReplaceRolePermissionsAsync(role.Id, permissionIds, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, user.EmailVerificationToken!, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification email to {Email}", user.Email);
        }

        return await IssueTokensAsync(user, ownerRole, "registration", cancellationToken);
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || user.IsDeleted)
        {
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        if (user.Status == UserStatus.Blocked)
        {
            throw new AuthenticationFailedException("This account has been blocked.");
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        var role = await _rolesRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), user.RoleId);

        return await IssueTokensAsync(user, role, ipAddress, cancellationToken);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var existingToken = await _authRepository.GetRefreshTokenAsync(refreshToken, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            throw new AuthenticationFailedException("Invalid or expired refresh token.");
        }

        var user = existingToken.User ?? await _usersRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), existingToken.UserId);

        var role = await _rolesRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), user.RoleId);

        var newRefreshToken = CreateRefreshToken(user.Id, ipAddress);
        existingToken.RevokedAt = _dateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;
        _authRepository.UpdateRefreshToken(existingToken);

        await _authRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        return new AuthResultDto
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = role.Name,
            AccessToken = accessToken,
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = newRefreshToken.Token
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var existingToken = await _authRepository.GetRefreshTokenAsync(refreshToken, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            throw new NotFoundException("RefreshToken", refreshToken);
        }

        existingToken.RevokedAt = _dateTime.UtcNow;
        _authRepository.UpdateRefreshToken(existingToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null)
        {
            // Do not reveal whether the email exists.
            return;
        }

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiresAt = _dateTime.UtcNow.AddHours(1);
        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendPasswordResetAsync(user.Email, user.FullName, user.PasswordResetToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset email to {Email}", user.Email);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new AuthenticationFailedException("Invalid password reset request.");

        if (string.IsNullOrEmpty(user.PasswordResetToken)
            || user.PasswordResetToken != request.Token
            || user.PasswordResetTokenExpiresAt is null
            || user.PasswordResetTokenExpiresAt < _dateTime.UtcNow)
        {
            throw new AuthenticationFailedException("Invalid or expired password reset token.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            throw new AuthenticationFailedException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Email);

        if (string.IsNullOrEmpty(user.EmailVerificationToken) || user.EmailVerificationToken != request.Token)
        {
            throw new AuthenticationFailedException("Invalid email verification token.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        if (user.Status == UserStatus.PendingVerification)
        {
            user.Status = UserStatus.Active;
        }

        _usersRepository.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResultDto> IssueTokensAsync(User user, Role role, string ipAddress, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(user.Id, ipAddress);

        await _authRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = role.Name,
            AccessToken = accessToken,
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = refreshToken.Token
        };
    }

    private RefreshToken CreateRefreshToken(Guid userId, string ipAddress) => new()
    {
        UserId = userId,
        Token = _jwtTokenService.GenerateRefreshTokenValue(),
        ExpiresAt = _dateTime.UtcNow.Add(_jwtTokenService.RefreshTokenLifetime),
        CreatedByIp = ipAddress
    };
}
