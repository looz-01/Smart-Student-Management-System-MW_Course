using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentMangmentSystem_API.Services.Auth;
using StudentMangmentSystemDTO_s.Auth;

namespace StudentMangmentSystem_API.Controllers
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
        public async Task<ActionResult<ResponseApi<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequst("Please Enter Valid Data"));

            var result = await _authService.RegisterAsync(dto);
            if (result.Data == null)
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequst(result.Error ?? "Registration Failed."));

            return Ok(ResponseApi<AuthResponseDto>.CreatedAt(result.Data, "Registration Successful."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseApi<AuthResponseDto>>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequst("Please Enter Valid Data"));

            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(ResponseApi<AuthResponseDto>.BadRequst("Invalid Email or Password."));

            return Ok(ResponseApi<AuthResponseDto>.Ok(result, "Login Successful."));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseApi<AuthResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(ResponseApi<AuthResponseDto>.BadRequst("Refresh token is required."));

            var result = await _authService.RefreshAsync(dto.RefreshToken);
            if (result == null)
                return Unauthorized(ResponseApi<AuthResponseDto>.BadRequst("Invalid or expired refresh token."));

            return Ok(ResponseApi<AuthResponseDto>.Ok(result, "Token Refreshed Successfully."));
        }
    }
}