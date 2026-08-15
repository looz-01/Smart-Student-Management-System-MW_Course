using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Enrollment;

namespace StudentMangmentSystem_API.Services.Enrollment
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public EnrollmentService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<EnrollmentReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null)
        {
            request.Normalize();

            var query = _db.Enrollments.AsNoTracking();

            if (studentId.HasValue)
                query = query.Where(e => e.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(e => e.CourseId == courseId.Value);

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .OrderByDescending(e => e.EnrollmentDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<EnrollmentReadDto>>(enrollments), totalCount, request);
        }

        public async Task<PagedResult<EnrollmentReadDto>> GetByStudentIdAsync(int studentId, PageRequest request)
        {
            request.Normalize();

            var query = _db.Enrollments.AsNoTracking().Where(e => e.StudentId == studentId);

            var totalCount = await query.CountAsync();

            var enrollments = await query
                .OrderByDescending(e => e.EnrollmentDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<EnrollmentReadDto>>(enrollments), totalCount, request);
        }

        public async Task<EnrollmentReadDto?> GetByIdAsync(int studentId, int courseId)
        {
            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            return enrollment == null ? null : _mapper.Map<EnrollmentReadDto>(enrollment);
        }

        public async Task<EnrollmentReadDto> CreateAsync(EnrollmentCreateDto dto)
        {
            var studentExists = await _db.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                throw new InvalidOperationException("Student not found.");

            var courseExists = await _db.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                throw new InvalidOperationException("Course not found.");

            var alreadyEnrolled = await _db.Enrollments.AnyAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);
            if (alreadyEnrolled)
                throw new InvalidOperationException("Student is already enrolled in this course.");

            var enrollment = _mapper.Map<Models.Enrollment>(dto);
            enrollment.EnrollmentDate = DateTime.Now;
            await _db.Enrollments.AddAsync(enrollment);
            await _db.SaveChangesAsync();
            return _mapper.Map<EnrollmentReadDto>(enrollment);
        }

        public async Task<bool> DeleteAsync(int studentId, int courseId)
        {
            var enrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (enrollment == null) return false;

            _db.Enrollments.Remove(enrollment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}