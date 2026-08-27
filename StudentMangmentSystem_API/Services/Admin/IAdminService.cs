using StudentMangmentSystemDTO_s.Admin;
using StudentMangmentSystemDTO_s.Common;

namespace StudentMangmentSystem_API.Services.Admin
{
    public interface IAdminService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task<PagedResult<UserListDto>> GetUsersAsync(PageRequest request, string? role = null);
        Task<bool> ChangeRoleAsync(ChangeRoleDto dto);
    }
}