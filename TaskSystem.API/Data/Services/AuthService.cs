using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskSystem.API.Data.Repositories;
using TaskSystem.API.Models.Domain;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        
        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IRefreshTokenRepository refreshTokenRepository,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }
        
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
            
            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            
            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user);
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            
            _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);
            
            return new AuthResponseDto
            {
                Token = token.Token,
                RefreshToken = refreshToken.Token,
                Expiration = token.Expiration,
                User = userDto
            };
        }
        
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }
            
            var user = _mapper.Map<ApplicationUser>(registerDto);
            user.Id = Guid.NewGuid().ToString();
            
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }
            
            // Assign role
            await _userManager.AddToRoleAsync(user, registerDto.Role);
            
            // Send welcome email
            if (!string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
            }
            
            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user);
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = new List<string> { registerDto.Role };
            
            _logger.LogInformation("User {Email} registered successfully", registerDto.Email);
            
            return new AuthResponseDto
            {
                Token = token.Token,
                RefreshToken = refreshToken.Token,
                Expiration = token.Expiration,
                User = userDto
            };
        }
        
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }
            
            var user = refreshToken.User;
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User is not active");
            }
            
            // Revoke the used refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(refreshToken);
            
            // Generate new tokens
            var newToken = await GenerateJwtTokenAsync(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(user);
            
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            
            return new AuthResponseDto
            {
                Token = newToken.Token,
                RefreshToken = newRefreshToken.Token,
                Expiration = newToken.Expiration,
                User = userDto
            };
        }
        
        public async Task<bool> RevokeTokenAsync(string token)
        {
            await _refreshTokenRepository.RevokeTokenAsync(token);
            return true;
        }
        
        public async Task<bool> RevokeUserTokensAsync(string userId)
        {
            await _refreshTokenRepository.RevokeUserTokensAsync(userId);
            return true;
        }
        
        private async Task<(string Token, DateTime Expiration)> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FullName", user.FullName)
            };
            
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"]));
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );
            
            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }
        
        private async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationInDays"])),
                UserId = user.Id
            };
            
            return await _refreshTokenRepository.AddAsync(refreshToken);
        }
    }
}