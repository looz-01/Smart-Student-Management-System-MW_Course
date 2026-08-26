using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Attendance
{
    public class AttendanceUpdateDto
    {
        [Required]
        public bool IsPresent { get; set; }
    }
}
