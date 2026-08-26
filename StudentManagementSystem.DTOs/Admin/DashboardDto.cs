namespace StudentManagementSystem.DTOs.Admin
{
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalGrades { get; set; }
        public int TotalAttendances { get; set; }
    }
}