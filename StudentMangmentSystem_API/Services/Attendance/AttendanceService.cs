using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Attendance;
using StudentMangmentSystemDTO_s.Common;

namespace StudentMangmentSystem_API.Services.Attendance
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

        public async Task<PagedResult<AttendanceReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null)
        {
            request.Normalize();

            var query = _db.Attendances.AsNoTracking();

            if (studentId.HasValue)
                query = query.Where(a => a.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            var totalCount = await query.CountAsync();

            var attendances = await query
                .OrderByDescending(a => a.Date)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<AttendanceReadDto>>(attendances), totalCount, request);
        }

        public async Task<AttendanceReadDto?> GetByIdAsync(int id)
        {
            var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            return attendance == null ? null : _mapper.Map<AttendanceReadDto>(attendance);
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
                throw new InvalidOperationException("Student not found.");

            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
                if (teacher == null)
                    throw new InvalidOperationException("Teacher profile not found.");

                var ownsCourse = await _db.Courses.AnyAsync(c => c.Id == dto.CourseId && c.TeacherId == teacher.Id);
                if (!ownsCourse)
                    throw new InvalidOperationException("You can only mark attendance for courses you teach.");
            }
            else
            {
                var courseExists = await _db.Courses.AnyAsync(c => c.Id == dto.CourseId);
                if (!courseExists)
                    throw new InvalidOperationException("Course not found.");
            }

            var attendance = _mapper.Map<Models.Attendance>(dto);
            await _db.Attendances.AddAsync(attendance);
            await _db.SaveChangesAsync();
            return _mapper.Map<AttendanceReadDto>(attendance);
        }

        public async Task<AttendanceReadDto?> UpdateAsync(int id, AttendanceUpdateDto dto, string? teacherUserId = null)
        {
            var attendance = await _db.Attendances.FirstOrDefaultAsync(a => a.Id == id);
            if (attendance == null) return null;

            if (!string.IsNullOrEmpty(teacherUserId))
            {
                var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.UserId == teacherUserId);
                if (teacher == null)
                    throw new InvalidOperationException("Teacher profile not found.");

                var ownsCourse = await _db.Courses.AnyAsync(c => c.Id == attendance.CourseId && c.TeacherId == teacher.Id);
                if (!ownsCourse)
                    throw new InvalidOperationException("You can only edit attendance for courses you teach.");
            }

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
    }
}