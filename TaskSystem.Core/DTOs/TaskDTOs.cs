using System.ComponentModel.DataAnnotations;
using TaskSystem.Core.Entities;

namespace TaskSystem.Core.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
}

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public DateTime? DueDate { get; set; }
    
    public int? AssignedToId { get; set; }
}

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public int? AssignedToId { get; set; }
}