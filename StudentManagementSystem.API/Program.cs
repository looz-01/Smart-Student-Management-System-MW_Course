using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StudentManagementSystem.API.Data;
using StudentManagementSystem.API.Mapping;
using StudentManagementSystem.API.Middleware;
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

namespace StudentManagementSystem.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOptions<JwtOptions>()
                .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32,
                    "Jwt:Key must be at least 32 characters. Set it via the Jwt__Key environment variable (or user-secrets) - never commit a real signing key.")
                .ValidateDataAnnotations();

            builder.Services.AddDbContext<AppDbContext>(option =>
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("SystemConn"));
            });

            // Add Identity services to the container.
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = true;

                options.User.RequireUniqueEmail = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Add JWT authentication.
            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                ?? throw new InvalidOperationException("Jwt configuration is missing.");
            if (string.IsNullOrWhiteSpace(jwtOptions.Key))
                throw new InvalidOperationException(
                    "Jwt:Key is not configured. Set the Jwt__Key environment variable (or user-secrets) with a key of at least 32 characters.");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

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
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            builder.Services.AddAuthorization();

            // Rate limiting: protect auth endpoints against brute force.
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("auth", limiter =>
                {
                    limiter.PermitLimit = 10;
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.QueueLimit = 0;
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapping>());

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
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                await DbSeeder.SeedAsync(scope.ServiceProvider, app.Environment);
            }

            // Configure the HTTP request pipeline.
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}