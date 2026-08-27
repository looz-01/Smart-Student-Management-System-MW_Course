using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Student
{
    public class StudentUpdateDto
    {
        [Required]

        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

[Required]
        [Range(5, 120, ErrorMessage = "Age must be between 5 and 120.")]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
