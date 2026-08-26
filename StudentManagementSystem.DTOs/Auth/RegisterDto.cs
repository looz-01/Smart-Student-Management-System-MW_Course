using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Range(5, 120, ErrorMessage = "Age must be between 5 and 120.")]
        public int? Age { get; set; }

        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Specialization { get; set; }
    }
}