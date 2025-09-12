using System.Security.Claims;
using TaskSystem.Core.Entities;

namespace TaskSystem.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    DateTime GetTokenExpiration(string token);
}