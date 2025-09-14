using System.ComponentModel.DataAnnotations;

namespace UserTaskManagement.Application.DTOs.Task
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime? DueDate { get; set; }
        
        // For Admin/Manager to assign tasks to specific users
        public int? UserId { get; set; }
    }
}