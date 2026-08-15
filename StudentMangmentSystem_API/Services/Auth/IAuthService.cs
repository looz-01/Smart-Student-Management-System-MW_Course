using StudentMangmentSystemDTO_s.Auth;

namespace StudentMangmentSystem_API.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<(AuthResponseDto? Data, string? Error)> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> RefreshAsync(string refreshToken);
    }
}