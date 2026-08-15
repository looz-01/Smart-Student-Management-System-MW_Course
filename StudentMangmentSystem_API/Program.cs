
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StudentMangmentSystem_API.Mapping;
using StudentMangmentSystem_API.Models;
using StudentMangmentSystem_API.Services.Admin;
using StudentMangmentSystem_API.Services.Attendance;
using StudentMangmentSystem_API.Services.Auth;
using StudentMangmentSystem_API.Services.Course;
using StudentMangmentSystem_API.Services.Enrollment;
using StudentMangmentSystem_API.Services.Grade;
using StudentMangmentSystem_API.Services.ImageService;
using StudentMangmentSystem_API.Services.Student;
using StudentMangmentSystem_API.Services.Teacher;

namespace StudentMangmentSystem_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(option =>
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("SystemConn"));
            });

            // Add Identity services to the container.
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Add JWT authentication.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });

            builder.Services.AddAutoMapper(typeof(AutoMapping));

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                // Register the JWT Bearer security scheme so Scalar shows the Authorize button.
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Paste the JWT token returned by POST /api/Auth/login (or /api/Auth/register)."
                    };
                    return Task.CompletedTask;
                });

                // Mark only the endpoints that require [Authorize] as secured,
                // so anonymous endpoints (register/login/refresh) stay unlocked.
                options.AddOperationTransformer((operation, context, cancellationToken) =>
                {
                    var requiresAuth = context.Description?.ActionDescriptor?.EndpointMetadata?
                        .OfType<IAuthorizeData>()
                        .Any() ?? false;

                    if (requiresAuth)
                    {
                        operation.Security = new List<OpenApiSecurityRequirement>
                        {
                            new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference("Bearer", context.Document, null)] = new List<string>()
                            }
                        };
                    }

                    return Task.CompletedTask;
                });
            });

            // Register application services.
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ITeacherService, TeacherService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IGradeService, GradeService>();
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<IImageService, ImageService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                foreach (var role in new[] { "Admin", "Teacher", "Student" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                const string adminEmail = "admin@school.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var admin = new AppUser { UserName = adminEmail, Email = adminEmail };
                    var adminResult = await userManager.CreateAsync(admin, "Admin@123456");
                    if (adminResult.Succeeded)
                        await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
