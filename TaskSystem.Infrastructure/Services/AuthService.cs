using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TaskSystem.Core.DTOs;
using TaskSystem.Core.Entities;
using TaskSystem.Core.Interfaces;

namespace TaskSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expires = _jwtTokenService.GetTokenExpiration(accessToken),
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Validate role
        if (!UserRoles.AllRoles.Contains(request.Role))
        {
            throw new InvalidOperationException("Invalid role specified");
        }

        var user = _mapper.Map<User>(request);
        user.UserName = request.Email.ToLower();
        user.Email = request.Email.ToLower();
        user.IsActive = true;

        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, request.Role);

        _logger.LogInformation("New user registered: {Email} with role {Role}", user.Email, request.Role);

        // Send welcome email
        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.FirstName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            // Don't fail registration if email fails
        }

        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expires = _jwtTokenService.GetTokenExpiration(accessToken),
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
        var userIdClaim = principal.FindFirst("userId");
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new SecurityTokenException("Invalid token");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null || user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow || !user.IsActive)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Expires = _jwtTokenService.GetTokenExpiration(newAccessToken),
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var users = _userManager.Users.Where(u => u.RefreshToken == refreshToken);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userManager.UpdateAsync(user);

        return true;
    }

    private async Task<UserDto> MapToUserDtoAsync(User user)
    {
        var userDto = _mapper.Map<UserDto>(user);
        var roles = await _userManager.GetRolesAsync(user);
        userDto.Role = roles.FirstOrDefault() ?? string.Empty;
        return userDto;
    }
}