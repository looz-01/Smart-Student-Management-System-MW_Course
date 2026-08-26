using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StudentManagementSystem.API.Models;

namespace StudentManagementSystem.API.Data
{
    public static class DbSeeder
    {
        public const string RoleAdmin = "Admin";
        public const string RoleTeacher = "Teacher";
        public const string RoleStudent = "Student";

        public static async Task SeedAsync(
            IServiceProvider services,
            IHostEnvironment environment)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

            foreach (var role in new[] { RoleAdmin, RoleTeacher, RoleStudent })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

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
    }
}