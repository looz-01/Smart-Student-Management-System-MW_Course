using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Extensions;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.DTOs.Attendance;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Services.Attendance
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public AttendanceService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<AttendanceReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null, string? teacherUserId = null)
        {
            request.Normalize();

            var query = _db.Attendances.AsNoTracking();

            if (studentId.HasValue)
                query = query.Where(a => a.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacherCourseIds = GetTeacherCourseIds(teacherUserId);
                query = query.Where(a => teacherCourseIds.Contains(a.CourseId));
            }

            var totalCount = await query.CountAsync();

            var attendances = await query
                .OrderByDescending(a => a.Date)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<AttendanceReadDto>>(attendances), totalCount, request);
        }

        public async Task<AttendanceReadDto?> GetByIdAsync(int id, string? teacherUserId = null)
        {
            var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null) return null;

            if (!string.IsNullOrEmpty(teacherUserId) &&
                !GetTeacherCourseIds(teacherUserId).Contains(attendance.CourseId))
                throw new AppException("You can only view attendance for courses you teach.");

            return _mapper.Map<AttendanceReadDto>(attendance);
        }

        public async Task<PagedResult<AttendanceReadDto>> GetByStudentIdAsync(int studentId, PageRequest request)
        {
            request.Normalize();

            var query = _db.Attendances.AsNoTracking().Where(a => a.StudentId == studentId);

            var totalCount = await query.CountAsync();

            var attendances = await query
                .OrderByDescending(a => a.Date)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<AttendanceReadDto>>(attendances), totalCount, request);
        }

        public async Task<int?> GetStudentIdByUserIdAsync(string userId)
        {
            var student = await _db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
            return student?.Id;
        }

        public async Task<AttendanceReadDto> CreateAsync(AttendanceCreateDto dto, string? teacherUserId = null)
        {
            var studentExists = await _db.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                throw new AppException("Student not found.");

            await EnsureCourseAccessAsync(dto.CourseId, teacherUserId);

            var isEnrolled = await _db.Enrollments
                .AnyAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);
            if (!isEnrolled)
                throw new AppException("Student is not enrolled in this course.");

            // Normalize to a date-only value so the unique index matches a single day.
            var date = DateTime.SpecifyKind(dto.Date.Date, DateTimeKind.Utc);

            var alreadyMarked = await _db.Attendances
                .AnyAsync(a => a.StudentId == dto.StudentId &&
                               a.CourseId == dto.CourseId &&
                               a.Date == date);
            if (alreadyMarked)
                throw new AppException("Attendance is already marked for this student on this day.");

            var attendance = _mapper.Map<Models.Attendance>(dto);
            attendance.Date = date;
            await _db.Attendances.AddAsync(attendance);
            await _db.SaveChangesAsync();
            return _mapper.Map<AttendanceReadDto>(attendance);
        }

        public async Task<AttendanceReadDto?> UpdateAsync(int id, AttendanceUpdateDto dto, string? teacherUserId = null)
        {
            var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null) return null;

            await EnsureCourseAccessAsync(attendance.CourseId, teacherUserId);

            _mapper.Map(dto, attendance);
            await _db.SaveChangesAsync();
            return _mapper.Map<AttendanceReadDto>(attendance);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null) return false;

            _db.Attendances.Remove(attendance);
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
                    throw new AppException("You can only manage attendance for courses you teach.");
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