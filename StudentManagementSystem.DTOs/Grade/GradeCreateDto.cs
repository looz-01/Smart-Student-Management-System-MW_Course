using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Grade
{
    public class GradeCreateDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public double Score { get; set; }
    }
}