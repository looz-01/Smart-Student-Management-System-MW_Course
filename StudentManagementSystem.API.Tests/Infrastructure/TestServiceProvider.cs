using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using StudentManagementSystem.API.Data;
using StudentManagementSystem.API.Mapping;
using StudentManagementSystem.API.Models;
using StudentManagementSystem.API.Options;
using StudentManagementSystem.API.Services.Admin;
using StudentManagementSystem.API.Services.Attendance;
using StudentManagementSystem.API.Services.Auth;
using StudentManagementSystem.API.Services.Course;
using StudentManagementSystem.API.Services.Enrollment;
using StudentManagementSystem.API.Services.Grade;
using StudentManagementSystem.API.Services.ImageService;
using StudentManagementSystem.API.Services.Student;
using StudentManagementSystem.API.Services.Teacher;

namespace StudentManagementSystem.API.Tests.Infrastructure
{
    public sealed class TestServiceProvider : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly SqliteConnection _connection;

        public TestServiceProvider()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddOptions<JwtOptions>().Configure(options =>
            {
                options.Key = "TestSigningKeyThatIsLongEnough1234567890";
                options.Issuer = "TestIssuer";
                options.Audience = "TestAudience";
                options.DurationInMinutes = 60;
                options.RefreshTokenDurationInDays = 7;
            });

            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapping>());

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());

            _provider = services.BuildServiceProvider();

            var db = _provider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            SeedRoles().GetAwaiter().GetResult();
        }

        public T GetService<T>() where T : notnull
            => _provider.GetRequiredService<T>();

        public AppDbContext Db => GetService<AppDbContext>();

        public UserManager<AppUser> UserManager => GetService<UserManager<AppUser>>();

        private async Task SeedRoles()
        {
            var roleManager = GetService<RoleManager<IdentityRole>>();
            foreach (var role in new[] { DbSeeder.RoleAdmin, DbSeeder.RoleTeacher, DbSeeder.RoleStudent })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public async Task<AppUser> CreateUserAsync(string email, string password, string role)
        {
            var user = new AppUser { UserName = email, Email = email };
            var result = await UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to create test user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            await UserManager.AddToRoleAsync(user, role);
            return user;
        }

        public void Dispose()
        {
            _provider.Dispose();
            _connection.Dispose();
        }

        private sealed class TestWebHostEnvironment : IWebHostEnvironment
        {
            public string ApplicationName { get; set; } = "TestApp";
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
            public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "smstest-wwwroot");
            public string EnvironmentName { get; set; } = "Development";
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}