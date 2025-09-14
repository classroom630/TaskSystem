using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.Tests;

public class UserServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = MockUserManager<User>();
        _roleManagerMock = MockRoleManager<ApplicationRole>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _taskRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsListOfUserDtos()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        }.AsQueryable();

        var userDtos = new List<UserDto>
        {
            new UserDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Role = "User" },
            new UserDto { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", Role = "Admin" }
        };

        _userManagerMock.Setup(x => x.Users).Returns(users);
        _mapperMock.SetupSequence(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDtos[0])
            .Returns(userDtos[1]);
        _userManagerMock.SetupSequence(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "User" })
            .ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, u => u.Id == 1 && u.FirstName == "John");
        Assert.Contains(result, u => u.Id == 2 && u.FirstName == "Jane");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, FirstName = "Test", LastName = "User", Email = "test@test.com" };
        var userDto = new UserDto { Id = userId, FirstName = "Test", LastName = "User", Email = "test@test.com", Role = "User" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = 999;

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidRequest_ReturnsUserDto()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = "new@test.com",
            Password = "password123",
            Role = UserRoles.User
        };
        var user = new User { Id = 1, FirstName = "New", LastName = "User", Email = "new@test.com" };
        var userDto = new UserDto { Id = 1, FirstName = "New", LastName = "User", Email = "new@test.com", Role = "User" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(createRequest.Email))
            .ReturnsAsync((User)null);
        _mapperMock.Setup(x => x.Map<User>(createRequest))
            .Returns(user);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), createRequest.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), createRequest.Role))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { UserRoles.User });

        // Act
        var result = await _userService.CreateUserAsync(createRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("new@test.com", result.Email);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var createRequest = new CreateUserRequest { Email = "existing@test.com", Role = UserRoles.User };
        var existingUser = new User { Email = "existing@test.com" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(createRequest.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var createRequest = new CreateUserRequest { Email = "test@test.com", Role = "InvalidRole" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(createRequest.Email))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(createRequest));
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidRequest_ReturnsUpdatedUserDto()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@test.com",
            Role = UserRoles.Admin,
            IsActive = true
        };
        var user = new User { Id = userId, FirstName = "Old", LastName = "User", Email = "old@test.com" };
        var userDto = new UserDto { Id = userId, FirstName = "Updated", LastName = "User", Email = "updated@test.com", Role = "Admin" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.FindByEmailAsync(updateRequest.Email))
            .ReturnsAsync((User)null);
        _userManagerMock.SetupSequence(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" })      // First call for role change
            .ReturnsAsync(new List<string> { "Admin" });    // Second call for mapping
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, updateRequest.Role))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("updated@test.com", result.Email);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 999;
        var updateRequest = new UpdateUserRequest { FirstName = "Test", LastName = "User", Email = "test@test.com" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserAsync(userId, updateRequest));
    }

    [Fact]
    public async Task UpdateUserAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var updateRequest = new UpdateUserRequest { FirstName = "Test", LastName = "User", Email = "existing@test.com" };
        var user = new User { Id = userId, Email = "original@test.com" };
        var existingUser = new User { Id = 2, Email = "existing@test.com" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.FindByEmailAsync(updateRequest.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserAsync(userId, updateRequest));
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingUserAndNoTasks_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Email = "test@test.com" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _taskRepositoryMock.Setup(x => x.GetByAssignedUserAsync(userId))
            .ReturnsAsync(new List<TaskItem>());
        _taskRepositoryMock.Setup(x => x.GetByCreatedUserAsync(userId))
            .ReturnsAsync(new List<TaskItem>());
        _userManagerMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingUser_ReturnsFalse()
    {
        // Arrange
        var userId = 999;

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingTasks_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Email = "test@test.com" };
        var tasks = new List<TaskItem> { new TaskItem { Id = 1, Title = "Test Task" } };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _taskRepositoryMock.Setup(x => x.GetByAssignedUserAsync(userId))
            .ReturnsAsync(tasks);
        _taskRepositoryMock.Setup(x => x.GetByCreatedUserAsync(userId))
            .ReturnsAsync(new List<TaskItem>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task AssignRoleAsync_WithValidRoleAndUser_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var role = UserRoles.Manager;
        var user = new User { Id = userId, Email = "test@test.com" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.AssignRoleAsync(userId, role);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AssignRoleAsync_WithInvalidRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var invalidRole = "InvalidRole";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.AssignRoleAsync(userId, invalidRole));
    }

    [Fact]
    public async Task AssignRoleAsync_WithNonExistingUser_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        var role = UserRoles.Admin;

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userService.AssignRoleAsync(userId, role);

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

    private static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        return new Mock<RoleManager<TRole>>(store.Object, null, null, null, null);
    }
}