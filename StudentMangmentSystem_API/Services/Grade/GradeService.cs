using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Grade;

namespace StudentMangmentSystem_API.Services.Grade
{
    public class GradeService : IGradeService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public GradeService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<GradeReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null)
        {
            request.Normalize();

            var query = _db.Grades.AsNoTracking();

            if (studentId.HasValue)
                query = query.Where(g => g.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(g => g.CourseId == courseId.Value);

            var totalCount = await query.CountAsync();

            var grades = await query
                .OrderByDescending(g => g.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<GradeReadDto>>(grades), totalCount, request);
        }

        public async Task<GradeReadDto?> GetByIdAsync(int id)
        {
            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id);
            return grade == null ? null : _mapper.Map<GradeReadDto>(grade);
        }

        public async Task<PagedResult<GradeReadDto>> GetByStudentIdAsync(int studentId, PageRequest request)
        {
            request.Normalize();

            var query = _db.Grades.AsNoTracking().Where(g => g.StudentId == studentId);

            var totalCount = await query.CountAsync();

            var grades = await query
                .OrderByDescending(g => g.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<GradeReadDto>>(grades), totalCount, request);
        }

        public async Task<int?> GetStudentIdByUserIdAsync(string userId)
        {
            var student = await _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
            return student?.Id;
        }

        public async Task<GradeReadDto> CreateAsync(GradeCreateDto dto, string? teacherUserId = null)
        {
            var studentExists = await _db.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                throw new InvalidOperationException("Student not found.");

            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
                if (teacher == null)
                    throw new InvalidOperationException("Teacher profile not found.");

                var ownsCourse = await _db.Courses.AnyAsync(c => c.Id == dto.CourseId && c.TeacherId == teacher.Id);
                if (!ownsCourse)
                    throw new InvalidOperationException("You can only add grades for courses you teach.");
            }
            else
            {
                var courseExists = await _db.Courses.AnyAsync(c => c.Id == dto.CourseId);
                if (!courseExists)
                    throw new InvalidOperationException("Course not found.");
            }

            var grade = _mapper.Map<Models.Grade>(dto);
            grade.CreatedDate = DateTime.Now;
            await _db.Grades.AddAsync(grade);
            await _db.SaveChangesAsync();
            return _mapper.Map<GradeReadDto>(grade);
        }

        public async Task<GradeReadDto?> UpdateAsync(int id, GradeUpdateDto dto, string? teacherUserId = null)
        {
            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id);
            if (grade == null) return null;

            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
                if (teacher == null)
                    throw new InvalidOperationException("Teacher profile not found.");

                var ownsCourse = await _db.Courses.AnyAsync(c => c.Id == grade.CourseId && c.TeacherId == teacher.Id);
                if (!ownsCourse)
                    throw new InvalidOperationException("You can only edit grades for courses you teach.");
            }

            _mapper.Map(dto, grade);
            await _db.SaveChangesAsync();
            return _mapper.Map<GradeReadDto>(grade);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id);
            if (grade == null) return false;

            _db.Grades.Remove(grade);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}