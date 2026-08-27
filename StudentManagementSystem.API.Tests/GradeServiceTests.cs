using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.API.Services.Grade;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Tests.Infrastructure;
using StudentManagementSystem.DTOs.Grade;

namespace StudentManagementSystem.API.Tests;

public class GradeServiceTests : IDisposable
{
    private readonly TestServiceProvider _provider = new();

    [Fact]
    public async Task Create_AdminWithoutEnrollment_Throws()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 90 }));

        Assert.Contains("not enrolled", ex.Message);
    }

    [Fact]
    public async Task Create_ScoreOutOfRange_Throws()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 150 }));

        Assert.Contains("between 0 and 100", ex.Message);
    }

    [Fact]
    public async Task Create_TeacherNotOwningCourse_Throws()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
await EnrollAsync(studentId, courseId);
        var otherTeacher = await SeedTeacherAsync("other@teacher.com");
        await SeedTeacherProfileAsync(otherTeacher);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            gradeService.CreateAsync(
                new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 85 },
                otherTeacher.Id));

        Assert.Contains("courses you teach", ex.Message);
    }

    [Fact]
    public async Task Create_AdminWithEnrollment_Succeeds()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);

        var grade = await gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 95 });

        Assert.Equal(95, grade.Score);
        Assert.True(await _provider.Db.Grades.AnyAsync(g => g.Id == grade.Id));
    }

    [Fact]
    public async Task Create_TeacherOwningCourse_Succeeds()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId, teacherUserId) = await SeedStudentAndCourseWithTeacherAsync();
        await EnrollAsync(studentId, courseId);

        var grade = await gradeService.CreateAsync(
            new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 77 },
            teacherUserId);

        Assert.Equal(77, grade.Score);
    }

[Fact]
    public async Task Update_TeacherNotOwningCourse_Throws()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);
var grade = await gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 60 });
        var otherTeacher = await SeedTeacherAsync("other2@teacher.com");
        await SeedTeacherProfileAsync(otherTeacher);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            gradeService.UpdateAsync(grade.Id, new GradeUpdateDto { Score = 99 }, otherTeacher.Id));

        Assert.Contains("courses you teach", ex.Message);
    }

    [Fact]
    public async Task Create_DuplicateGradeForSameStudentAndCourse_Throws()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);

        await gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 80 });

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 90 }));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task GetAll_TeacherOnlySeesOwnCoursesGrades()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId, teacherUserId) = await SeedStudentAndCourseWithTeacherAsync();
        await EnrollAsync(studentId, courseId);

        var otherTeacher = await SeedTeacherAsync("other3@teacher.com");
        await SeedTeacherProfileAsync(otherTeacher);
        var otherCourse = new Models.Course { Name = "History", Hours = 2, TeacherId = (await _provider.Db.Teachers.FirstAsync(t => t.UserId == otherTeacher.Id)).Id };
        _provider.Db.Courses.Add(otherCourse);
        await _provider.Db.SaveChangesAsync();
        _provider.Db.Enrollments.Add(new Models.Enrollment { StudentId = studentId, CourseId = otherCourse.Id });
        await _provider.Db.SaveChangesAsync();

        await gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 70 });
        await gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = otherCourse.Id, Score = 65 });

        var teacherView = await gradeService.GetAllAsync(new DTOs.Common.PageRequest { PageSize = 50 }, teacherUserId: teacherUserId);

        Assert.Equal(1, teacherView.TotalCount);
        Assert.Equal(70, teacherView.Items[0].Score);
    }

    [Fact]
    public async Task GetById_TeacherAccessingOtherCourseGrade_Throws()
    {
        var gradeService = _provider.GetService<IGradeService>();
        var (studentId, courseId) = await SeedStudentAndCourseAsync();
        await EnrollAsync(studentId, courseId);
        var grade = await gradeService.CreateAsync(new GradeCreateDto { StudentId = studentId, CourseId = courseId, Score = 88 });

        var otherTeacher = await SeedTeacherAsync("other4@teacher.com");
        await SeedTeacherProfileAsync(otherTeacher);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            gradeService.GetByIdAsync(grade.Id, otherTeacher.Id));

        Assert.Contains("courses you teach", ex.Message);
    }

    private async Task<(int StudentId, int CourseId)> SeedStudentAndCourseAsync()
    {
        var (studentId, courseId, _) = await SeedStudentAndCourseWithTeacherAsync();
        return (studentId, courseId);
    }

    private async Task<(int StudentId, int CourseId, string TeacherUserId)> SeedStudentAndCourseWithTeacherAsync()
    {
        var teacher = await SeedTeacherAsync("owner@teacher.com");

        var teacherProfile = new Models.Teacher { Name = "Owner", UserId = teacher.Id, Age = 30 };
        _provider.Db.Teachers.Add(teacherProfile);
        await _provider.Db.SaveChangesAsync();

        var course = new Models.Course { Name = "Math", Hours = 3, TeacherId = teacherProfile.Id };
        _provider.Db.Courses.Add(course);
        await _provider.Db.SaveChangesAsync();

        var studentUser = await _provider.CreateUserAsync("student@test.com", "Test12345", "Student");
        var student = new Models.Student { Name = "S1", UserId = studentUser.Id, Age = 18, Gender = "M" };
        _provider.Db.Students.Add(student);
        await _provider.Db.SaveChangesAsync();

        return (student.Id, course.Id, teacher.Id);
    }

private async Task<AppUser> SeedTeacherAsync(string email)
        => await _provider.CreateUserAsync(email, "Test12345", "Teacher");

    private async Task SeedTeacherProfileAsync(AppUser user)
    {
        _provider.Db.Teachers.Add(new Models.Teacher { Name = user.UserName!, UserId = user.Id, Age = 30 });
        await _provider.Db.SaveChangesAsync();
    }

    private async Task EnrollAsync(int studentId, int courseId)
    {
        _provider.Db.Enrollments.Add(new Models.Enrollment { StudentId = studentId, CourseId = courseId });
        await _provider.Db.SaveChangesAsync();
    }

    public void Dispose() => _provider.Dispose();
}