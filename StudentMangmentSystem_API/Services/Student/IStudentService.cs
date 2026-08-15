using Microsoft.AspNetCore.Http;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Student;

namespace StudentMangmentSystem_API.Services.Student
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