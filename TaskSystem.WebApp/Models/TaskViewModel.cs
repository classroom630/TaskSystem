using System.ComponentModel.DataAnnotations;

namespace TaskSystem.WebApp.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public TaskStatus Status { get; set; }
        
        public TaskPriority Priority { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Updated Date")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
        
        [Display(Name = "Assigned To")]
        public string AssignedToUserId { get; set; } = string.Empty;
        
        [Display(Name = "Assigned To")]
        public string AssignedToUserName { get; set; } = string.Empty;
        
        [Display(Name = "Created By")]
        public string? CreatedByUserId { get; set; }
        
        [Display(Name = "Created By")]
        public string? CreatedByUserName { get; set; }
        
        [Display(Name = "Status")]
        public string StatusDisplay => Status.ToString();
        
        [Display(Name = "Priority")]
        public string PriorityDisplay => Priority.ToString();
        
        [Display(Name = "Priority Badge")]
        public string PriorityBadgeClass => Priority switch
        {
            TaskPriority.Critical => "badge-danger",
            TaskPriority.High => "badge-warning",
            TaskPriority.Medium => "badge-info",
            TaskPriority.Low => "badge-secondary",
            _ => "badge-secondary"
        };
        
        [Display(Name = "Status Badge")]
        public string StatusBadgeClass => Status switch
        {
            TaskStatus.Pending => "badge-secondary",
            TaskStatus.InProgress => "badge-primary",
            TaskStatus.Completed => "badge-success",
            TaskStatus.Cancelled => "badge-danger",
            _ => "badge-secondary"
        };
    }
    
    public class CreateTaskViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }
        
        [Required]
        [Display(Name = "Assign To")]
        public string AssignedToUserId { get; set; } = string.Empty;
    }
    
    public class EditTaskViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public TaskStatus Status { get; set; }
        
        [Required]
        public TaskPriority Priority { get; set; }
        
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }
        
        [Display(Name = "Assign To")]
        public string? AssignedToUserId { get; set; }
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