namespace StudentManagementSystem.API.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }
    }
}