using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Teacher
{
    public class TeacherCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(5, 120, ErrorMessage = "Age must be between 5 and 120.")]
        public int Age { get; set; }

        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
