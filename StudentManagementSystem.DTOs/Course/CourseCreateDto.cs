using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Course
{
    public class CourseCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Hours { get; set; }

        [Required]
        public int TeacherId { get; set; }
    }
}
