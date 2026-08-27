using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Grade;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Grade;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

[HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<PagedResult<GradeReadDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var teacherUserId = User.IsInRole("Teacher")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var result = await _gradeService.GetAllAsync(request, studentId, courseId, teacherUserId);
            return Ok(ResponseApi<PagedResult<GradeReadDto>>.Ok(result, "Grades Retrieved Successfully."));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ResponseApi<PagedResult<GradeReadDto>>>> GetMyGrades(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ResponseApi<object>.BadRequest("Unauthorized."));

            var student = await _gradeService.GetStudentIdByUserIdAsync(userId);
            if (student == null)
                return NotFound(ResponseApi<object>.NotFound("Student profile not found."));

            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var result = await _gradeService.GetByStudentIdAsync(student.Value, request);
            return Ok(ResponseApi<PagedResult<GradeReadDto>>.Ok(result, "Grades Retrieved Successfully."));
        }

[HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<GradeReadDto>>> GetGrade(int id)
        {
            var teacherUserId = User.IsInRole("Teacher")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            GradeReadDto? grade;
            try
            {
                grade = await _gradeService.GetByIdAsync(id, teacherUserId);
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<GradeReadDto>.BadRequest(ex.Message));
            }

            if (grade == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Grade with ID : {id}"));

            return Ok(ResponseApi<GradeReadDto>.Ok(grade, "Grade Retrieved Successfully."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<GradeReadDto>>> Create(GradeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<GradeReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var teacherUserId = User.IsInRole("Teacher")
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : null;

                var result = await _gradeService.CreateAsync(dto, teacherUserId);
                return Ok(ResponseApi<GradeReadDto>.CreatedAt(result, "Grade Created Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<GradeReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<GradeReadDto>>> Update(int id, GradeUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<GradeReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var teacherUserId = User.IsInRole("Teacher")
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : null;

                var grade = await _gradeService.UpdateAsync(id, dto, teacherUserId);
                if (grade == null)
                    return NotFound(ResponseApi<object>.NotFound($"There's no Grade with ID : {id}"));

                return Ok(ResponseApi<GradeReadDto>.Ok(grade, "Grade Updated Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<GradeReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<object>>> Delete(int id)
        {
            var deleted = await _gradeService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ResponseApi<object>.NotFound($"There's no Grade with ID : {id}"));

            return Ok(ResponseApi<object>.NoContent(null, "Grade Deleted Successfully."));
        }
    }
}