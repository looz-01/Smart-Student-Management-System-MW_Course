using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Data;
using StudentManagementSystem.API.Extensions;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.DTOs.Admin;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminService(AppDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return new DashboardDto
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalStudents = await _db.Students.CountAsync(),
                TotalTeachers = await _db.Teachers.CountAsync(),
                TotalCourses = await _db.Courses.CountAsync(),
                TotalEnrollments = await _db.Enrollments.CountAsync(),
                TotalGrades = await _db.Grades.CountAsync(),
                TotalAttendances = await _db.Attendances.CountAsync()
            };
        }

        public async Task<PagedResult<UserListDto>> GetUsersAsync(PageRequest request, string? role = null)
        {
            request.Normalize();

            var query = from u in _db.Users.AsNoTracking()
                        join ur in _db.UserRoles.AsNoTracking() on u.Id equals ur.UserId into userRoles
                        from ur in userRoles.DefaultIfEmpty()
                        join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id into roleNames
                        from r in roleNames.DefaultIfEmpty()
                        select new UserListDto
                        {
                            Id = u.Id,
                            UserName = u.UserName ?? string.Empty,
                            Email = u.Email ?? string.Empty,
                            PhoneNumber = u.PhoneNumber,
                            Role = r != null ? r.Name : null
                        };

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(x =>
                    x.UserName.Contains(request.SearchTerm) ||
                    x.Email.Contains(request.SearchTerm));

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(x => x.Role == role);

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(x => x.UserName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(users, totalCount, request);
        }

        public async Task<bool> ChangeRoleAsync(ChangeRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) return false;

            // Normalize to canonical role names so "admin"/"ADMIN" map to "Admin".
            var newRole = NormalizeRole(dto.NewRole);
            if (newRole == null || !await _roleManager.RoleExistsAsync(newRole)) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Prevent locking the system out: the last Admin must not be demoted.
            if (currentRoles.Contains(DbSeeder.RoleAdmin) && newRole != DbSeeder.RoleAdmin)
            {
                var adminCount = await _userManager.GetUsersInRoleAsync(DbSeeder.RoleAdmin);
                if (adminCount.Count <= 1)
                    return false;
            }

            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (!result.Succeeded) return false;

            await SyncProfileAsync(user, newRole);
            return true;
        }

        private async Task SyncProfileAsync(AppUser user, string role)
        {
            // Remove the profile of the previous role so a user can never exist in both tables.
            var oldStudent = await _db.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (role != DbSeeder.RoleStudent && oldStudent != null)
            {
                _db.Grades.RemoveRange(_db.Grades.Where(g => g.StudentId == oldStudent.Id));
                _db.Attendances.RemoveRange(_db.Attendances.Where(a => a.StudentId == oldStudent.Id));
                _db.Enrollments.RemoveRange(_db.Enrollments.Where(e => e.StudentId == oldStudent.Id));
                _db.Students.Remove(oldStudent);
            }

            var oldTeacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (role != DbSeeder.RoleTeacher && oldTeacher != null)
            {
                var teacherCourseIds = _db.Courses.Where(c => c.TeacherId == oldTeacher.Id).Select(c => c.Id);
                _db.Grades.RemoveRange(_db.Grades.Where(g => teacherCourseIds.Contains(g.CourseId)));
                _db.Attendances.RemoveRange(_db.Attendances.Where(a => teacherCourseIds.Contains(a.CourseId)));
                _db.Enrollments.RemoveRange(_db.Enrollments.Where(e => teacherCourseIds.Contains(e.CourseId)));
                _db.Courses.RemoveRange(_db.Courses.Where(c => c.TeacherId == oldTeacher.Id));
                _db.Teachers.Remove(oldTeacher);
            }

            await _db.SaveChangesAsync();

            if (role == DbSeeder.RoleStudent &&
                !await _db.Students.AnyAsync(s => s.UserId == user.Id))
            {
                _db.Students.Add(new Models.Student
                {
                    Name = user.UserName ?? user.Email ?? string.Empty,
                    UserId = user.Id,
                    Age = 0,
                    Gender = string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty
                });
                await _db.SaveChangesAsync();
            }
            else if (role == DbSeeder.RoleTeacher &&
                     !await _db.Teachers.AnyAsync(t => t.UserId == user.Id))
            {
                _db.Teachers.Add(new Models.Teacher
                {
                    Name = user.UserName ?? user.Email ?? string.Empty,
                    UserId = user.Id,
                    Age = 0,
                    Specialization = string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty
                });
                await _db.SaveChangesAsync();
            }
        }

        private static string? NormalizeRole(string role)
        {
            if (role.Equals(DbSeeder.RoleAdmin, StringComparison.OrdinalIgnoreCase)) return DbSeeder.RoleAdmin;
            if (role.Equals(DbSeeder.RoleStudent, StringComparison.OrdinalIgnoreCase)) return DbSeeder.RoleStudent;
            if (role.Equals(DbSeeder.RoleTeacher, StringComparison.OrdinalIgnoreCase)) return DbSeeder.RoleTeacher;
            return null;
        }
    }
}