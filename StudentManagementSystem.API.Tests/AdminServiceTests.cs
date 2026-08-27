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

    [Fact]
    public async Task ChangeRole_LastAdmin_IsBlocked()
    {
        var adminService = _provider.GetService<IAdminService>();
        var admin = await _provider.CreateUserAsync("last-admin@test.com", "Test12345", "Admin");

        var changed = await adminService.ChangeRoleAsync(new ChangeRoleDto { UserId = admin.Id, NewRole = "Student" });

        Assert.False(changed);
        Assert.True(await _provider.UserManager.IsInRoleAsync(admin, "Admin"));
    }

    [Fact]
    public async Task ChangeRole_RemovesOldProfileAndItsData()
    {
        var adminService = _provider.GetService<IAdminService>();
        var teacherUser = await _provider.CreateUserAsync("old-teacher@test.com", "Test12345", "Teacher");

        var teacher = new Models.Teacher { Name = "Old Teacher", UserId = teacherUser.Id, Age = 30 };
        _provider.Db.Teachers.Add(teacher);
        await _provider.Db.SaveChangesAsync();

        var course = new Models.Course { Name = "Legacy Course", Hours = 3, TeacherId = teacher.Id };
        _provider.Db.Courses.Add(course);
        await _provider.Db.SaveChangesAsync();

        var studentUser = await _provider.CreateUserAsync("old-student@test.com", "Test12345", "Student");
        var student = new Models.Student { Name = "Old Student", UserId = studentUser.Id, Age = 18, Gender = "M" };
        _provider.Db.Students.Add(student);
        await _provider.Db.SaveChangesAsync();

        _provider.Db.Enrollments.Add(new Models.Enrollment { StudentId = student.Id, CourseId = course.Id });
        await _provider.Db.SaveChangesAsync();

        var changed = await adminService.ChangeRoleAsync(new ChangeRoleDto { UserId = teacherUser.Id, NewRole = "Student" });

        Assert.True(changed);
        Assert.True(await _provider.UserManager.IsInRoleAsync(teacherUser, "Student"));
        Assert.False(await _provider.Db.Teachers.AnyAsync(t => t.UserId == teacherUser.Id));
        Assert.False(await _provider.Db.Courses.AnyAsync(c => c.Id == course.Id));
        Assert.False(await _provider.Db.Enrollments.AnyAsync(e => e.StudentId == student.Id));
        Assert.True(await _provider.Db.Students.AnyAsync(s => s.UserId == teacherUser.Id));
    }

public void Dispose() => _provider.Dispose();
}