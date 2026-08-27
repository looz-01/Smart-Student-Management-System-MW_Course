using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentMangmentSystemDTO_s.Common;
using StudentMangmentSystem_API.Services.Admin;
using StudentMangmentSystemDTO_s.Admin;
using StudentMangmentSystemDTO_s.Common;

namespace StudentMangmentSystem_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ResponseApi<DashboardDto>>> GetDashboard()
        {
            var result = await _adminService.GetDashboardAsync();
            return Ok(ResponseApi<DashboardDto>.Ok(result, "Dashboard Retrieved Successfully."));
        }

        [HttpGet("users")]
        public async Task<ActionResult<ResponseApi<PagedResult<UserListDto>>>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null)
        {
            var request = new PageRequest { PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm };
            var result = await _adminService.GetUsersAsync(request, role);
            return Ok(ResponseApi<PagedResult<UserListDto>>.Ok(result, "Users Retrieved Successfully."));
        }

        [HttpPut("change-role")]
        public async Task<ActionResult<ResponseApi<object>>> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseApi<object>.BadRequst("Please Enter Valid Data"));

            var result = await _adminService.ChangeRoleAsync(dto);
            if (!result)
                return BadRequest(ResponseApi<object>.BadRequst("Failed to Change Role."));

            return Ok(ResponseApi<object>.Ok(null, "Role Changed Successfully."));
        }
    }
}