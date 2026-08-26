using StudentManagementSystem.DTOs.Auth;

namespace StudentManagementSystem.API.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<(AuthResponseDto? Data, string? Error)> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> RefreshAsync(string refreshToken);
    }
}