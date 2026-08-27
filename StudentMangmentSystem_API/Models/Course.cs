namespace StudentMangmentSystem_API.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Hours { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<Grade>? Grades { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<Attendance>? Attendances { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
    }
}
