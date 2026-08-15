using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Course
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
