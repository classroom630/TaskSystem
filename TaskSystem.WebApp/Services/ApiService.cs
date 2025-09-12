using Newtonsoft.Json;
using System.Text;
using TaskSystem.Core.DTOs;

namespace TaskSystem.WebApp.Services;

public interface IApiService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<IEnumerable<UserDto>?> GetUsersAsync(string token);
    Task<UserDto?> GetUserAsync(int id, string token);
    Task<UserDto?> CreateUserAsync(CreateUserRequest request, string token);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request, string token);
    Task<bool> DeleteUserAsync(int id, string token);
    Task<IEnumerable<TaskDto>?> GetTasksAsync(string token);
    Task<TaskDto?> GetTaskAsync(int id, string token);
    Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request, string token);
    Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskRequest request, string token);
    Task<bool> DeleteTaskAsync(int id, string token);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await PostAsync<AuthResponse>("api/auth/login", request);
        return response;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await PostAsync<AuthResponse>("api/auth/register", request);
        return response;
    }

    public async Task<IEnumerable<UserDto>?> GetUsersAsync(string token)
    {
        var response = await GetAsync<IEnumerable<UserDto>>("api/users", token);
        return response;
    }

    public async Task<UserDto?> GetUserAsync(int id, string token)
    {
        var response = await GetAsync<UserDto>($"api/users/{id}", token);
        return response;
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request, string token)
    {
        var response = await PostAsync<UserDto>("api/users", request, token);
        return response;
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request, string token)
    {
        var response = await PutAsync<UserDto>($"api/users/{id}", request, token);
        return response;
    }

    public async Task<bool> DeleteUserAsync(int id, string token)
    {
        var response = await DeleteAsync($"api/users/{id}", token);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<TaskDto>?> GetTasksAsync(string token)
    {
        var response = await GetAsync<IEnumerable<TaskDto>>("api/tasks", token);
        return response;
    }

    public async Task<TaskDto?> GetTaskAsync(int id, string token)
    {
        var response = await GetAsync<TaskDto>($"api/tasks/{id}", token);
        return response;
    }

    public async Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request, string token)
    {
        var response = await PostAsync<TaskDto>("api/tasks", request, token);
        return response;
    }

    public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskRequest request, string token)
    {
        var response = await PutAsync<TaskDto>($"api/tasks/{id}", request, token);
        return response;
    }

    public async Task<bool> DeleteTaskAsync(int id, string token)
    {
        var response = await DeleteAsync($"api/tasks/{id}", token);
        return response.IsSuccessStatusCode;
    }

    private async Task<T?> GetAsync<T>(string endpoint, string? token = null) where T : class
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
        }
        return null;
    }

    private async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null) where T : class
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
        }
        return null;
    }

    private async Task<T?> PutAsync<T>(string endpoint, object data, string token) where T : class
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = content };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
        }
        return null;
    }

    private async Task<HttpResponseMessage> DeleteAsync(string endpoint, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}