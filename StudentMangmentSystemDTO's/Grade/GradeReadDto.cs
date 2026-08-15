namespace StudentMangmentSystemDTO_s.Grade
{
    public class GradeReadDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public double Score { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
