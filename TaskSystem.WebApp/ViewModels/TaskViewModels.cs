using System.ComponentModel.DataAnnotations;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;

namespace TaskSystem.WebApp.ViewModels;

public class TaskListViewModel
{
    public IEnumerable<TaskDto> Tasks { get; set; } = new List<TaskDto>();
    public string CurrentUserRole { get; set; } = string.Empty;
}

public class CreateTaskViewModel
{
    [Required]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Priority")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Assigned To")]
    public int? AssignedToId { get; set; }

    public IEnumerable<UserDto> AvailableUsers { get; set; } = new List<UserDto>();
}

public class EditTaskViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Status")]
    public TaskItemStatus Status { get; set; }

    [Display(Name = "Priority")]
    public TaskPriority Priority { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Assigned To")]
    public int? AssignedToId { get; set; }

    public IEnumerable<UserDto> AvailableUsers { get; set; } = new List<UserDto>();
}

public class TaskDetailsViewModel
{
    public TaskDto Task { get; set; } = null!;
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}