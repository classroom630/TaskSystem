using Microsoft.AspNetCore.Identity;
using Moq;
using TaskSystem.API.Data.Services;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;
using TaskSystem.API.Data.Repositories;

namespace TaskSystem.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
        
        var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };
        userManager.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync((ApplicationUser?)null);
        
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var signInManager = new Mock<SignInManager<ApplicationUser>>(userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
        
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mapper = new Mock<AutoMapper.IMapper>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>();
        var emailService = new Mock<IEmailService>();

        var authService = new AuthService(
            userManager.Object,
            signInManager.Object,
            refreshTokenRepository.Object,
            mapper.Object,
            configuration.Object,
            logger.Object,
            emailService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
        
        var registerDto = new RegisterDto 
        { 
            Email = "existing@example.com", 
            Password = "password123", 
            FirstName = "Test", 
            LastName = "User",
            Role = "User"
        };
        
        var existingUser = new ApplicationUser { Email = registerDto.Email };
        userManager.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync(existingUser);
        
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var signInManager = new Mock<SignInManager<ApplicationUser>>(userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
        
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mapper = new Mock<AutoMapper.IMapper>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>();
        var emailService = new Mock<IEmailService>();

        var authService = new AuthService(
            userManager.Object,
            signInManager.Object,
            refreshTokenRepository.Object,
            mapper.Object,
            configuration.Object,
            logger.Object,
            emailService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => authService.RegisterAsync(registerDto));
        Assert.Contains("User already exists", exception.Message);
    }
}