using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Course;

namespace StudentManagementSystem.API.Services.Course
{
    public interface ICourseService
    {
        Task<PagedResult<CourseReadDto>> GetAllAsync(PageRequest request, int? teacherId = null);
        Task<CourseReadDto?> GetByIdAsync(int id);
        Task<CourseReadDto> CreateAsync(CourseCreateDto dto);
        Task<CourseReadDto?> UpdateAsync(int id, CourseUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}