using System.Security.Claims;
using TaskSystem.Core.Entities;

namespace TaskSystem.Core.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateAccessTokenAsync(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    DateTime GetTokenExpiration(string token);
}