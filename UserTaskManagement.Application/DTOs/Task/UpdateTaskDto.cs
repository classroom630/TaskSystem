using System.ComponentModel.DataAnnotations;

namespace UserTaskManagement.Application.DTOs.Task
{
    public class UpdateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
    }
}