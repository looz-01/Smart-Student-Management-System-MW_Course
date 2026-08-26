using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Attendance
{
    public class AttendanceCreateDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }
    }
}
