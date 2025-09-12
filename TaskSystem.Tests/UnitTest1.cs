using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;
using TaskSystem.Infrastructure.Data;
using TaskSystem.Infrastructure.Repositories;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.Tests;

public class AuthServiceTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var mockJwtService = new Mock<IJwtTokenService>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        // Add test user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!");
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = passwordHash,
            Role = UserRoles.User,
            IsActive = true
        };
        
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("test-token");
        mockJwtService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        mockJwtService.Setup(x => x.GetTokenExpiration("test-token")).Returns(DateTime.UtcNow.AddHours(1));

        var authService = new AuthService(userRepository, mockJwtService.Object, mockEmailService.Object, mockLogger.Object);

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var result = await authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("Test User", result.User.FullName);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var mockJwtService = new Mock<IJwtTokenService>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        var authService = new AuthService(userRepository, mockJwtService.Object, mockEmailService.Object, mockLogger.Object);

        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "wrongpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var mockJwtService = new Mock<IJwtTokenService>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<AuthService>>();

        mockJwtService.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("test-token");
        mockJwtService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        mockJwtService.Setup(x => x.GetTokenExpiration("test-token")).Returns(DateTime.UtcNow.AddHours(1));

        var authService = new AuthService(userRepository, mockJwtService.Object, mockEmailService.Object, mockLogger.Object);

        var registerRequest = new RegisterRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = "newuser@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await authService.RegisterAsync(registerRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("New User", result.User.FullName);
        Assert.Equal(UserRoles.User, result.User.Role);
    }
}