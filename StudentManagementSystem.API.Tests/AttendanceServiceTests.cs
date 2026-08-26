using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Attendance;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Tests.Infrastructure;
using StudentManagementSystem.DTOs.Attendance;

namespace StudentManagementSystem.API.Tests;

public class AttendanceServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    [Fact]
    public async Task Create_NotEnrolledStudent_Throws()
    {
        var attendanceService = _provider.GetService<IAttendanceService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            attendanceService.CreateAsync(new AttendanceCreateDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Date = DateTime.UtcNow,
                IsPresent = true
            }));

        Assert.Contains("not enrolled", ex.Message);
    }

    [Fact]
    public async Task Create_DuplicateAttendanceSameDay_Throws()
    {
        var attendanceService = _provider.GetService<IAttendanceService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);
        var dto = new AttendanceCreateDto
        {
            StudentId = studentId,
            CourseId = courseId,
            Date = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            IsPresent = true
        };

        await attendanceService.CreateAsync(dto);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            attendanceService.CreateAsync(new AttendanceCreateDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Date = new DateTime(2026, 1, 1, 14, 30, 0, DateTimeKind.Utc),
                IsPresent = false
            }));

        Assert.Contains("already marked", ex.Message);
    }

    [Fact]
    public async Task Create_DifferentDay_AllowsSecondRecord()
    {
        var attendanceService = _provider.GetService<IAttendanceService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);

        var first = await attendanceService.CreateAsync(new AttendanceCreateDto
        {
            StudentId = studentId,
            CourseId = courseId,
            Date = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            IsPresent = true
        });

        var second = await attendanceService.CreateAsync(new AttendanceCreateDto
        {
            StudentId = studentId,
            CourseId = courseId,
            Date = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc),
            IsPresent = false
        });

        Assert.True(first.IsPresent);
        Assert.False(second.IsPresent);
    }

    [Fact]
    public async Task Create_TeacherNotOwningCourse_Throws()
    {
        var attendanceService = _provider.GetService<IAttendanceService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
await EnrollAsync(studentId, courseId);
        var otherTeacher = await _provider.CreateUserAsync("other@teacher.com", "Test12345", "Teacher");
        _provider.Db.Teachers.Add(new Models.Teacher { Name = "Other", UserId = otherTeacher.Id, Age = 30 });
        await _provider.Db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            attendanceService.CreateAsync(new AttendanceCreateDto
            {
                StudentId = studentId,
                CourseId = courseId,
                Date = DateTime.UtcNow,
                IsPresent = true
            }, otherTeacher.Id));

        Assert.Contains("courses you teach", ex.Message);
    }

    private async Task<(int StudentId, int CourseId)> SeedStudentAndCourseAsync()
    {
        var teacherUser = await _provider.CreateUserAsync("owner@teacher.com", "Test12345", "Teacher");
        var teacherProfile = new Models.Teacher { Name = "Owner", UserId = teacherUser.Id, Age = 30 };
        _provider.Db.Teachers.Add(teacherProfile);
        await _provider.Db.SaveChangesAsync();

        var course = new Models.Course { Name = "Physics", Hours = 2, TeacherId = teacherProfile.Id };
        _provider.Db.Courses.Add(course);
        await _provider.Db.SaveChangesAsync();

        var studentUser = await _provider.CreateUserAsync("att-student@test.com", "Test12345", "Student");
        var student = new Models.Student { Name = "S1", UserId = studentUser.Id, Age = 18, Gender = "M" };
        _provider.Db.Students.Add(student);
        await _provider.Db.SaveChangesAsync();

        return (student.Id, course.Id);
    }

    private async Task EnrollAsync(int studentId, int courseId)
    {
        _provider.Db.Enrollments.Add(new Models.Enrollment { StudentId = studentId, CourseId = courseId });
        await _provider.Db.SaveChangesAsync();
    }

    public void Dispose() => _provider.Dispose();
}