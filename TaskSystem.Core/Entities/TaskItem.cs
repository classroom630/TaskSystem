using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.Core.Entities;

public class TaskItem : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    // Foreign Keys
    [Required]
    [ForeignKey(nameof(CreatedBy))]
    public int CreatedById { get; set; }
    
    [ForeignKey(nameof(AssignedTo))]
    public int? AssignedToId { get; set; }

    // Navigation Properties
    public virtual User CreatedBy { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }
}

public enum TaskItemStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}