using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Attendance;
using StudentManagementSystem.DTOs.Attendance;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendancesController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendancesController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

[HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<PagedResult<AttendanceReadDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var teacherUserId = User.IsInRole("Teacher")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var result = await _attendanceService.GetAllAsync(request, studentId, courseId, teacherUserId);
            return Ok(ResponseApi<PagedResult<AttendanceReadDto>>.Ok(result, "Attendances Retrieved Successfully."));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ResponseApi<PagedResult<AttendanceReadDto>>>> GetMyAttendance(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ResponseApi<object>.BadRequest("Unauthorized."));

            var studentId = await _attendanceService.GetStudentIdByUserIdAsync(userId);
            if (studentId == null)
                return NotFound(ResponseApi<object>.NotFound("Student profile not found."));

            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
            var result = await _attendanceService.GetByStudentIdAsync(studentId.Value, request);
            return Ok(ResponseApi<PagedResult<AttendanceReadDto>>.Ok(result, "Attendance Retrieved Successfully."));
        }

[HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<AttendanceReadDto>>> GetAttendance(int id)
        {
            var teacherUserId = User.IsInRole("Teacher")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            AttendanceReadDto? attendance;
            try
            {
                attendance = await _attendanceService.GetByIdAsync(id, teacherUserId);
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<AttendanceReadDto>.BadRequest(ex.Message));
            }

            if (attendance == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Attendance with ID : {id}"));

            return Ok(ResponseApi<AttendanceReadDto>.Ok(attendance, "Attendance Retrieved Successfully."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<AttendanceReadDto>>> Create(AttendanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<AttendanceReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var teacherUserId = User.IsInRole("Teacher")
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : null;

                var result = await _attendanceService.CreateAsync(dto, teacherUserId);
                return Ok(ResponseApi<AttendanceReadDto>.CreatedAt(result, "Attendance Marked Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<AttendanceReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ResponseApi<AttendanceReadDto>>> Update(int id, AttendanceUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<AttendanceReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var teacherUserId = User.IsInRole("Teacher")
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : null;

                var attendance = await _attendanceService.UpdateAsync(id, dto, teacherUserId);
                if (attendance == null)
                    return NotFound(ResponseApi<object>.NotFound($"There's no Attendance with ID : {id}"));

                return Ok(ResponseApi<AttendanceReadDto>.Ok(attendance, "Attendance Updated Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<AttendanceReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<object>>> Delete(int id)
        {
            var deleted = await _attendanceService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ResponseApi<object>.NotFound($"There's no Attendance with ID : {id}"));

            return Ok(ResponseApi<object>.NoContent(null, "Attendance Deleted Successfully."));
        }
    }
}