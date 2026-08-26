using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudentManagementSystem.API.Services.Auth;
using StudentManagementSystem.DTOs.Auth;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ResponseApi<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequest("Please Enter Valid Data"));

            var result = await _authService.RegisterAsync(dto);
            if (result.Data == null)
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequest(result.Error ?? "Registration Failed."));

            return Ok(ResponseApi<AuthResponseDto>.CreatedAt(result.Data, "Registration Successful."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ResponseApi<AuthResponseDto>>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequest("Please Enter Valid Data"));

            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(ResponseApi<AuthResponseDto>.BadRequest("Invalid Email or Password."));

            return Ok(ResponseApi<AuthResponseDto>.Ok(result, "Login Successful."));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<ResponseApi<AuthResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto? dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequest("Refresh token is required."));

            var result = await _authService.RefreshAsync(dto.RefreshToken);
            if (result == null)
                return Unauthorized(ResponseApi<AuthResponseDto>.BadRequest("Invalid or expired refresh token."));

            return Ok(ResponseApi<AuthResponseDto>.Ok(result, "Token Refreshed Successfully."));
        }
    }
}