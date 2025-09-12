using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TaskSystem.API.Data.Repositories;
using TaskSystem.API.Data.Services;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TaskService>>();
        _emailServiceMock = new Mock<IEmailService>();

        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _userManagerMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidData_ReturnsTaskDto()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            AssignedToUserId = "user1"
        };
        
        var assignedUser = new ApplicationUser
        {
            Id = "user1",
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        
        var taskItem = new TaskItem
        {
            Id = 1,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Priority = createTaskDto.Priority,
            AssignedToUserId = createTaskDto.AssignedToUserId,
            CreatedByUserId = "creator1"
        };
        
        var taskDto = new TaskDto
        {
            Id = 1,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Priority = createTaskDto.Priority,
            AssignedToUserId = createTaskDto.AssignedToUserId
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(createTaskDto.AssignedToUserId)).ReturnsAsync(assignedUser);
        _mapperMock.Setup(x => x.Map<TaskItem>(createTaskDto)).Returns(taskItem);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>())).ReturnsAsync(taskItem);
        _taskRepositoryMock.Setup(x => x.GetTaskWithUserDetailsAsync(taskItem.Id)).ReturnsAsync(taskItem);
        _mapperMock.Setup(x => x.Map<TaskDto>(taskItem)).Returns(taskDto);
        _emailServiceMock.Setup(x => x.SendTaskAssignedEmailAsync(assignedUser.Email!, assignedUser.FullName, createTaskDto.Title)).Returns(Task.CompletedTask);

        // Act
        var result = await _taskService.CreateTaskAsync(createTaskDto, "creator1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createTaskDto.Title, result.Title);
        Assert.Equal(createTaskDto.AssignedToUserId, result.AssignedToUserId);
    }

    [Fact]
    public async Task CreateTaskAsync_WithInvalidUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto
        {
            Title = "Test Task",
            AssignedToUserId = "nonexistent"
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(createTaskDto.AssignedToUserId)).ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.CreateTaskAsync(createTaskDto, "creator1"));
    }

    [Fact]
    public async Task CanUserAccessTaskAsync_AdminUser_ReturnsTrue()
    {
        // Arrange
        var taskId = 1;
        var userId = "admin1";
        var userRoles = new List<string> { "Admin" };
        var task = new TaskItem { Id = taskId, AssignedToUserId = "other-user" };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act
        var result = await _taskService.CanUserAccessTaskAsync(taskId, userId, userRoles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserAccessTaskAsync_OwnerUser_ReturnsTrue()
    {
        // Arrange
        var taskId = 1;
        var userId = "user1";
        var userRoles = new List<string> { "User" };
        var task = new TaskItem { Id = taskId, AssignedToUserId = userId };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act
        var result = await _taskService.CanUserAccessTaskAsync(taskId, userId, userRoles);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserAccessTaskAsync_NonOwnerUser_ReturnsFalse()
    {
        // Arrange
        var taskId = 1;
        var userId = "user1";
        var userRoles = new List<string> { "User" };
        var task = new TaskItem { Id = taskId, AssignedToUserId = "other-user", CreatedByUserId = "another-user" };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId)).ReturnsAsync(task);

        // Act
        var result = await _taskService.CanUserAccessTaskAsync(taskId, userId, userRoles);

        // Assert
        Assert.False(result);
    }
}