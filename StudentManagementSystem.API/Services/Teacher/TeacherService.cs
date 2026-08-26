using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Data;
using StudentManagementSystem.API.Extensions;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Teacher;

namespace StudentManagementSystem.API.Services.Teacher
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public TeacherService(AppDbContext db, IMapper mapper, UserManager<AppUser> userManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<PagedResult<TeacherReadDto>> GetAllAsync(PageRequest request, string? specialization = null)
        {
            request.Normalize();

            var query = _db.Teachers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(t =>
                    t.Name.Contains(request.SearchTerm) ||
                    t.PhoneNumber.Contains(request.SearchTerm));

            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(t => t.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase));

            var totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<TeacherReadDto>>(teachers), totalCount, request);
        }

        public async Task<TeacherReadDto?> GetByIdAsync(int id)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            return teacher == null ? null : _mapper.Map<TeacherReadDto>(teacher);
        }

        public async Task<TeacherReadDto?> GetByUserIdAsync(string userId)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            return teacher == null ? null : _mapper.Map<TeacherReadDto>(teacher);
        }

        public async Task<TeacherReadDto> CreateAsync(TeacherCreateDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new AppException("The linked user does not exist.");

            var isTeacherRole = await _userManager.IsInRoleAsync(user, DbSeeder.RoleTeacher);
            if (!isTeacherRole)
                throw new AppException("The linked user is not registered as a Teacher.");

            var teacher = _mapper.Map<Models.Teacher>(dto);
            await _db.Teachers.AddAsync(teacher);
            await _db.SaveChangesAsync();
            return _mapper.Map<TeacherReadDto>(teacher);
        }

        public async Task<TeacherReadDto?> UpdateAsync(int id, TeacherUpdateDto dto)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

            var duplicate = await _db.Teachers
                .FirstOrDefaultAsync(x => x.Name.ToLower() == dto.Name.ToLower() && x.Id != id);
            if (duplicate != null)
                throw new AppException("A teacher with this name already exists.");

            _mapper.Map(dto, teacher);
            await _db.SaveChangesAsync();
            return _mapper.Map<TeacherReadDto>(teacher);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return false;

            _db.Teachers.Remove(teacher);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}