using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StudentMangmentSystem_API.Extensions;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystem_API.Services.ImageService;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystemDTO_s.Student;

namespace StudentMangmentSystem_API.Services.Student
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public StudentService(AppDbContext db, IMapper mapper, IImageService imageService)
        {
            _db = db;
            _mapper = mapper;
            _imageService = imageService;
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
            var student = _mapper.Map<Models.Student>(dto);
            await _db.Students.AddAsync(student);
            await _db.SaveChangesAsync();
            return _mapper.Map<StudentReadDto>(student);
        }

        public async Task<StudentReadDto?> UpdateAsync(int id, StudentUpdateDto dto)
        {
            if (dto.Id != id)
                throw new InvalidOperationException("ID mismatch.");

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return null;

            var duplicate = await _db.Students.FirstOrDefaultAsync(x => x.Name.ToLower() == dto.Name.ToLower() && x.Id != id);
            if (duplicate != null)
                throw new InvalidOperationException("Duplicate name.");

            _mapper.Map(dto, student);
            await _db.SaveChangesAsync();
            return _mapper.Map<StudentReadDto>(student);
        }

        public async Task<StudentReadDto?> UploadPhotoAsync(int id, IFormFile file)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return null;

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