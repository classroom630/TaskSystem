using System.ComponentModel.DataAnnotations;

namespace TaskSystem.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? CompletedAt { get; set; }
    }
    
    public enum TaskItemStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
    
    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}