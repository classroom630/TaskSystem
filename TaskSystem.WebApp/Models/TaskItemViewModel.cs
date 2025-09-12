using System.ComponentModel.DataAnnotations;

namespace TaskSystem.WebApp.Models
{
    public class TaskItemViewModel
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
        
        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }
        
        [Display(Name = "Completed")]
        public DateTime? CompletedAt { get; set; }
    }
    
    public class CreateTaskViewModel
    {
        [Required]
        [Display(Name = "Title")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string? Description { get; set; }
        
        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }
    }
    
    public class EditTaskViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Title")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string? Description { get; set; }
        
        [Display(Name = "Status")]
        public TaskItemStatus Status { get; set; }
        
        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }
        
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }
    }
    
    public enum TaskItemStatus
    {
        [Display(Name = "Pending")]
        Pending = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        [Display(Name = "Completed")]
        Completed = 2,
        [Display(Name = "Cancelled")]
        Cancelled = 3
    }
    
    public enum TaskPriority
    {
        [Display(Name = "Low")]
        Low = 0,
        [Display(Name = "Medium")]
        Medium = 1,
        [Display(Name = "High")]
        High = 2,
        [Display(Name = "Critical")]
        Critical = 3
    }
}