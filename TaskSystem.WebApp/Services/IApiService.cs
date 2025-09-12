using TaskSystem.WebApp.Models;

namespace TaskSystem.WebApp.Services
{
    public interface IApiService
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null);
        Task<ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null);
        
        // Authentication methods
        Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password);
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterViewModel model);
        
        // User methods
        Task<ApiResponse<List<UserViewModel>>> GetUsersAsync(string token);
        Task<ApiResponse<UserViewModel>> GetUserAsync(string id, string token);
        Task<ApiResponse<UserViewModel>> CreateUserAsync(CreateUserViewModel model, string token);
        Task<ApiResponse<UserViewModel>> UpdateUserAsync(string id, EditUserViewModel model, string token);
        Task<ApiResponse<bool>> DeleteUserAsync(string id, string token);
        
        // Task methods
        Task<ApiResponse<List<TaskViewModel>>> GetTasksAsync(string token);
        Task<ApiResponse<List<TaskViewModel>>> GetMyTasksAsync(string token);
        Task<ApiResponse<TaskViewModel>> GetTaskAsync(int id, string token);
        Task<ApiResponse<TaskViewModel>> CreateTaskAsync(CreateTaskViewModel model, string token);
        Task<ApiResponse<TaskViewModel>> UpdateTaskAsync(int id, EditTaskViewModel model, string token);
        Task<ApiResponse<bool>> DeleteTaskAsync(int id, string token);
    }
}