using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentManagementSystem.API.Models;

namespace StudentManagementSystem.API.Data
{
    public static class DbSeeder
    {
        public const string RoleAdmin = "Admin";
        public const string RoleTeacher = "Teacher";
        public const string RoleStudent = "Student";

        private const string TeacherPassword = "Teacher@123";
        private const string StudentPassword = "Student@123";

        // Teachers: Name, Email, Specialization, PhoneNumber, Age
        private static readonly (string Name, string Email, string Specialization, string Phone, int Age)[] TeacherData =
        {
            ("Dr. Ahmed Hassan",  "teacher1@school.com", "Mathematics",      "01010000001", 45),
            ("Ms. Sara Mostafa",  "teacher2@school.com", "Computer Science", "01010000002", 34),
            ("Mr. Omar El-Sayed", "teacher3@school.com", "Physics",          "01010000003", 41),
            ("Mrs. Laila Mohamed","teacher4@school.com", "English",          "01010000004", 38),
            ("Mr. Karim Adel",    "teacher5@school.com", "Chemistry",        "01010000005", 47),
            ("Ms. Nour Ali",      "teacher6@school.com", "Biology",          "01010000006", 29),
        };

        // Students: Name, Email, Gender, PhoneNumber, Age
        private static readonly (string Name, string Email, string Gender, string Phone, int Age)[] StudentData =
        {
            ("Adam Khaled",     "student1@school.com",  "Male",   "01110000001", 21),
            ("Youssef Nabil",   "student2@school.com",  "Male",   "01110000002", 20),
            ("Mariam Tarek",    "student3@school.com",  "Female", "01110000003", 22),
            ("Omar Sherif",     "student4@school.com",  "Male",   "01110000004", 19),
            ("Salma Hany",      "student5@school.com",  "Female", "01110000005", 20),
            ("Mostafa Emad",    "student6@school.com",  "Male",   "01110000006", 23),
            ("Hana Mahmoud",    "student7@school.com",  "Female", "01110000007", 21),
            ("Kareem Samir",    "student8@school.com",  "Male",   "01110000008", 22),
            ("Farida Wael",     "student9@school.com",  "Female", "01110000009", 18),
            ("Ziad Ashraf",     "student10@school.com", "Male",   "01110000010", 24),
            ("Laila Gamal",     "student11@school.com", "Female", "01110000011", 20),
            ("Hassan Ali",      "student12@school.com", "Male",   "01110000012", 21),
            ("Nada Fathy",      "student13@school.com", "Female", "01110000013", 19),
            ("Mazen Hossam",    "student14@school.com", "Male",   "01110000014", 22),
            ("Rana Yasser",     "student15@school.com", "Female", "01110000015", 23),
            ("Seif El-Din",     "student16@school.com", "Male",   "01110000016", 20),
            ("Malak Saeed",     "student17@school.com", "Female", "01110000017", 21),
            ("Bishoy George",   "student18@school.com", "Male",   "01110000018", 24),
            ("Nourhan Adel",    "student19@school.com", "Female", "01110000019", 19),
            ("Tarek Refaat",    "student20@school.com", "Male",   "01110000020", 20),
        };

        // Courses: Name, Hours, TeacherIndex (index into TeacherData)
        private static readonly (string Name, int Hours, int TeacherIndex)[] CourseData =
        {
            ("Mathematics 101",          60, 0),
            ("Calculus II",              45, 0),
            ("Introduction to Programming", 48, 1),
            ("Data Structures",          40, 1),
            ("Physics I",                55, 2),
            ("Thermodynamics",           40, 2),
            ("English Composition",      30, 3),
            ("Technical Writing",        30, 3),
            ("Organic Chemistry",        50, 4),
            ("Analytical Chemistry",     35, 4),
            ("Molecular Biology",        52, 5),
            ("Genetics",                 38, 5),
        };

        // Every student is enrolled in this many courses.
        private const int CoursesPerStudent = 4;
        // Offsets used to pick a deterministic set of courses per student (mod course count).
        private static readonly int[] EnrollmentOffsets = { 0, 3, 7, 11 };
        // Number of attendance records generated per enrollment.
        private const int AttendanceDaysPerEnrollment = 10;

        public static async Task SeedAsync(
            IServiceProvider services,
            IHostEnvironment environment)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var db = services.GetRequiredService<AppDbContext>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager, configuration, logger);

            // Demo data is opt-in via configuration (enabled in Development).
            if (configuration.GetValue("Seeding:DemoData", false))
            {
                await SeedDemoDataAsync(db, userManager, logger);
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { RoleAdmin, RoleTeacher, RoleStudent })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private static async Task SeedAdminAsync(
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            ILogger logger)
        {
            var seedSection = configuration.GetSection("SeedAdmin");
            var adminEmail = seedSection["Email"];
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                logger.LogWarning("SeedAdmin:Email is not configured; skipping admin seeding.");
                return;
            }

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return;

            var adminPassword = seedSection["Password"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("SeedAdmin:Password is not configured; skipping admin seeding.");
                return;
            }

            var admin = new AppUser { UserName = adminEmail, Email = adminEmail };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed admin user: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(admin, RoleAdmin);
            logger.LogInformation("Admin user seeded: {Email}", adminEmail);
        }

        private static async Task SeedDemoDataAsync(
            AppDbContext db,
            UserManager<AppUser> userManager,
            ILogger logger)
        {
            // Idempotency guard: if any demo teacher already exists, skip entirely.
            if (await userManager.FindByEmailAsync(TeacherData[0].Email) != null)
            {
                logger.LogInformation("Demo data already seeded; skipping.");
                return;
            }

            var now = DateTime.UtcNow;

            // ---- Teachers + linked user accounts ----
            var teachers = new List<Teacher>(TeacherData.Length);
            foreach (var (name, email, specialization, phone, age) in TeacherData)
            {
                var user = new AppUser { UserName = email, Email = email };
                var result = await userManager.CreateAsync(user, TeacherPassword);
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to seed teacher {Email}: {Errors}",
                        email, string.Join("; ", result.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(user, RoleTeacher);

                var teacher = new Teacher
                {
                    Name = name,
                    UserId = user.Id,
                    Age = age,
                    Specialization = specialization,
                    PhoneNumber = phone
                };
                teachers.Add(teacher);
            }
            await db.Teachers.AddRangeAsync(teachers);
            await db.SaveChangesAsync();

            // ---- Students + linked user accounts ----
            var students = new List<Student>(StudentData.Length);
            foreach (var (name, email, gender, phone, age) in StudentData)
            {
                var user = new AppUser { UserName = email, Email = email };
                var result = await userManager.CreateAsync(user, StudentPassword);
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to seed student {Email}: {Errors}",
                        email, string.Join("; ", result.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(user, RoleStudent);

                var student = new Student
                {
                    Name = name,
                    UserId = user.Id,
                    Age = age,
                    Gender = gender,
                    PhoneNumber = phone
                };
                students.Add(student);
            }
            await db.Students.AddRangeAsync(students);
            await db.SaveChangesAsync();

            // ---- Courses (assigned to seeded teachers) ----
            var courses = new List<Course>(CourseData.Length);
            for (var i = 0; i < CourseData.Length; i++)
            {
                var (name, hours, teacherIndex) = CourseData[i];
                courses.Add(new Course
                {
                    Name = name,
                    Hours = hours,
                    CreatedDate = now.AddMonths(-3).AddDays(i),
                    TeacherId = teachers[teacherIndex].Id
                });
            }
            await db.Courses.AddRangeAsync(courses);
            await db.SaveChangesAsync();

            // ---- Enrollments + Grades + Attendance ----
            var enrollments = new List<Enrollment>();
            var grades = new List<Grade>();
            var attendances = new List<Attendance>();

            for (var s = 0; s < students.Count; s++)
            {
                var student = students[s];
                var courseCount = CourseData.Length;

                for (var k = 0; k < CoursesPerStudent; k++)
                {
                    var course = courses[EnrollmentOffsets[k] % courseCount];

                    // Enrollments are keyed by (StudentId, CourseId), so skip duplicates.
                    if (enrollments.Any(e => e.StudentId == student.Id && e.CourseId == course.Id))
                        continue;

                    enrollments.Add(new Enrollment
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        EnrollmentDate = now.AddMonths(-2).AddDays(s + k)
                    });

                    grades.Add(new Grade
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        Score = 55 + ((s * 13 + course.Id * 7) % 46),
                        CreatedDate = now.AddDays(-14).AddHours(s % 12)
                    });

                    // One attendance record per enrollment, every other day, going back.
                    for (var d = 0; d < AttendanceDaysPerEnrollment; d++)
                    {
                        attendances.Add(new Attendance
                        {
                            StudentId = student.Id,
                            CourseId = course.Id,
                            Date = now.AddDays(-(d * 2) - (s % 3)).Date,
                            IsPresent = (s + course.Id + d) % 10 != 0
                        });
                    }
                }
            }

            await db.Enrollments.AddRangeAsync(enrollments);
            await db.Grades.AddRangeAsync(grades);
            await db.Attendances.AddRangeAsync(attendances);
            await db.SaveChangesAsync();

            logger.LogInformation(
                "Demo data seeded: {TeacherCount} teachers, {StudentCount} students, {CourseCount} courses, " +
                "{EnrollmentCount} enrollments, {GradeCount} grades, {AttendanceCount} attendance records.",
                teachers.Count, students.Count, courses.Count, enrollments.Count, grades.Count, attendances.Count);
        }
    }
}
