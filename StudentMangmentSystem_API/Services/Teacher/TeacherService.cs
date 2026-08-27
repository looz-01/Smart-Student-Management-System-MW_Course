using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Teacher;

namespace StudentMangmentSystem_API.Services.Teacher
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public TeacherService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
            var teacher = _mapper.Map<Models.Teacher>(dto);
            await _db.Teachers.AddAsync(teacher);
            await _db.SaveChangesAsync();
            return _mapper.Map<TeacherReadDto>(teacher);
        }

        public async Task<TeacherReadDto?> UpdateAsync(int id, TeacherUpdateDto dto)
        {
            var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

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