using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Course
{
    public class CourseUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Hours { get; set; }
    }
}
