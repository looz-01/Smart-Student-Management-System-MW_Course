using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.API.Common;
using StudentManagementSystem.API.Services.Course;
using StudentManagementSystem.DTOs.Common;
using StudentManagementSystem.DTOs.Course;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseApi<PagedResult<CourseReadDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? teacherId = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm };
            var result = await _courseService.GetAllAsync(request, teacherId);
            return Ok(ResponseApi<PagedResult<CourseReadDto>>.Ok(result, "Courses Retrieved Successfully."));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResponseApi<CourseReadDto>>> GetCourse(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Course with ID : {id}"));

            return Ok(ResponseApi<CourseReadDto>.Ok(course, "Course Retrieved Successfully."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<CourseReadDto>>> Create(CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<CourseReadDto>.BadRequest("Please Enter Valid Data"));

            try
            {
                var result = await _courseService.CreateAsync(dto);
                return Ok(ResponseApi<CourseReadDto>.CreatedAt(result, "Course Created Successfully."));
            }
            catch (AppException ex)
            {
                return BadRequest(ResponseApi<CourseReadDto>.BadRequest(ex.Message));
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<CourseReadDto>>> Update(int id, CourseUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<CourseReadDto>.BadRequest("Please Enter Valid Data"));

            var course = await _courseService.UpdateAsync(id, dto);
            if (course == null)
                return NotFound(ResponseApi<object>.NotFound($"There's no Course with ID : {id}"));

            return Ok(ResponseApi<CourseReadDto>.Ok(course, "Course Updated Successfully."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseApi<object>>> Delete(int id)
        {
            var deleted = await _courseService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ResponseApi<object>.NotFound($"There's no Course with ID : {id}"));

            return Ok(ResponseApi<object>.NoContent(null, "Course Deleted Successfully."));
        }
    }
}