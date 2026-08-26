using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Enrollment;

namespace StudentManagementSystem.API.Services.Enrollment
{
    public interface IEnrollmentService
    {
        Task<PagedResult<EnrollmentReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null);
        Task<PagedResult<EnrollmentReadDto>> GetByStudentIdAsync(int studentId, PageRequest request);
        Task<EnrollmentReadDto?> GetByIdAsync(int studentId, int courseId);
        Task<EnrollmentReadDto> CreateAsync(EnrollmentCreateDto dto);
        Task<bool> DeleteAsync(int studentId, int courseId);
    }
}