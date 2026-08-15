using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Enrollment
{
    public class EnrollmentCreateDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
