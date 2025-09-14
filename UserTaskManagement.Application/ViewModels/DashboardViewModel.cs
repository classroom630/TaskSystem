using UserTaskManagement.Application.DTOs.Task;
using UserTaskManagement.Application.DTOs.User;
using UserTaskManagement.Domain.Enums;

namespace UserTaskManagement.Application.ViewModels
{
    public class DashboardViewModel
    {
        public Role UserRole { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<TaskDto> Tasks { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalUsers { get; set; }
    }
}