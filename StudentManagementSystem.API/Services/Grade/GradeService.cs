using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Extensions;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Grade;

namespace StudentManagementSystem.API.Services.Grade
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

        public async Task<PagedResult<GradeReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null, string? teacherUserId = null)
        {
            request.Normalize();

            var query = _db.Grades.AsNoTracking();

            if (studentId.HasValue)
                query = query.Where(g => g.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(g => g.CourseId == courseId.Value);

            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacherCourseIds = GetTeacherCourseIds(teacherUserId);
                query = query.Where(g => teacherCourseIds.Contains(g.CourseId));
            }

            var totalCount = await query.CountAsync();

            var grades = await query
                .OrderByDescending(g => g.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<GradeReadDto>>(grades), totalCount, request);
        }

        public async Task<GradeReadDto?> GetByIdAsync(int id, string? teacherUserId = null)
        {
            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id);
            if (grade == null) return null;

            if (!string.IsNullOrEmpty(teacherUserId) &&
                !GetTeacherCourseIds(teacherUserId).Contains(grade.CourseId))
                throw new AppException("You can only view grades for courses you teach.");

            return _mapper.Map<GradeReadDto>(grade);
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
                throw new AppException("Student not found.");

            if (dto.Score < 0 || dto.Score > 100)
                throw new AppException("Score must be between 0 and 100.");

            await EnsureCourseAccessAsync(dto.CourseId, teacherUserId);

            var isEnrolled = await _db.Enrollments
                .AnyAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);
            if (!isEnrolled)
                throw new AppException("Student is not enrolled in this course.");

            var gradeExists = await _db.Grades
                .AnyAsync(g => g.StudentId == dto.StudentId && g.CourseId == dto.CourseId);
            if (gradeExists)
                throw new AppException("A grade already exists for this student in this course.");

            var grade = _mapper.Map<Models.Grade>(dto);
            grade.CreatedDate = DateTime.UtcNow;
            await _db.Grades.AddAsync(grade);
            await _db.SaveChangesAsync();
            return _mapper.Map<GradeReadDto>(grade);
        }

        public async Task<GradeReadDto?> UpdateAsync(int id, GradeUpdateDto dto, string? teacherUserId = null)
        {
            var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id);
            if (grade == null) return null;

            if (dto.Score < 0 || dto.Score > 100)
                throw new AppException("Score must be between 0 and 100.");

            await EnsureCourseAccessAsync(grade.CourseId, teacherUserId);

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

        private async Task EnsureCourseAccessAsync(int courseId, string? teacherUserId)
        {
            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
                if (teacher == null)
                    throw new AppException("Teacher profile not found.");

                var ownsCourse = await _db.Courses
                    .AnyAsync(c => c.Id == courseId && c.TeacherId == teacher.Id);
                if (!ownsCourse)
                    throw new AppException("You can only manage grades for courses you teach.");
            }
            else
            {
                var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
                if (!courseExists)
                    throw new AppException("Course not found.");
            }
        }

        private IQueryable<int> GetTeacherCourseIds(string teacherUserId)
        {
            return from c in _db.Courses.AsNoTracking()
                   join t in _db.Teachers.AsNoTracking() on c.TeacherId equals t.Id
                   where t.UserId == teacherUserId
                   select c.Id;
        }
    }
}