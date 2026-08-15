namespace StudentMangmentSystemDTO_s.Course
{
    public class CourseReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hours { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TeacherId { get; set; }
    }
}
