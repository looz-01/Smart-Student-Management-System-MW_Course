namespace StudentManagementSystem.API.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public double Score { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}