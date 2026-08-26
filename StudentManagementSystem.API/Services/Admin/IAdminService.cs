using StudentManagementSystem.DTOs.Admin;
using StudentManagementSystem.DTOs.Common;

namespace StudentManagementSystem.API.Services.Admin
{
    public interface IAdminService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task<PagedResult<UserListDto>> GetUsersAsync(PageRequest request, string? role = null);
        Task<bool> ChangeRoleAsync(ChangeRoleDto dto);
    }
}