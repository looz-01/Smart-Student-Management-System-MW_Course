using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Enrollment;

namespace StudentMangmentSystem_API.Services.Enrollment
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