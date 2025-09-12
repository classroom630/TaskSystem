using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TaskSystem.WebApp.Models;

namespace TaskSystem.WebApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.GetAsync(endpoint);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
                return CreateErrorResponse<T>("An error occurred while making the request");
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
                return CreateErrorResponse<T>("An error occurred while making the request");
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
                return CreateErrorResponse<T>("An error occurred while making the request");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var response = await _httpClient.DeleteAsync(endpoint);
                return new ApiResponse<bool>
                {
                    Success = response.IsSuccessStatusCode,
                    Data = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? null : "Delete operation failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
                return CreateErrorResponse<bool>("An error occurred while making the request");
            }
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password)
        {
            var loginData = new { Email = email, Password = password };
            return await PostAsync<AuthResponse>("api/auth/login", loginData);
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterViewModel model)
        {
            var registerData = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role
            };
            return await PostAsync<AuthResponse>("api/auth/register", registerData);
        }

        public async Task<ApiResponse<List<UserViewModel>>> GetUsersAsync(string token)
        {
            return await GetAsync<List<UserViewModel>>("api/users", token);
        }

        public async Task<ApiResponse<UserViewModel>> GetUserAsync(string id, string token)
        {
            return await GetAsync<UserViewModel>($"api/users/{id}", token);
        }

        public async Task<ApiResponse<UserViewModel>> CreateUserAsync(CreateUserViewModel model, string token)
        {
            var userData = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
                Role = model.Role
            };
            return await PostAsync<UserViewModel>("api/users", userData, token);
        }

        public async Task<ApiResponse<UserViewModel>> UpdateUserAsync(string id, EditUserViewModel model, string token)
        {
            var userData = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive
            };
            return await PutAsync<UserViewModel>($"api/users/{id}", userData, token);
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string id, string token)
        {
            return await DeleteAsync($"api/users/{id}", token);
        }

        public async Task<ApiResponse<List<TaskViewModel>>> GetTasksAsync(string token)
        {
            return await GetAsync<List<TaskViewModel>>("api/tasks", token);
        }

        public async Task<ApiResponse<List<TaskViewModel>>> GetMyTasksAsync(string token)
        {
            return await GetAsync<List<TaskViewModel>>("api/tasks/my-tasks", token);
        }

        public async Task<ApiResponse<TaskViewModel>> GetTaskAsync(int id, string token)
        {
            return await GetAsync<TaskViewModel>($"api/tasks/{id}", token);
        }

        public async Task<ApiResponse<TaskViewModel>> CreateTaskAsync(CreateTaskViewModel model, string token)
        {
            var taskData = new
            {
                Title = model.Title,
                Description = model.Description,
                Priority = (int)model.Priority,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId
            };
            return await PostAsync<TaskViewModel>("api/tasks", taskData, token);
        }

        public async Task<ApiResponse<TaskViewModel>> UpdateTaskAsync(int id, EditTaskViewModel model, string token)
        {
            var taskData = new
            {
                Title = model.Title,
                Description = model.Description,
                Status = (int)model.Status,
                Priority = (int)model.Priority,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId
            };
            return await PutAsync<TaskViewModel>($"api/tasks/{id}", taskData, token);
        }

        public async Task<ApiResponse<bool>> DeleteTaskAsync(int id, string token)
        {
            return await DeleteAsync($"api/tasks/{id}", token);
        }

        private void SetAuthorizationHeader(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = data,
                        StatusCode = (int)response.StatusCode
                    };
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing response");
                    return CreateErrorResponse<T>("Error processing server response");
                }
            }
            
            string errorMessage = "Request failed";
            try
            {
                // Try to parse error response
                var errorDoc = JsonDocument.Parse(content);
                if (errorDoc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    errorMessage = messageElement.GetString() ?? errorMessage;
                }
            }
            catch
            {
                // Use default error message if parsing fails
            }

            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                ErrorMessage = errorMessage
            };
        }

        private ApiResponse<T> CreateErrorResponse<T>(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = message,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}