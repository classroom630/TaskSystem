using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", CreatedById = 1, CreatedBy = new User { FirstName = "John", LastName = "Doe" } },
            new TaskItem { Id = 2, Title = "Task 2", CreatedById = 2, CreatedBy = new User { FirstName = "Jane", LastName = "Smith" } }
        };

        _taskRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Id == 1 && t.Title == "Task 1");
        Assert.Contains(result, t => t.Id == 2 && t.Title == "Task 2");
    }

    [Fact]
    public async Task GetTasksByUserAsync_ReturnsUserAssignedAndCreatedTasks()
    {
        // Arrange
        var userId = 1;
        var assignedTasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Assigned Task", AssignedToId = userId, CreatedById = 2, CreatedBy = new User { FirstName = "Manager", LastName = "User" } }
        };
        var createdTasks = new List<TaskItem>
        {
            new TaskItem { Id = 2, Title = "Created Task", CreatedById = userId, CreatedBy = new User { FirstName = "John", LastName = "Doe" } }
        };

        _taskRepositoryMock.Setup(x => x.GetByAssignedUserAsync(userId))
            .ReturnsAsync(assignedTasks);
        _taskRepositoryMock.Setup(x => x.GetByCreatedUserAsync(userId))
            .ReturnsAsync(createdTasks);

        // Act
        var result = await _taskService.GetTasksByUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Id == 1 && t.Title == "Assigned Task");
        Assert.Contains(result, t => t.Id == 2 && t.Title == "Created Task");
    }

    [Fact]
    public async Task GetTasksForUserAsync_AdminRole_ReturnsAllTasks()
    {
        // Arrange
        var userId = 1;
        var userRole = UserRoles.Admin;
        var allTasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", CreatedById = 1, CreatedBy = new User { FirstName = "User", LastName = "One" } },
            new TaskItem { Id = 2, Title = "Task 2", CreatedById = 2, CreatedBy = new User { FirstName = "User", LastName = "Two" } }
        };

        _taskRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(allTasks);

        // Act
        var result = await _taskService.GetTasksForUserAsync(userId, userRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _taskRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTasksForUserAsync_ManagerRole_ReturnsAllTasks()
    {
        // Arrange
        var userId = 1;
        var userRole = UserRoles.Manager;
        var allTasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", CreatedById = 1, CreatedBy = new User { FirstName = "User", LastName = "One" } }
        };

        _taskRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(allTasks);

        // Act
        var result = await _taskService.GetTasksForUserAsync(userId, userRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count());
        _taskRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTasksForUserAsync_UserRole_ReturnsOnlyUserTasks()
    {
        // Arrange
        var userId = 1;
        var userRole = UserRoles.User;
        var assignedTasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Assigned Task", AssignedToId = userId, CreatedById = 2, CreatedBy = new User { FirstName = "Manager", LastName = "User" } }
        };
        var createdTasks = new List<TaskItem>
        {
            new TaskItem { Id = 2, Title = "Created Task", CreatedById = userId, CreatedBy = new User { FirstName = "John", LastName = "Doe" } }
        };

        _taskRepositoryMock.Setup(x => x.GetByAssignedUserAsync(userId))
            .ReturnsAsync(assignedTasks);
        _taskRepositoryMock.Setup(x => x.GetByCreatedUserAsync(userId))
            .ReturnsAsync(createdTasks);

        // Act
        var result = await _taskService.GetTasksForUserAsync(userId, userRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _taskRepositoryMock.Verify(x => x.GetByAssignedUserAsync(userId), Times.Once);
        _taskRepositoryMock.Verify(x => x.GetByCreatedUserAsync(userId), Times.Once);
        _taskRepositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithExistingTask_ReturnsTaskDto()
    {
        // Arrange
        var taskId = 1;
        var task = new TaskItem 
        { 
            Id = taskId, 
            Title = "Test Task", 
            Description = "Test Description", 
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Medium,
            CreatedById = 1, 
            CreatedBy = new User { FirstName = "Creator", LastName = "User" } 
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(TaskItemStatus.Pending, result.Status);
        Assert.Equal(TaskPriority.Medium, result.Priority);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithNonExistingTask_ReturnsNull()
    {
        // Arrange
        var taskId = 999;

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidRequest_ReturnsTaskDto()
    {
        // Arrange
        var createRequest = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedToId = 2
        };
        var createdById = 1;
        var assignedUser = new User { Id = 2, IsActive = true };
        var createdTask = new TaskItem 
        { 
            Id = 1, 
            Title = "New Task", 
            Description = "New Description",
            Priority = TaskPriority.High,
            Status = TaskItemStatus.Pending,
            CreatedById = createdById,
            AssignedToId = 2,
            CreatedBy = new User { FirstName = "Creator", LastName = "User" },
            AssignedTo = assignedUser
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(assignedUser);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync(It.IsAny<TaskItem>());
        _taskRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTaskAsync(createRequest, createdById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(TaskItemStatus.Pending, result.Status);
        Assert.Equal(createdById, result.CreatedById);
        Assert.Equal(2, result.AssignedToId);
    }

    [Fact]
    public async Task CreateTaskAsync_WithInvalidAssignedUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var createRequest = new CreateTaskRequest
        {
            Title = "New Task",
            AssignedToId = 999
        };
        var createdById = 1;

        _userRepositoryMock.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateTaskAsync(createRequest, createdById));
    }

    [Fact]
    public async Task CreateTaskAsync_WithInactiveAssignedUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var createRequest = new CreateTaskRequest
        {
            Title = "New Task",
            AssignedToId = 2
        };
        var createdById = 1;
        var inactiveUser = new User { Id = 2, IsActive = false };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(inactiveUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateTaskAsync(createRequest, createdById));
    }

    [Fact]
    public async Task UpdateTaskAsync_WithValidRequestAndPermissions_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = 1;
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High,
            AssignedToId = 3
        };
        var currentUserId = 1;
        var userRole = UserRoles.Admin;
        var existingTask = new TaskItem 
        { 
            Id = taskId, 
            Title = "Old Task", 
            CreatedById = 1,
            CreatedBy = new User { FirstName = "Creator", LastName = "User" }
        };
        var assignedUser = new User { Id = 3, IsActive = true };
        var updatedTask = new TaskItem 
        { 
            Id = taskId, 
            Title = "Updated Task", 
            Description = "Updated Description",
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High,
            AssignedToId = 3,
            CreatedById = 1,
            CreatedBy = new User { FirstName = "Creator", LastName = "User" },
            AssignedTo = assignedUser
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(3))
            .ReturnsAsync(assignedUser);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _taskRepositoryMock.SetupSequence(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask)
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, updateRequest, currentUserId, userRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Task", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(3, result.AssignedToId);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNonExistingTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var taskId = 999;
        var updateRequest = new UpdateTaskRequest { Title = "Updated Task" };
        var currentUserId = 1;
        var userRole = UserRoles.Admin;

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.UpdateTaskAsync(taskId, updateRequest, currentUserId, userRole));
    }

    [Fact]
    public async Task UpdateTaskAsync_WithoutPermissions_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = 1;
        var updateRequest = new UpdateTaskRequest { Title = "Updated Task" };
        var currentUserId = 2;
        var userRole = UserRoles.User;
        var existingTask = new TaskItem { Id = taskId, CreatedById = 1, AssignedToId = 3 }; // Different user created and assigned

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.UpdateTaskAsync(taskId, updateRequest, currentUserId, userRole));
    }

    [Fact]
    public async Task DeleteTaskAsync_WithValidPermissions_ReturnsTrue()
    {
        // Arrange
        var taskId = 1;
        var currentUserId = 1;
        var userRole = UserRoles.Admin;
        var existingTask = new TaskItem { Id = taskId, CreatedById = 1 };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.DeleteAsync(existingTask))
            .Returns(Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, currentUserId, userRole);

        // Assert
        Assert.True(result);
        _taskRepositoryMock.Verify(x => x.DeleteAsync(existingTask), Times.Once);
        _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithNonExistingTask_ReturnsFalse()
    {
        // Arrange
        var taskId = 999;
        var currentUserId = 1;
        var userRole = UserRoles.Admin;

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, currentUserId, userRole);

        // Assert
        Assert.False(result);
        _taskRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTaskAsync_WithoutPermissions_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var taskId = 1;
        var currentUserId = 2;
        var userRole = UserRoles.User;
        var existingTask = new TaskItem { Id = taskId, CreatedById = 1, AssignedToId = 3 }; // Different user created and assigned

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.DeleteTaskAsync(taskId, currentUserId, userRole));
    }

    [Fact]
    public async Task UpdateTaskAsync_UserCanModifyOwnCreatedTask()
    {
        // Arrange
        var taskId = 1;
        var updateRequest = new UpdateTaskRequest { Title = "Updated Task", Status = TaskItemStatus.Completed, Priority = TaskPriority.Low };
        var currentUserId = 1;
        var userRole = UserRoles.User;
        var existingTask = new TaskItem 
        { 
            Id = taskId, 
            Title = "Old Task", 
            CreatedById = currentUserId, // User created this task
            CreatedBy = new User { FirstName = "Creator", LastName = "User" }
        };
        var updatedTask = new TaskItem 
        { 
            Id = taskId, 
            Title = "Updated Task", 
            Status = TaskItemStatus.Completed,
            Priority = TaskPriority.Low,
            CreatedById = currentUserId,
            CreatedBy = new User { FirstName = "Creator", LastName = "User" }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _taskRepositoryMock.SetupSequence(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask)
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, updateRequest, currentUserId, userRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Task", result.Title);
        Assert.Equal(TaskItemStatus.Completed, result.Status);
        Assert.Equal(TaskPriority.Low, result.Priority);
    }

    [Fact]
    public async Task UpdateTaskAsync_UserCanModifyAssignedTask()
    {
        // Arrange
        var taskId = 1;
        var updateRequest = new UpdateTaskRequest { Title = "Updated Task", Status = TaskItemStatus.InProgress, Priority = TaskPriority.High };
        var currentUserId = 2;
        var userRole = UserRoles.User;
        var existingTask = new TaskItem 
        { 
            Id = taskId, 
            Title = "Old Task", 
            CreatedById = 1, 
            AssignedToId = currentUserId, // User is assigned to this task
            CreatedBy = new User { FirstName = "Creator", LastName = "User" },
            AssignedTo = new User { FirstName = "Assignee", LastName = "User" }
        };
        var updatedTask = new TaskItem 
        { 
            Id = taskId, 
            Title = "Updated Task", 
            Status = TaskItemStatus.InProgress,
            Priority = TaskPriority.High,
            CreatedById = 1,
            AssignedToId = currentUserId,
            CreatedBy = new User { FirstName = "Creator", LastName = "User" },
            AssignedTo = new User { FirstName = "Assignee", LastName = "User" }
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);
        _taskRepositoryMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _taskRepositoryMock.SetupSequence(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(existingTask)
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, updateRequest, currentUserId, userRole);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Task", result.Title);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
        Assert.Equal(TaskPriority.High, result.Priority);
    }
}