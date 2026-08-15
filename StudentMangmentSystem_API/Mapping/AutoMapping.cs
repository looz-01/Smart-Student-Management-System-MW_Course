using AutoMapper;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystemDTO_s.Grade;
using StudentMangmentSystemDTO_s.Attendance;
using StudentMangmentSystemDTO_s.Course;
using StudentMangmentSystemDTO_s.Enrollment;
using StudentMangmentSystemDTO_s.Student;
using StudentMangmentSystemDTO_s.Teacher;

namespace StudentMangmentSystem_API.Mapping
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            // Student
            CreateMap<StudentCreateDto, Student>().ReverseMap();
            CreateMap<StudentUpdateDto, Student>().ReverseMap();
            CreateMap<Student, StudentReadDto>().ReverseMap();
            CreateMap<Student, StudentDto>().ReverseMap();



            // Teacher
            CreateMap<TeacherCreateDto, Teacher>().ReverseMap();
            CreateMap<TeacherUpdateDto, Teacher>().ReverseMap();
            CreateMap<Teacher, TeacherReadDto>().ReverseMap();

            // Course
            CreateMap<CourseCreateDto, Course>().ReverseMap();
            CreateMap<CourseUpdateDto, Course>().ReverseMap();
            CreateMap<Course, CourseReadDto>().ReverseMap();

            // Enrollment
            CreateMap<EnrollmentCreateDto, Enrollment>().ReverseMap();
            CreateMap<Enrollment, EnrollmentReadDto>().ReverseMap();

            // Grade
            CreateMap<GradeCreateDto, Grade>().ReverseMap();
            CreateMap<GradeUpdateDto, Grade>().ReverseMap();
            CreateMap<Grade, GradeReadDto>().ReverseMap();

            // Attendance
            CreateMap<AttendanceCreateDto, Attendance>().ReverseMap();
            CreateMap<AttendanceUpdateDto, Attendance>().ReverseMap();
            CreateMap<Attendance, AttendanceReadDto>().ReverseMap();
        }
    }
}
