using UserTaskManagement.Application.DTOs.Task;

namespace UserTaskManagement.Application.ViewModels
{
    public class TaskListViewModel
    {
        public List<TaskDto> Tasks { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public string Filter { get; set; } = string.Empty;
        public bool? CompletedFilter { get; set; }
    }
}