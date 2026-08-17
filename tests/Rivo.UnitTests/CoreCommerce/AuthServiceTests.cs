using Microsoft.Extensions.Logging;
using Moq;
using Rivo.Application.Auth.Dtos;
using Rivo.Application.Auth.Interfaces;
using Rivo.Application.Auth.Services;
using Rivo.Application.Common.Interfaces;
using Rivo.Application.Permissions.Interfaces;
using Rivo.Application.Roles.Interfaces;
using Rivo.Application.Tenancy.Interfaces;
using Rivo.Application.Users.Interfaces;
using Rivo.Domain.Entities.Permissions;
using Rivo.Domain.Entities.Roles;
using Rivo.Domain.Entities.Users;
using Rivo.Domain.Enums;
using Rivo.Domain.Exceptions;

namespace Rivo.UnitTests.CoreCommerce;

public class AuthServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IRolesRepository> _rolesRepository = new();
    private readonly Mock<IPermissionsRepository> _permissionsRepository = new();
    private readonly Mock<IAuthRepository> _authRepository = new();
    private readonly Mock<ITenantsRepository> _tenantsRepository = new();
    private readonly Mock<IPasswordHasherService> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IApplicationDbContext> _dbContext = new();
    private readonly Mock<IDateTimeService> _dateTime = new();

    private AuthService CreateSut()
    {
        _permissionsRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());
        _jwtTokenService.Setup(j => j.RefreshTokenLifetime).Returns(TimeSpan.FromDays(7));
        _dateTime.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);

        return new AuthService(
            _usersRepository.Object,
            _rolesRepository.Object,
            _permissionsRepository.Object,
            _authRepository.Object,
            _tenantsRepository.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _emailService.Object,
            _dbContext.Object,
            _dateTime.Object,
            Mock.Of<ILogger<AuthService>>());
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsValidationAppException()
    {
        _usersRepository.Setup(r => r.ExistsByEmailAsync("owner@shop.uz", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut();
        var request = new RegisterRequestDto { CompanyName = "Shop", FullName = "Owner", Email = "owner@shop.uz", Password = "Passw0rd!" };

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsAuthenticationFailedException()
    {
        var user = new User { Email = "cashier@shop.uz", PasswordHash = "hashed", Status = UserStatus.Active };
        _usersRepository.Setup(r => r.GetByEmailAsync("cashier@shop.uz", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(p => p.VerifyPassword("hashed", "wrong")).Returns(false);

        var sut = CreateSut();
        var request = new LoginRequestDto { Email = "cashier@shop.uz", Password = "wrong" };

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => sut.LoginAsync(request, "127.0.0.1"));
    }

    [Fact]
    public async Task LoginAsync_WhenUserBlocked_ThrowsAuthenticationFailedException()
    {
        var user = new User { Email = "cashier@shop.uz", PasswordHash = "hashed", Status = UserStatus.Blocked };
        _usersRepository.Setup(r => r.GetByEmailAsync("cashier@shop.uz", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = CreateSut();
        var request = new LoginRequestDto { Email = "cashier@shop.uz", Password = "anything" };

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => sut.LoginAsync(request, "127.0.0.1"));
        _passwordHasher.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_IssuesAccessAndRefreshTokens()
    {
        var role = new Role { Name = "Cashier" };
        var user = new User { Email = "cashier@shop.uz", PasswordHash = "hashed", Status = UserStatus.Active, RoleId = role.Id };

        _usersRepository.Setup(r => r.GetByEmailAsync("cashier@shop.uz", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(p => p.VerifyPassword("hashed", "Passw0rd!")).Returns(true);
        _rolesRepository.Setup(r => r.GetByIdAsync(user.RoleId, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _jwtTokenService.Setup(j => j.GenerateAccessToken(user)).Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenService.Setup(j => j.GenerateRefreshTokenValue()).Returns("refresh-token");

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequestDto { Email = "cashier@shop.uz", Password = "Passw0rd!" }, "127.0.0.1");

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("Cashier", result.RoleName);
        _authRepository.Verify(a => a.AddRefreshTokenAsync(It.Is<Rivo.Domain.Entities.Auth.RefreshToken>(t => t.Token == "refresh-token"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
