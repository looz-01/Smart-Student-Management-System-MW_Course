using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Enrollment;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Enrollment;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseApi<PagedResult<EnrollmentReadDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var result = await _enrollmentService.GetAllAsync(request, studentId, courseId);
            return Ok(ResponseApi<PagedResult<EnrollmentReadDto>>.Ok(result, "Enrollments Retrieved Successfully."));
        }

        [HttpGet("student/{studentId:int}")]
        public async Task<ActionResult<ResponseApi<PagedResult<EnrollmentReadDto>>>> GetByStudent(
            int studentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var result = await _enrollmentService.GetByStudentIdAsync(studentId, request);
            return Ok(ResponseApi<PagedResult<EnrollmentReadDto>>.Ok(result, "Enrollments Retrieved Successfully."));
        }

        [HttpPost]
        public async Task<ActionResult<ResponseApi<EnrollmentReadDto>>> Create(EnrollmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<EnrollmentReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var result = await _enrollmentService.CreateAsync(dto);
                return Ok(ResponseApi<EnrollmentReadDto>.CreatedAt(result, "Enrollment Created Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<EnrollmentReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpDelete("{studentId:int}/{courseId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<object>>> Delete(int studentId, int courseId)
        {
            var deleted = await _enrollmentService.DeleteAsync(studentId, courseId);
            if (!deleted)
                return NotFound(ResponseApi<object>.NotFound("Enrollment not found."));

            return Ok(ResponseApi<object>.NoContent(null, "Enrollment Deleted Successfully."));
        }
    }
}