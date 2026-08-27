using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Data;
using StudentManagementSystem.API.Extensions;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.API.Services.ImageService;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Student;

namespace StudentManagementSystem.API.Services.Student
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly UserManager<AppUser> _userManager;

        public StudentService(AppDbContext db, IMapper mapper, IImageService imageService, UserManager<AppUser> userManager)
        {
            _db = db;
            _mapper = mapper;
            _imageService = imageService;
            _userManager = userManager;
        }

        public async Task<PagedResult<StudentReadDto>> GetAllAsync(PageRequest request, string? gender = null)
        {
            request.Normalize();

            var query = _db.Students.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(s =>
                    s.Name.Contains(request.SearchTerm) ||
                    s.PhoneNumber.Contains(request.SearchTerm));

            if (!string.IsNullOrWhiteSpace(gender))
                query = query.Where(s => s.Gender.Equals(gender, StringComparison.OrdinalIgnoreCase));

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return PagedResultFactory.Create(_mapper.Map<List<StudentReadDto>>(students), totalCount, request);
        }

        public async Task<StudentReadDto?> GetByIdAsync(int id)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            return student == null ? null : _mapper.Map<StudentReadDto>(student);
        }

        public async Task<StudentReadDto?> GetByUserIdAsync(string userId)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            return student == null ? null : _mapper.Map<StudentReadDto>(student);
        }

        public async Task<StudentReadDto> CreateAsync(StudentCreateDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                throw new AppException("The linked user does not exist.");

            var isStudentRole = await _userManager.IsInRoleAsync(user, DbSeeder.RoleStudent);
            if (!isStudentRole)
                throw new AppException("The linked user is not registered as a Student.");

            var alreadyLinked = await _db.Students.AnyAsync(s => s.UserId == dto.UserId);
            if (alreadyLinked)
                throw new AppException("A student profile is already linked to this user.");

            var duplicate = await _db.Students.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());
            if (duplicate)
                throw new AppException("A student with this name already exists.");

            var student = _mapper.Map<Models.Student>(dto);
            await _db.Students.AddAsync(student);
            await _db.SaveChangesAsync();
            return _mapper.Map<StudentReadDto>(student);
        }

        public async Task<StudentReadDto?> UpdateAsync(int id, StudentUpdateDto dto)
        {
            if (dto.Id != id)
                throw new AppException("ID mismatch.");

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return null;

            var duplicate = await _db.Students
                .FirstOrDefaultAsync(x => x.Name.ToLower() == dto.Name.ToLower() && x.Id != id);
            if (duplicate != null)
                throw new AppException("A student with this name already exists.");

            _mapper.Map(dto, student);
            await _db.SaveChangesAsync();
            return _mapper.Map<StudentReadDto>(student);
        }

        public async Task<StudentReadDto?> UploadPhotoAsync(int id, IFormFile file)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return null;

            if (file == null || file.Length == 0)
                throw new AppException("No image file was provided.");

            var url = await _imageService.SaveImageAsync(file, "uploads/students");
            if (url == null) return _mapper.Map<StudentReadDto>(student);

            await _imageService.DeleteImageAsync(student.ProfilePhotoUrl);
            student.ProfilePhotoUrl = url;
            await _db.SaveChangesAsync();

            return _mapper.Map<StudentReadDto>(student);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return false;

            await _imageService.DeleteImageAsync(student.ProfilePhotoUrl);
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}