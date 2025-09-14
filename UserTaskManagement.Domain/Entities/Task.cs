using System.ComponentModel.DataAnnotations;

namespace UserTaskManagement.Domain.Entities
{
    public class Task : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsCompleted { get; set; } = false;
        public DateTime? DueDate { get; set; }
        public int UserId { get; set; }
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}