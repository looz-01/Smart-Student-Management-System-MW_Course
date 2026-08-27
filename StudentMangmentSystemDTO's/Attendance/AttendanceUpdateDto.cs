using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Attendance
{
    public class AttendanceUpdateDto
    {
        [Required]
        public bool IsPresent { get; set; }
    }
}
