using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Student;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Tests.Infrastructure;
using StudentManagementSystem.DTOs.Student;

namespace StudentManagementSystem.API.Tests;

public class StudentServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    [Fact]
    public async Task Create_UserDoesNotExist_Throws()
    {
        var studentService = _provider.GetService<IStudentService>();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            studentService.CreateAsync(new StudentCreateDto
            {
                Name = "Ghost",
                Age = 20,
                Gender = "M",
                PhoneNumber = "0100",
                UserId = "missing-user-id"
            }));

        Assert.Contains("does not exist", ex.Message);
    }

    [Fact]
    public async Task Create_UserNotInStudentRole_Throws()
    {
        var studentService = _provider.GetService<IStudentService>();
        var teacher = await _provider.CreateUserAsync("teacher@test.com", "Test12345", "Teacher");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            studentService.CreateAsync(new StudentCreateDto
            {
                Name = "Wrong",
                Age = 20,
                Gender = "M",
                PhoneNumber = "0100",
                UserId = teacher.Id
            }));

        Assert.Contains("not registered as a Student", ex.Message);
    }

    [Fact]
    public async Task Create_ValidStudentUser_Succeeds()
    {
        var studentService = _provider.GetService<IStudentService>();
        var student = await _provider.CreateUserAsync("student@test.com", "Test12345", "Student");

        var result = await studentService.CreateAsync(new StudentCreateDto
        {
            Name = "Omar",
            Age = 21,
            Gender = "Male",
            PhoneNumber = "0111",
            UserId = student.Id
        });

        Assert.Equal("Omar", result.Name);
        Assert.Equal(21, result.Age);
    }

    [Fact]
    public async Task Update_IdMismatch_Throws()
    {
        var studentService = _provider.GetService<IStudentService>();
        var student = await _provider.CreateUserAsync("student2@test.com", "Test12345", "Student");
        await studentService.CreateAsync(new StudentCreateDto
        {
            Name = "Ali",
            Age = 21,
            Gender = "M",
            PhoneNumber = "0111",
            UserId = student.Id
        });

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            studentService.UpdateAsync(1, new StudentUpdateDto { Id = 999, Name = "X", Age = 20, Gender = "M", PhoneNumber = "0" }));

        Assert.Contains("ID mismatch", ex.Message);
    }

    [Fact]
    public async Task Update_DuplicateName_Throws()
    {
        var studentService = _provider.GetService<IStudentService>();
        var s1 = await _provider.CreateUserAsync("s1@test.com", "Test12345", "Student");
        var s2 = await _provider.CreateUserAsync("s2@test.com", "Test12345", "Student");
        var first = await studentService.CreateAsync(new StudentCreateDto
        {
            Name = "SameName",
            Age = 20,
            Gender = "M",
            PhoneNumber = "0111",
            UserId = s1.Id
        });
        var second = await studentService.CreateAsync(new StudentCreateDto
        {
            Name = "OtherName",
            Age = 20,
            Gender = "M",
            PhoneNumber = "0112",
            UserId = s2.Id
        });

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            studentService.UpdateAsync(second.Id, new StudentUpdateDto
            {
                Id = second.Id,
                Name = "SameName",
                Age = 20,
                Gender = "M",
                PhoneNumber = "0112"
            }));

        Assert.Contains("already exists", ex.Message);
    }

    public void Dispose() => _provider.Dispose();
}