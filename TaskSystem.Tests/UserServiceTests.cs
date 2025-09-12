using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Infrastructure.Data;
using TaskSystem.Infrastructure.Repositories;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.Tests;

public class UserServiceTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<UserService>>();

        // Add test users
        var users = new List<User>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@example.com", Role = UserRoles.User, IsActive = true, PasswordHash = "hash1" },
            new() { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Role = UserRoles.Manager, IsActive = true, PasswordHash = "hash2" }
        };

        foreach (var user in users)
        {
            await userRepository.AddAsync(user);
        }
        await userRepository.SaveChangesAsync();

        var userService = new UserService(userRepository, taskRepository, mockLogger.Object);

        // Act
        var result = await userService.GetAllUsersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, u => u.Email == "john@example.com");
        Assert.Contains(result, u => u.Email == "jane@example.com");
    }

    [Fact]
    public async Task CreateUserAsync_ValidRequest_ReturnsUserDto()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<UserService>>();

        var userService = new UserService(userRepository, taskRepository, mockLogger.Object);

        var createRequest = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@example.com",
            Password = "Password123!",
            Role = UserRoles.User
        };

        // Act
        var result = await userService.CreateUserAsync(createRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test User", result.FullName);
        Assert.Equal("testuser@example.com", result.Email);
        Assert.Equal(UserRoles.User, result.Role);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<UserService>>();

        // Add existing user
        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@example.com",
            PasswordHash = "hash",
            Role = UserRoles.User,
            IsActive = true
        };
        
        await userRepository.AddAsync(existingUser);
        await userRepository.SaveChangesAsync();

        var userService = new UserService(userRepository, taskRepository, mockLogger.Object);

        var createRequest = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "existing@example.com", // Same email
            Password = "Password123!",
            Role = UserRoles.User
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRole_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<UserService>>();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRoles.User,
            IsActive = true
        };
        
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        var userService = new UserService(userRepository, taskRepository, mockLogger.Object);

        // Act
        var result = await userService.AssignRoleAsync(user.Id, UserRoles.Manager);

        // Assert
        Assert.True(result);
        
        var updatedUser = await userRepository.GetByIdAsync(user.Id);
        Assert.Equal(UserRoles.Manager, updatedUser!.Role);
    }

    [Fact]
    public async Task AssignRoleAsync_InvalidRole_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<UserService>>();

        var userService = new UserService(userRepository, taskRepository, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => userService.AssignRoleAsync(1, "InvalidRole"));
    }
}