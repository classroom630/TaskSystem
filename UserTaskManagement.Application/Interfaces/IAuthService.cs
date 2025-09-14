using UserTaskManagement.Application.DTOs.Auth;

namespace UserTaskManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
    }
}