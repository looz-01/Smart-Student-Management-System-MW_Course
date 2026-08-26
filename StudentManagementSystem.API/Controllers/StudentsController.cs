using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Student;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Student;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<PagedResult<StudentReadDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? gender = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm };
            var result = await _studentService.GetAllAsync(request, gender);
            return Ok(ResponseApi<PagedResult<StudentReadDto>>.Ok(result, "Students Retrieved Successfully."));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ResponseApi<StudentReadDto>>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ResponseApi<object>.BadRequest("Unauthorized."));

            var student = await _studentService.GetByUserIdAsync(userId);
            if (student == null)
                return NotFound(ResponseApi<object>.NotFound("Student profile not found."));

            return Ok(ResponseApi<StudentReadDto>.Ok(student, "Student Retrieved Successfully."));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<StudentReadDto>>> GetStudent(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Student with ID : {id}"));

            return Ok(ResponseApi<StudentReadDto>.Ok(student, "Student Retrieved Successfully."));
        }

[HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<StudentReadDto>>> Create(StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<StudentReadDto>.BadRequest("Please Enter Valid Data"));

            var result = await _studentService.CreateAsync(dto);
            return Ok(ResponseApi<StudentReadDto>.CreatedAt(result, "Student Created Successfully."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<StudentReadDto>>> Update(int id, StudentUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<StudentReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var student = await _studentService.UpdateAsync(id, dto);
                if (student == null)
                    return NotFound(ResponseApi<object>.NotFound($"There's no Student with ID : {id}"));

                return Ok(ResponseApi<StudentReadDto>.Ok(student, "Student Updated Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<StudentReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpPost("{id:int}/photo")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<StudentReadDto>>> UploadPhoto(int id, IFormFile file)
        {
            try
            {
                var student = await _studentService.UploadPhotoAsync(id, file);
                if (student == null)
                    return NotFound(ResponseApi<object>.NotFound($"There's no Student with ID : {id}"));

                return Ok(ResponseApi<StudentReadDto>.Ok(student, "Photo Uploaded Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<StudentReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<object>>> Delete(int id)
        {
            var deleted = await _studentService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ResponseApi<object>.NotFound($"There's no Student with ID : {id}"));

            return Ok(ResponseApi<object>.NoContent(null, "Student Deleted Successfully."));
        }
    }
}