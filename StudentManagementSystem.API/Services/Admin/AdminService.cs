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

            if (!await _roleManager.RoleExistsAsync(dto.NewRole)) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, dto.NewRole);
            if (!result.Succeeded) return false;

            await SyncProfileAsync(user, dto.NewRole);
            return true;
        }

        private async Task SyncProfileAsync(AppUser user, string role)
        {
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
    }
}