using System.ComponentModel.DataAnnotations;
using DomainTaskStatus = TaskSystem.API.Models.Domain.TaskStatus;
using TaskSystem.API.Models.Domain;

namespace TaskSystem.API.Models.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DomainTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string AssignedToUserId { get; set; } = string.Empty;
        public string AssignedToUserName { get; set; } = string.Empty;
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
    }
    
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public DateTime? DueDate { get; set; }
        
        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;
    }
    
    public class UpdateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public DomainTaskStatus Status { get; set; }
        
        [Required]
        public TaskPriority Priority { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public string? AssignedToUserId { get; set; }
    }
    
    public class TaskFilterDto
    {
        public DomainTaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? AssignedToUserId { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}