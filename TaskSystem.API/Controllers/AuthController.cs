using Microsoft.AspNetCore.Mvc;
using TaskSystem.API.Data.Services;
using TaskSystem.API.Models.DTOs;

namespace TaskSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            return Ok(result);
        }

        /// <summary>
        /// Login user and return JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(result);
        }

        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string token)
        {
            var result = await _authService.RevokeTokenAsync(token);
            if (result)
            {
                return Ok(new { message = "Token revoked successfully" });
            }
            return BadRequest(new { message = "Failed to revoke token" });
        }
    }
}