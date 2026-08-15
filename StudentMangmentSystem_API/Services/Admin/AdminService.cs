using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Admin;
using StudentMangmentSystemDTO_s.Common;

namespace StudentMangmentSystem_API.Services.Admin
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

            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(u =>
                    u.UserName.Contains(request.SearchTerm) ||
                    u.Email.Contains(request.SearchTerm));

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = from u in query
                        join ur in _db.UserRoles on u.Id equals ur.UserId
                        join r in _db.Roles on ur.RoleId equals r.Id
                        where r.Name == role
                        select u;
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var items = new List<UserListDto>();
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                items.Add(new UserListDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Role = userRoles.FirstOrDefault()
                });
            }

            return PagedResultFactory.Create(items, totalCount, request);
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
            return result.Succeeded;
        }
    }
}