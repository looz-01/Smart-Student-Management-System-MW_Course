using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.API.Services.Auth;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Tests.Infrastructure;
using StudentManagementSystem.DTOs.Auth;

namespace StudentManagementSystem.API.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    [Fact]
    public async Task Register_Student_CreatesUserProfileAndReturnsTokens()
    {
        var auth = _provider.GetService<IAuthService>();

        var result = await auth.RegisterAsync(new RegisterDto
        {
            Email = "student@test.com",
            Password = "Test12345",
            Name = "Ahmed",
            Role = "Student",
            Age = 20,
            Gender = "Male"
        });

        Assert.NotNull(result.Data);
        Assert.Null(result.Error);
        Assert.NotEmpty(result.Data.Token);
        Assert.NotEmpty(result.Data.RefreshToken);
        Assert.Equal("Student", result.Data.Role);
        Assert.True(await _provider.Db.Students.AnyAsync(s => s.Name == "Ahmed"));
    }

    [Fact]
    public async Task Register_AdminRole_IsRejected()
    {
        var auth = _provider.GetService<IAuthService>();

        var result = await auth.RegisterAsync(new RegisterDto
        {
            Email = "hacker@test.com",
            Password = "Test12345",
            Name = "Hacker",
            Role = "Admin"
        });

        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.DoesNotContain("Admin", result.Error);
    }

    [Fact]
    public async Task Register_InvalidRole_IsRejected()
    {
        var auth = _provider.GetService<IAuthService>();

        var result = await auth.RegisterAsync(new RegisterDto
        {
            Email = "x@test.com",
            Password = "Test12345",
            Name = "X",
            Role = "Superuser"
        });

        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected()
    {
        var auth = _provider.GetService<IAuthService>();
        var dto = new RegisterDto
        {
            Email = "dup@test.com",
            Password = "Test12345",
            Name = "Dup",
            Role = "Student"
        };

        var first = await auth.RegisterAsync(dto);
        var second = await auth.RegisterAsync(dto);

        Assert.NotNull(first.Data);
        Assert.Null(second.Data);
        Assert.NotNull(second.Error);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var auth = _provider.GetService<IAuthService>();
        await _provider.CreateUserAsync("user@test.com", "Test12345", "Student");

        var result = await auth.LoginAsync(new LoginDto { Email = "user@test.com", Password = "WrongPass1" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var auth = _provider.GetService<IAuthService>();
        await _provider.CreateUserAsync("user2@test.com", "Test12345", "Teacher");

        var result = await auth.LoginAsync(new LoginDto { Email = "user2@test.com", Password = "Test12345" });

        Assert.NotNull(result);
        Assert.Equal("Teacher", result.Role);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsNull()
    {
        var auth = _provider.GetService<IAuthService>();

        var result = await auth.RefreshAsync("totally-invalid-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesToken()
    {
        var auth = _provider.GetService<IAuthService>();
        await _provider.CreateUserAsync("refresh@test.com", "Test12345", "Student");

        var login = await auth.LoginAsync(new LoginDto { Email = "refresh@test.com", Password = "Test12345" });
        Assert.NotNull(login);

        var refreshed = await auth.RefreshAsync(login.RefreshToken);

        Assert.NotNull(refreshed);
        Assert.NotEqual(login.Token, refreshed.Token);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

        // Old refresh token must now be revoked.
        var replay = await auth.RefreshAsync(login.RefreshToken);
        Assert.Null(replay);
    }

    public void Dispose() => _provider.Dispose();
}