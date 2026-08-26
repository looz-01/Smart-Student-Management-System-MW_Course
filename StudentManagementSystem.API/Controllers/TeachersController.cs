using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.API.Services.Teacher;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Teacher;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<PagedResult<TeacherReadDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? specialization = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm };
            var result = await _teacherService.GetAllAsync(request, specialization);
            return Ok(ResponseApi<PagedResult<TeacherReadDto>>.Ok(result, "Teachers Retrieved Successfully."));

        }

        [HttpGet("me")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<ResponseApi<TeacherReadDto>>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ResponseApi<object>.BadRequest("Unauthorized."));

            var teacher = await _teacherService.GetByUserIdAsync(userId);
            if (teacher == null)
                return NotFound(ResponseApi<object>.NotFound("Teacher profile not found."));

            return Ok(ResponseApi<TeacherReadDto>.Ok(teacher, "Teacher Retrieved Successfully."));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<TeacherReadDto>>> GetTeacher(int id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            if (teacher == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Teacher with ID : {id}"));

            return Ok(ResponseApi<TeacherReadDto>.Ok(teacher, "Teacher Retrieved Successfully."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<TeacherReadDto>>> Create(TeacherCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<TeacherReadDto>.BadRequest("Please Enter Valid Data"));

            var result = await _teacherService.CreateAsync(dto);
            return Ok(ResponseApi<TeacherReadDto>.CreatedAt(result, "Teacher Created Successfully."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<TeacherReadDto>>> Update(int id, TeacherUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<TeacherReadDto>.BadRequest("Please Enter Valid Data"));

            var teacher = await _teacherService.UpdateAsync(id, dto);
            if (teacher == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Teacher with ID : {id}"));

            return Ok(ResponseApi<TeacherReadDto>.Ok(teacher, "Teacher Updated Successfully."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<object>>> Delete(int id)
        {
            var deleted = await _teacherService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ResponseApi<object>.NotFound($"There's no Teacher with ID : {id}"));

            return Ok(ResponseApi<object>.NoContent(null, "Teacher Deleted Successfully."));
        }
    }
}