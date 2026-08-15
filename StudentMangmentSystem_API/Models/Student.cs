namespace StudentMangmentSystem_API.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int Age { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<Grade>? Grades { get; set; }
        public ICollection<Attendance>? Attendances { get; set; }



    }
}