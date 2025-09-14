using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.Tests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = MockUserManager<User>();
        _signInManagerMock = MockSignInManager<User>();
        _roleManagerMock = MockRoleManager<ApplicationRole>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _emailServiceMock = new Mock<IEmailService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _roleManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _emailServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "test@test.com", Password = "password123" };
        var user = new User 
        { 
            Id = 1, 
            Email = "test@test.com", 
            FirstName = "Test", 
            LastName = "User", 
            IsActive = true 
        };
        var userDto = new UserDto { Id = 1, Email = "test@test.com", FirstName = "Test", LastName = "User" };
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var expiration = DateTime.UtcNow.AddHours(1);

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(SignInResult.Success);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user))
            .ReturnsAsync(accessToken);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        _jwtTokenServiceMock.Setup(x => x.GetTokenExpiration(accessToken))
            .Returns(expiration);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accessToken, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(expiration, result.Expires);
        Assert.Equal(userDto.Id, result.User.Id);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = "password123" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "test@test.com", Password = "password123" };
        var user = new User { Email = "test@test.com", IsActive = false };

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "test@test.com", Password = "wrongpassword" };
        var user = new User { Email = "test@test.com", IsActive = true };

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginRequest.Email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest 
        { 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test@test.com", 
            Password = "password123", 
            ConfirmPassword = "password123", 
            Role = UserRoles.User 
        };
        var user = new User { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com" };
        var userDto = new UserDto { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com" };
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var expiration = DateTime.UtcNow.AddHours(1);

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((User)null);
        _mapperMock.Setup(x => x.Map<User>(registerRequest))
            .Returns(user);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), registerRequest.Role))
            .ReturnsAsync(IdentityResult.Success);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(accessToken);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        _jwtTokenServiceMock.Setup(x => x.GetTokenExpiration(accessToken))
            .Returns(expiration);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { UserRoles.User });
        _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accessToken, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(expiration, result.Expires);
        Assert.Equal(userDto.Id, result.User.Id);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerRequest = new RegisterRequest { Email = "existing@test.com", Role = UserRoles.User };
        var existingUser = new User { Email = "existing@test.com" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(registerRequest));
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerRequest = new RegisterRequest { Email = "test@test.com", Role = "InvalidRole" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(registerRequest));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequest { Token = "valid-token", RefreshToken = "valid-refresh-token" };
        var userId = 1;
        var user = new User 
        { 
            Id = userId, 
            RefreshToken = "valid-refresh-token", 
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1), 
            IsActive = true 
        };
        var userDto = new UserDto { Id = userId };
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        var expiration = DateTime.UtcNow.AddHours(1);

        var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            new[] { new System.Security.Claims.Claim("userId", userId.ToString()) }));

        _jwtTokenServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(refreshTokenRequest.Token))
            .Returns(principal);
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user))
            .ReturnsAsync(newAccessToken);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);
        _jwtTokenServiceMock.Setup(x => x.GetTokenExpiration(newAccessToken))
            .Returns(expiration);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newAccessToken, result.Token);
        Assert.Equal(newRefreshToken, result.RefreshToken);
        Assert.Equal(expiration, result.Expires);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsSecurityTokenException()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequest { Token = "invalid-token", RefreshToken = "refresh-token" };

        _jwtTokenServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(refreshTokenRequest.Token))
            .Throws<SecurityTokenException>();

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => _authService.RefreshTokenAsync(refreshTokenRequest));
    }

    [Fact]
    public async Task RevokeTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var user = new User { RefreshToken = refreshToken };
        var users = new List<User> { user }.AsQueryable();

        _userManagerMock.Setup(x => x.Users).Returns(users);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RevokeTokenAsync(refreshToken);

        // Assert
        Assert.True(result);
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task RevokeTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var refreshToken = "invalid-refresh-token";
        var users = new List<User>().AsQueryable();

        _userManagerMock.Setup(x => x.Users).Returns(users);

        // Act
        var result = await _authService.RevokeTokenAsync(refreshToken);

        // Assert
        Assert.False(result);
    }

    // Helper methods to create mocks for Identity classes
    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }

    private static Mock<SignInManager<TUser>> MockSignInManager<TUser>() where TUser : class
    {
        var userManager = MockUserManager<TUser>().Object;
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
        return new Mock<SignInManager<TUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
    }

    private static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        return new Mock<RoleManager<TRole>>(store.Object, null, null, null, null);
    }
}