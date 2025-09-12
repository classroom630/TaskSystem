using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskSystem.API.Models.Domain
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        // Foreign key to ApplicationUser
        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;
        
        [ForeignKey(nameof(AssignedToUserId))]
        public virtual ApplicationUser AssignedToUser { get; set; } = null!;
        
        // Created by user (could be different from assigned user)
        public string? CreatedByUserId { get; set; }
        
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
    
    public enum TaskStatus
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