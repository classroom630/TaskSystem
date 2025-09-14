using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserTaskManagement.Application.DTOs.Auth;
using UserTaskManagement.Application.Interfaces;
using UserTaskManagement.Domain.Entities;

namespace UserTaskManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IBaseRepository<RefreshToken> _refreshTokenRepository;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<AuthService> logger,
            IEmailService emailService,
            IBaseRepository<RefreshToken> refreshTokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async System.Threading.Tasks.Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            try
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

                var token = await GenerateJwtTokenAsync(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                _logger.LogInformation("User {Email} logged in successfully", request.Email);

                return new TokenResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                    TokenType = "Bearer"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", request.Email);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                var user = _mapper.Map<User>(request);
                user.SecurityStamp = Guid.NewGuid().ToString();

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User registration failed: {errors}");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToString());
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign role {Role} to user {Email}", request.Role, request.Email);
                }

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(request.Email, request.FirstName, request.LastName);

                var token = await GenerateJwtTokenAsync(user);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                _logger.LogInformation("User {Email} registered successfully", request.Email);

                return new TokenResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                    TokenType = "Bearer"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", request.Email);
                throw;
            }
        }

        public async System.Threading.Tasks.Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = (await _refreshTokenRepository.GetAllAsync())
                    .FirstOrDefault(rt => rt.Token == refreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }

                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user == null || !user.IsActive)
                {
                    throw new UnauthorizedAccessException("User not found or inactive");
                }

                // Revoke old refresh token
                storedToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(storedToken);

                // Generate new tokens
                var newAccessToken = await GenerateJwtTokenAsync(user);
                var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

                return new TokenResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                    TokenType = "Bearer"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                throw;
            }
        }

        public async System.Threading.Tasks.Task RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = (await _refreshTokenRepository.GetAllAsync())
                    .FirstOrDefault(rt => rt.Token == refreshToken);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(storedToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke refresh token");
                throw;
            }
        }

        public async System.Threading.Tasks.Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(GetJwtSecretKey());
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async System.Threading.Tasks.Task<string> GenerateJwtTokenAsync(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetJwtSecretKey());

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("role", user.Role.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Add user roles from Identity
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async System.Threading.Tasks.Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureRandomString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7), // 7 days expiry
                UserId = userId
            };

            return await _refreshTokenRepository.AddAsync(refreshToken);
        }

        private string GenerateSecureRandomString()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string GetJwtSecretKey()
        {
            return _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        }

        private int GetJwtExpirationMinutes()
        {
            return int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        }
    }
}