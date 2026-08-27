namespace StudentMangmentSystemDTO_s.Attendance
{
    public class AttendanceReadDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
    }
}
