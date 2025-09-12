using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.API.Data.Services;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.Tests;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
        
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
        
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _emailServiceMock = new Mock<IEmailService>();

        _userService = new UserService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUserDto()
    {
        // Arrange
        var userId = "user1";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        
        var userDto = new UserDto
        {
            Id = userId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = "nonexistent";
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ReturnsUserDto()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User",
            Role = "User"
        };
        
        var user = new ApplicationUser
        {
            Id = "newuser1",
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName
        };
        
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(createUserDto.Email)).ReturnsAsync((ApplicationUser?)null);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(createUserDto.Role)).ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<ApplicationUser>(createUserDto)).Returns(user);
        _userManagerMock.Setup(x => x.CreateAsync(user, createUserDto.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, createUserDto.Role)).ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(userDto);
        _emailServiceMock.Setup(x => x.SendWelcomeEmailAsync(user.Email!, user.FullName)).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.CreateUserAsync(createUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createUserDto.Email, result.Email);
        Assert.Equal(createUserDto.FirstName, result.FirstName);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Email = "existing@example.com",
            Password = "password123",
            FirstName = "Test",
            LastName = "User",
            Role = "User"
        };
        
        var existingUser = new ApplicationUser { Email = createUserDto.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(createUserDto.Email)).ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(createUserDto));
        Assert.Contains("User with this email already exists", exception.Message);
    }

    [Fact]
    public async Task AssignRoleAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var assignRoleDto = new AssignRoleDto
        {
            UserId = "user1",
            Role = "Manager"
        };
        
        var user = new ApplicationUser { Id = assignRoleDto.UserId };
        
        _userManagerMock.Setup(x => x.FindByIdAsync(assignRoleDto.UserId)).ReturnsAsync(user);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(assignRoleDto.Role)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, assignRoleDto.Role)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.AssignRoleAsync(assignRoleDto);

        // Assert
        Assert.True(result);
    }
}