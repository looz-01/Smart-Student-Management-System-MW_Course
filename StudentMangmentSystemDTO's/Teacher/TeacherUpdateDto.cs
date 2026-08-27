using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Teacher
{
    public class TeacherUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
