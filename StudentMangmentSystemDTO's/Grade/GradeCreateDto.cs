using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Grade
{
    public class GradeCreateDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public double Score { get; set; }
    }
}
