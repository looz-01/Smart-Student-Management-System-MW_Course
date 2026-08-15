using StudentMangmentSystemDTO_s.Attendance;
using StudentMangmentSystemDTO_s.Common;

namespace StudentMangmentSystem_API.Services.Attendance
{
    public interface IAttendanceService
    {
        Task<PagedResult<AttendanceReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null);
        Task<AttendanceReadDto?> GetByIdAsync(int id);
        Task<PagedResult<AttendanceReadDto>> GetByStudentIdAsync(int studentId, PageRequest request);
        Task<int?> GetStudentIdByUserIdAsync(string userId);
        Task<AttendanceReadDto> CreateAsync(AttendanceCreateDto dto, string? teacherUserId = null);
        Task<AttendanceReadDto?> UpdateAsync(int id, AttendanceUpdateDto dto, string? teacherUserId = null);
        Task<bool> DeleteAsync(int id);
    }
}