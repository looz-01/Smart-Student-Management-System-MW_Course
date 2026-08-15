using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Admin
{
    public class ChangeRoleDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string NewRole { get; set; } = string.Empty;
    }
}
