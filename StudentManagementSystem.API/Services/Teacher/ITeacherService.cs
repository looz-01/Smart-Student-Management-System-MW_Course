using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Teacher;

namespace StudentManagementSystem.API.Services.Teacher
{
    public interface ITeacherService
    {
        Task<PagedResult<TeacherReadDto>> GetAllAsync(PageRequest request, string? specialization = null);
        Task<TeacherReadDto?> GetByIdAsync(int id);
        Task<TeacherReadDto?> GetByUserIdAsync(string userId);
        Task<TeacherReadDto> CreateAsync(TeacherCreateDto dto);
        Task<TeacherReadDto?> UpdateAsync(int id, TeacherUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}