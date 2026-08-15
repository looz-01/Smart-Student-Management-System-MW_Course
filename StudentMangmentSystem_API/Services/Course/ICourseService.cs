using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Course;

namespace StudentMangmentSystem_API.Services.Course
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