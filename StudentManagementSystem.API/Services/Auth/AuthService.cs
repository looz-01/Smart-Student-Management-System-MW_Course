using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentManagementSystem.API.Data;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.API.Options;
using StudentManagementSystem.DTOs.Auth;

namespace StudentManagementSystem.API.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _db;
        private readonly JwtOptions _jwtOptions;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext db,
            IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<(AuthResponseDto? Data, string? Error)> RegisterAsync(RegisterDto dto)
        {
            var role = NormalizeRole(dto.Role);
            if (role == null)
                return (null, "You can only choose between Student or Teacher.");

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return (null, createResult.Errors.FirstOrDefault()?.Description ?? "Registration Failed.");

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (null, roleResult.Errors.FirstOrDefault()?.Description ?? "Registration Failed.");
            }

            if (role == DbSeeder.RoleStudent)
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

            try
            {
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return (null, "Registration Failed. Please try again.");
            }

            return (await GenerateTokensAsync(user, role), null);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return null;

            var check = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
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

            if (stored == null || stored.User == null) return null;

            // Reuse detection: presenting an already-revoked token means it may have been
            // stolen, so revoke the whole family and force re-authentication.
            if (stored.IsRevoked)
            {
                await RevokeAllUserTokensAsync(stored.UserId);
                return null;
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
            {
                _db.RefreshTokens.Remove(stored);
                await _db.SaveChangesAsync();
                return null;
            }

            stored.IsRevoked = true;
            await _db.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(stored.User);
            var role = roles.FirstOrDefault() ?? string.Empty;

            return await GenerateTokensAsync(stored.User, role);
        }

        private async Task<AuthResponseDto> GenerateTokensAsync(AppUser user, string role)
        {
            await PurgeExpiredTokensAsync(user.Id);

            var accessToken = GenerateAccessToken(user, role);
            var refreshTokenValue = GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Token = HashToken(refreshTokenValue),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDurationInDays),
                IsRevoked = false
            };

            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();

            var now = DateTime.UtcNow;

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshTokenValue,
                Expiration = now.AddMinutes(_jwtOptions.DurationInMinutes),
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
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_jwtOptions.DurationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshTokenValue()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private static string HashToken(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private static string? NormalizeRole(string role)
        {
            if (role.Equals(DbSeeder.RoleAdmin, StringComparison.OrdinalIgnoreCase)) return null;
            if (role.Equals(DbSeeder.RoleStudent, StringComparison.OrdinalIgnoreCase)) return DbSeeder.RoleStudent;
            if (role.Equals(DbSeeder.RoleTeacher, StringComparison.OrdinalIgnoreCase)) return DbSeeder.RoleTeacher;
            return null;
        }

        private async Task PurgeExpiredTokensAsync(string userId)
        {
            var cutoff = DateTime.UtcNow.AddDays(-_jwtOptions.RefreshTokenDurationInDays * 2);
            var stale = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId &&
                             (rt.ExpiresAt <= DateTime.UtcNow || (rt.IsRevoked && rt.CreatedAt <= cutoff)))
                .ToListAsync();

            if (stale.Count == 0) return;

            _db.RefreshTokens.RemoveRange(stale);
            await _db.SaveChangesAsync();
        }

        private async Task RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _db.RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked).ToListAsync();
            foreach (var token in tokens)
                token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }
}