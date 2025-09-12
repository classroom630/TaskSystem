using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> RevokeUserTokensAsync(string userId);
    }
}