using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Course
{
    public class CourseUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Hours must be a positive number.")]
        public int Hours { get; set; }

        public int? TeacherId { get; set; }
    }
}