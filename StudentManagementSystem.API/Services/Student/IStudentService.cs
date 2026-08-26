using Microsoft.AspNetCore.Http;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Student;

namespace StudentManagementSystem.API.Services.Student
{
    public interface IStudentService
    {
        Task<PagedResult<StudentReadDto>> GetAllAsync(PageRequest request, string? gender = null);
        Task<StudentReadDto?> GetByIdAsync(int id);
        Task<StudentReadDto?> GetByUserIdAsync(string userId);
        Task<StudentReadDto> CreateAsync(StudentCreateDto dto);
        Task<StudentReadDto?> UpdateAsync(int id, StudentUpdateDto dto);
        Task<StudentReadDto?> UploadPhotoAsync(int id, IFormFile file);
        Task<bool> DeleteAsync(int id);
    }
}