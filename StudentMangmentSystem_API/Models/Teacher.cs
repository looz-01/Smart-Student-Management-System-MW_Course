using System.ComponentModel.DataAnnotations;

namespace StudentMangmentSystem_API.Models
{
    public class Teacher
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int Age { get; set; }
        public string Specialization { get; set; }
        public string PhoneNumber { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public ICollection<Course>? Courses { get; set; }
    }
}
