using AutoMapper;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.DTOs.Grade;
using StudentManagementSystem.DTOs.Attendance;
using StudentManagementSystem.DTOs.Course;
using StudentManagementSystem.DTOs.Enrollment;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.DTOs.Teacher;

namespace StudentManagementSystem.API.Mapping
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
            CreateMap<CourseUpdateDto, Course>()
                .ForMember(c => c.TeacherId, opt => opt.Condition(src => src.TeacherId.HasValue));
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
