using System.Text;
using System.Text.Json;
using TaskSystem.WebApp.Models;

namespace TaskSystem.WebApp.Services
{
    public class TaskApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TaskApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TaskApiService(HttpClient httpClient, ILogger<TaskApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<IEnumerable<TaskItemViewModel>> GetAllTasksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/tasks");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var tasks = JsonSerializer.Deserialize<TaskItemViewModel[]>(json, _jsonOptions);
                return tasks ?? Array.Empty<TaskItemViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks from API");
                return Array.Empty<TaskItemViewModel>();
            }
        }

        public async Task<TaskItemViewModel?> GetTaskByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/tasks/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TaskItemViewModel>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task {TaskId} from API", id);
                return null;
            }
        }

        public async Task<TaskItemViewModel?> CreateTaskAsync(CreateTaskViewModel createTask)
        {
            try
            {
                var json = JsonSerializer.Serialize(createTask, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/tasks", content);
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TaskItemViewModel>(responseJson, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return null;
            }
        }

        public async Task<bool> UpdateTaskAsync(int id, EditTaskViewModel editTask)
        {
            try
            {
                var updateDto = new
                {
                    title = editTask.Title,
                    description = editTask.Description,
                    status = editTask.Status,
                    priority = editTask.Priority,
                    dueDate = editTask.DueDate
                };
                
                var json = JsonSerializer.Serialize(updateDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"api/tasks/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<TaskItemViewModel>> GetTasksByStatusAsync(TaskItemStatus status)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/tasks/status/{(int)status}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var tasks = JsonSerializer.Deserialize<TaskItemViewModel[]>(json, _jsonOptions);
                return tasks ?? Array.Empty<TaskItemViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks by status {Status}", status);
                return Array.Empty<TaskItemViewModel>();
            }
        }
    }
}