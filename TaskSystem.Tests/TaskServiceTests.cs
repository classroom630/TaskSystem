using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Infrastructure.Data;
using TaskSystem.Infrastructure.Repositories;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.Tests;

public class TaskServiceTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateTaskAsync_ValidRequest_ReturnsTaskDto()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<TaskService>>();

        // Add test user
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            UserName = "test@example.com",
            PasswordHash = "hash",
            // Role = UserRoles.User,
            IsActive = true
        };
        
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        var taskService = new TaskService(taskRepository, userRepository, mockLogger.Object);

        var createRequest = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await taskService.CreateTaskAsync(createRequest, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(TaskItemStatus.Pending, result.Status);
        Assert.Equal(user.Id, result.CreatedById);
    }

    [Fact]
    public async Task GetTasksForUserAsync_AdminRole_ReturnsAllTasks()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<TaskService>>();

        // Add test users
        var admin = new User { FirstName = "Admin", LastName = "User", Email = "admin@example.com", PasswordHash = "hash", // Role = UserRoles.Admin, IsActive = true };
        var user1 = new User { FirstName = "User", LastName = "One", Email = "user1@example.com", PasswordHash = "hash", // Role = UserRoles.User, IsActive = true };
        var user2 = new User { FirstName = "User", LastName = "Two", Email = "user2@example.com", PasswordHash = "hash", // Role = UserRoles.User, IsActive = true };

        await userRepository.AddAsync(admin);
        await userRepository.AddAsync(user1);
        await userRepository.AddAsync(user2);
        await userRepository.SaveChangesAsync();

        // Add test tasks
        var tasks = new List<TaskItem>
        {
            new() { Title = "Task 1", CreatedById = user1.Id, Status = TaskItemStatus.Pending, Priority = TaskPriority.Medium },
            new() { Title = "Task 2", CreatedById = user2.Id, Status = TaskItemStatus.Pending, Priority = TaskPriority.Medium },
            new() { Title = "Task 3", CreatedById = admin.Id, Status = TaskItemStatus.Pending, Priority = TaskPriority.Medium }
        };

        foreach (var task in tasks)
        {
            await taskRepository.AddAsync(task);
        }
        await userRepository.SaveChangesAsync();

        var taskService = new TaskService(taskRepository, userRepository, mockLogger.Object);

        // Act
        var result = await taskService.GetTasksForUserAsync(admin.Id, UserRoles.Admin);

        // Assert
        Assert.Equal(3, result.Count()); // Admin should see all tasks
    }

    [Fact]
    public async Task GetTasksForUserAsync_UserRole_ReturnsOnlyUserTasks()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<TaskService>>();

        // Add test users
        var user1 = new User { FirstName = "User", LastName = "One", Email = "user1@example.com", PasswordHash = "hash", // Role = UserRoles.User, IsActive = true };
        var user2 = new User { FirstName = "User", LastName = "Two", Email = "user2@example.com", PasswordHash = "hash", // Role = UserRoles.User, IsActive = true };

        await userRepository.AddAsync(user1);
        await userRepository.AddAsync(user2);
        await userRepository.SaveChangesAsync();

        // Add test tasks
        var tasks = new List<TaskItem>
        {
            new() { Title = "Task 1", CreatedById = user1.Id, Status = TaskItemStatus.Pending, Priority = TaskPriority.Medium },
            new() { Title = "Task 2", CreatedById = user2.Id, Status = TaskItemStatus.Pending, Priority = TaskPriority.Medium },
            new() { Title = "Task 3", CreatedById = user1.Id, AssignedToId = user1.Id, Status = TaskItemStatus.Pending, Priority = TaskPriority.Medium }
        };

        foreach (var task in tasks)
        {
            await taskRepository.AddAsync(task);
        }
        await userRepository.SaveChangesAsync();

        var taskService = new TaskService(taskRepository, userRepository, mockLogger.Object);

        // Act
        var result = await taskService.GetTasksForUserAsync(user1.Id, UserRoles.User);

        // Assert
        Assert.Equal(2, result.Count()); // User should only see tasks they created or are assigned to
        Assert.All(result, t => Assert.True(t.CreatedById == user1.Id || t.AssignedToId == user1.Id));
    }

    [Fact]
    public async Task UpdateTaskAsync_UserUpdatingOwnTask_Success()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<TaskService>>();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            UserName = "test@example.com",
            PasswordHash = "hash",
            // Role = UserRoles.User,
            IsActive = true
        };
        
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        var task = new TaskItem
        {
            Title = "Original Title",
            CreatedById = user.Id,
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Medium
        };
        
        await taskRepository.AddAsync(task);
        await userRepository.SaveChangesAsync();

        var taskService = new TaskService(taskRepository, userRepository, mockLogger.Object);

        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High
        };

        // Act
        var result = await taskService.UpdateTaskAsync(task.Id, updateRequest, user.Id, UserRoles.User);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
        Assert.Equal(TaskPriority.High, result.Priority);
    }

    [Fact]
    public async Task UpdateTaskAsync_UserUpdatingOthersTask_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var userRepository = new UserRepository(context);
        var taskRepository = new TaskRepository(context);
        var mockLogger = new Mock<ILogger<TaskService>>();

        var user1 = new User { FirstName = "User", LastName = "One", Email = "user1@example.com", PasswordHash = "hash", // Role = UserRoles.User, IsActive = true };
        var user2 = new User { FirstName = "User", LastName = "Two", Email = "user2@example.com", PasswordHash = "hash", // Role = UserRoles.User, IsActive = true };

        await userRepository.AddAsync(user1);
        await userRepository.AddAsync(user2);
        await userRepository.SaveChangesAsync();

        var task = new TaskItem
        {
            Title = "Task by User 1",
            CreatedById = user1.Id,
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Medium
        };
        
        await taskRepository.AddAsync(task);
        await userRepository.SaveChangesAsync();

        var taskService = new TaskService(taskRepository, userRepository, mockLogger.Object);

        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated by User 2",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            taskService.UpdateTaskAsync(task.Id, updateRequest, user2.Id, UserRoles.User));
    }
}