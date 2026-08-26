using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Course;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Tests.Infrastructure;
using StudentManagementSystem.DTOs.Course;

namespace StudentManagementSystem.API.Tests;

public class CourseServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    [Fact]
    public async Task Create_MissingTeacher_Throws()
    {
        var courseService = _provider.GetService<ICourseService>();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            courseService.CreateAsync(new CourseCreateDto { Name = "Math", Hours = 3, TeacherId = 999 }));

        Assert.Contains("Teacher not found", ex.Message);
    }

    [Fact]
    public async Task Create_ValidTeacher_Succeeds()
    {
        var courseService = _provider.GetService<ICourseService>();
        var teacherUser = await _provider.CreateUserAsync("t@test.com", "Test12345", "Teacher");
        var teacher = new Models.Teacher { Name = "T", UserId = teacherUser.Id, Age = 30 };
        _provider.Db.Teachers.Add(teacher);
        await _provider.Db.SaveChangesAsync();

        var result = await courseService.CreateAsync(new CourseCreateDto { Name = "Science", Hours = 4, TeacherId = teacher.Id });

        Assert.Equal("Science", result.Name);
        Assert.Equal(4, result.Hours);
        Assert.Equal(teacher.Id, result.TeacherId);
    }

    [Fact]
    public async Task Update_InvalidNewTeacher_Throws()
    {
        var courseService = _provider.GetService<ICourseService>();
        var teacherUser = await _provider.CreateUserAsync("t2@test.com", "Test12345", "Teacher");
        var teacher = new Models.Teacher { Name = "T2", UserId = teacherUser.Id, Age = 30 };
        _provider.Db.Teachers.Add(teacher);
        await _provider.Db.SaveChangesAsync();
        var course = new Models.Course { Name = "Old", Hours = 2, TeacherId = teacher.Id };
        _provider.Db.Courses.Add(course);
        await _provider.Db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            courseService.UpdateAsync(course.Id, new CourseUpdateDto { Name = "New", Hours = 3, TeacherId = 4242 }));

        Assert.Contains("Teacher not found", ex.Message);
    }

    [Fact]
    public async Task Update_Valid_Succeeds()
    {
        var courseService = _provider.GetService<ICourseService>();
        var teacherUser = await _provider.CreateUserAsync("t3@test.com", "Test12345", "Teacher");
        var teacher = new Models.Teacher { Name = "T3", UserId = teacherUser.Id, Age = 30 };
        _provider.Db.Teachers.Add(teacher);
        await _provider.Db.SaveChangesAsync();
        var course = new Models.Course { Name = "Old", Hours = 2, TeacherId = teacher.Id };
        _provider.Db.Courses.Add(course);
        await _provider.Db.SaveChangesAsync();

var result = await courseService.UpdateAsync(course.Id, new CourseUpdateDto { Name = "New", Hours = 5 });
        Assert.NotNull(result);

        Assert.Equal("New", result.Name);
        Assert.Equal(5, result.Hours);
        Assert.Equal(teacher.Id, result.TeacherId);
    }

    public void Dispose() => _provider.Dispose();
}