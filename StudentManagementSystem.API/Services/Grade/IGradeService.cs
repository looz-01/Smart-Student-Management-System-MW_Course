using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Grade;

namespace StudentManagementSystem.API.Services.Grade
{
    public interface IGradeService
    {
        Task<PagedResult<GradeReadDto>> GetAllAsync(PageRequest request, int? studentId = null, int? courseId = null);
        Task<GradeReadDto?> GetByIdAsync(int id);
        Task<PagedResult<GradeReadDto>> GetByStudentIdAsync(int studentId, PageRequest request);
        Task<int?> GetStudentIdByUserIdAsync(string userId);
        Task<GradeReadDto> CreateAsync(GradeCreateDto dto, string? teacherUserId = null);
        Task<GradeReadDto?> UpdateAsync(int id, GradeUpdateDto dto, string? teacherUserId = null);
        Task<bool> DeleteAsync(int id);
    }
}