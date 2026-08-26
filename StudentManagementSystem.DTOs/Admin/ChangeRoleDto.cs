using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Admin
{
    public class ChangeRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string NewRole { get; set; } = string.Empty;
    }
}
