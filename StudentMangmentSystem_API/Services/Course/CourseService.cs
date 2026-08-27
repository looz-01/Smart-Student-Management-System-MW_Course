using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Course;

namespace StudentMangmentSystem_API.Services.Course
{
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public CourseService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<CourseReadDto>> GetAllAsync(PageRequest request, int? teacherId = null)
        {
            request.Normalize();

            var query = _db.Courses.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(c => c.Name.Contains(request.SearchTerm));

            if (teacherId.HasValue)
                query = query.Where(c => c.TeacherId == teacherId.Value);

            var totalCount = await query.CountAsync();

            var courses = await query
                .OrderBy(c => c.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<CourseReadDto>>(courses), totalCount, request);
        }

        public async Task<CourseReadDto?> GetByIdAsync(int id)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            return course == null ? null : _mapper.Map<CourseReadDto>(course);
        }

        public async Task<CourseReadDto> CreateAsync(CourseCreateDto dto)
        {
            var teacherExists = await _db.Teachers.AnyAsync(t => t.Id == dto.TeacherId);
            if (!teacherExists)
                throw new InvalidOperationException("Teacher not found.");

            var course = _mapper.Map<Models.Course>(dto);
            course.CreatedDate = DateTime.Now;
            await _db.Courses.AddAsync(course);
            await _db.SaveChangesAsync();
            return _mapper.Map<CourseReadDto>(course);
        }

        public async Task<CourseReadDto?> UpdateAsync(int id, CourseUpdateDto dto)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return null;

            _mapper.Map(dto, course);
            await _db.SaveChangesAsync();
            return _mapper.Map<CourseReadDto>(course);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return false;

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}