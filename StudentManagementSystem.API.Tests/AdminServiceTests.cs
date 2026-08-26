using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.API.Services.Admin;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Tests.Infrastructure;
using StudentManagementSystem.DTOs.Admin;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Tests;

public class AdminServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    [Fact]
    public async Task GetUsers_ReturnsRolesWithSingleQuery()
    {
        var adminService = _provider.GetService<IAdminService>();
        await _provider.CreateUserAsync("a@test.com", "Test12345", "Student");
        await _provider.CreateUserAsync("b@test.com", "Test12345", "Teacher");

        var result = await adminService.GetUsersAsync(new PageRequest { PageSize = 50 });

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, u => u.Role == "Student");
        Assert.Contains(result.Items, u => u.Role == "Teacher");
    }

    [Fact]
    public async Task GetUsers_FiltersByRole()
    {
        var adminService = _provider.GetService<IAdminService>();
        await _provider.CreateUserAsync("c@test.com", "Test12345", "Student");
        await _provider.CreateUserAsync("d@test.com", "Test12345", "Teacher");

        var result = await adminService.GetUsersAsync(new PageRequest { PageSize = 50 }, role: "Teacher");

        Assert.Equal(1, result.TotalCount);
        Assert.All(result.Items, u => Assert.Equal("Teacher", u.Role));
    }

    [Fact]
    public async Task GetUsers_FiltersBySearchTerm()
    {
        var adminService = _provider.GetService<IAdminService>();
        await _provider.CreateUserAsync("findme@test.com", "Test12345", "Student");
        await _provider.CreateUserAsync("other@test.com", "Test12345", "Teacher");

        var result = await adminService.GetUsersAsync(new PageRequest { PageSize = 50, SearchTerm = "findme" });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("findme@test.com", result.Items[0].Email);
    }

    [Fact]
    public async Task ChangeRole_ToStudent_CreatesMissingProfile()
    {
        var adminService = _provider.GetService<IAdminService>();
        var user = await _provider.CreateUserAsync("e@test.com", "Test12345", "Teacher");

        var changed = await adminService.ChangeRoleAsync(new ChangeRoleDto { UserId = user.Id, NewRole = "Student" });

        Assert.True(changed);
        Assert.True(await _provider.Db.Students.AnyAsync(s => s.UserId == user.Id));
        Assert.False(await _provider.Db.Teachers.AnyAsync(t => t.UserId == user.Id));
    }

    [Fact]
    public async Task ChangeRole_UnknownRole_ReturnsFalse()
    {
        var adminService = _provider.GetService<IAdminService>();
        var user = await _provider.CreateUserAsync("f@test.com", "Test12345", "Student");

        var changed = await adminService.ChangeRoleAsync(new ChangeRoleDto { UserId = user.Id, NewRole = "Superadmin" });

        Assert.False(changed);
    }

    [Fact]
    public async Task ChangeRole_UnknownUser_ReturnsFalse()
    {
        var adminService = _provider.GetService<IAdminService>();

        var changed = await adminService.ChangeRoleAsync(new ChangeRoleDto { UserId = "nope", NewRole = "Student" });

        Assert.False(changed);
    }

    public void Dispose() => _provider.Dispose();
}