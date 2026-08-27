using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystemDTO_s.Student
{
    public class StudentUpdateDto
    {
        [Required]

        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
