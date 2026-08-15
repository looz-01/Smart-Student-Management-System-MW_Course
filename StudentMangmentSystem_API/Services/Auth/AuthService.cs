using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Auth;

namespace StudentMangmentSystem_API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext db,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _configuration = configuration;
        }

        public async Task<(AuthResponseDto? Data, string? Error)> RegisterAsync(RegisterDto dto)
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null) return (null, "An account with this email already exists.");

            if (dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return (null, "Choosing the Admin role is not allowed during registration.");

            if (!dto.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
                !dto.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                return (null, "You can only choose between Student or Teacher.");

            var role = dto.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Teacher";

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return (null, "Registration Failed.");

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded) return (null, "Registration Failed.");

            if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                await _db.Students.AddAsync(new Models.Student
                {
                    Name = dto.Name,
                    UserId = user.Id,
                    Age = dto.Age ?? 0,
                    Gender = dto.Gender ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty
                });
            }
            else
            {
                await _db.Teachers.AddAsync(new Models.Teacher
                {
                    Name = dto.Name,
                    UserId = user.Id,
                    Age = dto.Age ?? 0,
                    Specialization = dto.Specialization ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty
                });
            }

            await _db.SaveChangesAsync();

            return (await GenerateTokensAsync(user, role), null);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return null;

            var check = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!check.Succeeded) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            return await GenerateTokensAsync(user, role);
        }

        public async Task<AuthResponseDto?> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            var stored = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == HashToken(refreshToken));

            if (stored == null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow) return null;

            stored.IsRevoked = true;
            await _db.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(stored.User);
            var role = roles.FirstOrDefault() ?? string.Empty;

            return await GenerateTokensAsync(stored.User, role);
        }

        private async Task<AuthResponseDto> GenerateTokensAsync(AppUser user, string role)
        {
            var accessToken = GenerateAccessToken(user, role);
            var refreshTokenValue = GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Token = HashToken(refreshTokenValue),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();

            var durationMinutes = _configuration.GetValue<int>("JWT:DuarationTime");

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshTokenValue,
                Expiration = DateTime.UtcNow.AddMinutes(durationMinutes),
                Role = role,
                UserId = user.Id
            };
        }

        private string GenerateAccessToken(AppUser user, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            if (!string.IsNullOrEmpty(role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var durationMinutes = _configuration.GetValue<int>("JWT:DuarationTime");
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(durationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshTokenValue()
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        private static string HashToken(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}